using Autofac;
using Autofac.Core;
using BenMakesGames.PlayPlayMini.Attributes.DI;
using BenMakesGames.PlayPlayMini.Model;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace BenMakesGames.PlayPlayMini;

public class GameStateManagerBuilder
{
    private Type? InitialGameState { get; set; }
    private Type? LostFocusGameState { get; set; }

    private AssetCollection GameAssets { get; } = new();

    private Action<ContainerBuilder>? AddServicesCallback { get; set; }
    private Action<IConfigurationBuilder>? ConfigurationCallback { get; set; }
    private string WindowTitle { get; set; } = "PlayPlayMini Game";
    private (int Width, int Height, int Zoom) WindowSize { get; set; } = (1920 / 3, 1080 / 3, 2);
    private bool FixedTimeStep { get; set; }

    public GameStateManagerBuilder SetWindowSize(int width, int height, int zoom)
    {
        WindowSize = (width, height, zoom);

        return this;
    }

    public GameStateManagerBuilder SetWindowTitle(string title)
    {
        WindowTitle = title;

        return this;
    }

    public GameStateManagerBuilder SetInitialGameState<T>() where T: GameState
    {
        InitialGameState = typeof(T);

        return this;
    }

    public GameStateManagerBuilder SetLostFocusGameState<T>() where T: GameState
    {
        LostFocusGameState = typeof(T);

        return this;
    }

    public GameStateManagerBuilder UseFixedTimeStep(bool fixedTimeStep = true)
    {
        FixedTimeStep = fixedTimeStep;

        return this;
    }

    public GameStateManagerBuilder AddAssets(IList<IAsset> assets)
    {
        GameAssets.AddRange(assets);

        return this;
    }

    public GameStateManagerBuilder AddServices(Action<ContainerBuilder> callback)
    {
        if (AddServicesCallback != null)
            throw new ArgumentException("AddServices may only be called once!");

        AddServicesCallback = callback;

        return this;
    }

    public GameStateManagerBuilder AddConfiguration(Action<IConfigurationBuilder> callback)
    {
        if (ConfigurationCallback != null)
            throw new ArgumentException("AddConfiguration may only be called once!");

        ConfigurationCallback = callback;

        return this;
    }

    private static List<Assembly> GetListOfEntryAssemblyWithReferences()
    {
        var listOfAssemblies = new List<Assembly>();
        var mainAsm = Assembly.GetEntryAssembly()!;

        listOfAssemblies.Add(mainAsm);
        listOfAssemblies.AddRange(mainAsm.GetReferencedAssemblies().Select(Assembly.Load));

        return listOfAssemblies;
    }

    public void Run()
    {
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"Content{Path.DirectorySeparatorChar}appsettings.json", optional: true, reloadOnChange: false) // TODO: does this work on Android?
        ;

        ConfigurationCallback?.Invoke(configBuilder);

        var configuration = configBuilder.Build();

        var builder = new ContainerBuilder();
        var serviceWatcher = new ServiceWatcher();
        var assemblies = GetListOfEntryAssemblyWithReferences();

        builder.RegisterInstance(serviceWatcher);
        builder.RegisterInstance(configuration).As<IConfiguration>();

        foreach(var assembly in assemblies)
        {
            var withAutoRegister = assembly.GetTypes()
                .Where(t => t.GetCustomAttributes<AutoRegister>().Any())
            ;

            foreach(var type in withAutoRegister)
            {
                var autoRegisterInfo = type.GetCustomAttribute<AutoRegister>()!;

                var registration = builder.RegisterType(type);

                if(autoRegisterInfo.InstanceOf != null)
                    registration.As(autoRegisterInfo.InstanceOf);
                else
                    registration.AsSelf();

                switch(autoRegisterInfo.Lifetime)
                {
                    case Lifetime.Singleton:
                        registration.SingleInstance();
                        break;

                    case Lifetime.PerDependency:
                        registration.InstancePerDependency();
                        break;
                }

                registration
                    .OnActivating(s => serviceWatcher.RegisterService(s.Instance))
                    .OnRelease(s => serviceWatcher.UnregisterService(s)) // pretty sure this is never called for ISingleInstance?
                ;
            }

            builder.RegisterAssemblyTypes(assembly)
                .Where(t => t.IsAssignableTo<GameState>())
                .AsSelf()
                .InstancePerDependency()
                .OnActivating(s => serviceWatcher.RegisterService(s.Instance))
                .OnRelease(s => serviceWatcher.UnregisterService(s))
            ;
        }

        builder.Register(_ => LoggerFactory.Create(f => { f.AddConsole(); }))
            .As<ILoggerFactory>()
            .SingleInstance();
        builder.RegisterGeneric(typeof(Logger<>))
            .As(typeof(ILogger<>))
            .SingleInstance();

        AddServicesCallback?.Invoke(builder);

        if(InitialGameState == null)
            throw new ArgumentException("No initial game state set! You must call GameStateManagerBuilder's SetInitialGameState method before calling its Run method.");

        var gameStateManagerConfig = new GameStateManagerConfig(
            InitialGameState,
            LostFocusGameState,
            WindowSize,
            WindowTitle,
            GameAssets
        );

        // here we go!
        using (var container = builder.Build())
        using (var scope = container.BeginLifetimeScope())
        using (var game = scope.Resolve<GameStateManager>(new TypedParameter(typeof(GameStateManagerConfig), gameStateManagerConfig)))
        {
            InstantiateLoadContentAndInitializedServices(scope);

            game.IsFixedTimeStep = FixedTimeStep;

            game.Run();
        }
    }

    private static void InstantiateLoadContentAndInitializedServices(ILifetimeScope scope)
    {
        var serviceTypes = scope.ComponentRegistry.Registrations
            .SelectMany(r => r.Services)
            .OfType<IServiceWithType>()
            .Select(s => s.ServiceType)
            .Where(serviceType =>
                typeof(IServiceInitialize).IsAssignableFrom(serviceType) ||
                typeof(IServiceLoadContent).IsAssignableFrom(serviceType)
            )
            .ToList()
        ;

        foreach(var t in serviceTypes)
            scope.Resolve(t);
    }
}
