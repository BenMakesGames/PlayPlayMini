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
using Autofac.Util;
using Microsoft.Extensions.Configuration;

namespace BenMakesGames.PlayPlayMini;

/// <summary>
/// Builder for creating a <see cref="GameStateManager"/>.
/// </summary>
/// <remarks>
/// Creating one of these and calling <c>.Run()</c> on it will start the game. This should generally be done in the entry point of your application (often <c>Program.cs</c>).
/// </remarks>
public class GameStateManagerBuilder
{
    private Type? InitialGameState { get; set; }
    private Type? LostFocusGameState { get; set; }

    private AssetCollection GameAssets { get; } = new();

    private List<Action<ContainerBuilder, IConfiguration, ServiceWatcher>> AddServicesCallbacks { get; } = [ ];
    private List<Action<IConfigurationBuilder>> ConfigurationCallbacks { get; } = [ ];
    private string WindowTitle { get; set; } = "PlayPlayMini Game";
    private (int Width, int Height, int Zoom) WindowSize { get; set; } = (1920 / 3, 1080 / 3, 2);
    private bool FixedTimeStep { get; set; }

    /// <summary>
    /// Sets the window size and zoom level.
    ///
    /// If this method is not called, the window defaults to 640x360 with a zoom level of 2.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="zoom"></param>
    /// <returns></returns>
    public GameStateManagerBuilder SetWindowSize(int width, int height, int zoom)
    {
        WindowSize = (width, height, zoom);

        return this;
    }

    /// <summary>
    /// Sets the window title.
    ///
    /// If this method is not called, the window title defaults to "PlayPlayMini Game".
    /// </summary>
    /// <param name="title"></param>
    /// <returns></returns>
    public GameStateManagerBuilder SetWindowTitle(string title)
    {
        WindowTitle = title;

        return this;
    }

    /// <summary>
    /// Sets the initial game state.
    ///
    /// If this method is not called, an exception will be thrown when <see cref="Run"/> is called.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
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

    /// <summary>
    /// Sets whether the game should use a fixed time step. When using a fixed time step, the <c>Update</c> method of
    /// game states will be called an average of 60 times per second, instead of as fast as possible.
    ///
    /// If this method is not called, the game will NOT use a fixed time step.
    /// </summary>
    /// <param name="fixedTimeStep"></param>
    /// <returns></returns>
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

    public GameStateManagerBuilder AddServices(Action<ContainerBuilder, IConfiguration, ServiceWatcher> callback)
    {
        AddServicesCallbacks.Add(callback);

        return this;
    }

    public GameStateManagerBuilder AddServices(Action<ContainerBuilder, IConfiguration> callback)
    {
        AddServicesCallbacks.Add((s, c, _) => callback(s, c));

        return this;
    }

    public GameStateManagerBuilder AddConfiguration(Action<IConfigurationBuilder> callback)
    {
        ConfigurationCallbacks.Add(callback);

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

    /// <summary>
    /// Call to build & run the game.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    public void Run()
    {
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(Path.Combine("Content", "appsettings.json"), optional: true, reloadOnChange: false) // TODO: does this work on Android?
        ;

        foreach(var callback in ConfigurationCallbacks)
            callback(configBuilder);

        var configuration = configBuilder.Build();

        var builder = new ContainerBuilder();
        var serviceWatcher = new ServiceWatcher();
        var assemblies = GetListOfEntryAssemblyWithReferences();

        builder.RegisterInstance(serviceWatcher);
        builder.RegisterInstance(configuration).As<IConfiguration>();

        foreach(var assembly in assemblies)
        {
            var withAutoRegister = assembly.GetLoadableTypes()
                .Where(t => t.GetCustomAttributes<AutoRegister>().Any())
            ;

            foreach(var type in withAutoRegister)
            {
                var autoRegisterInfo = type.GetCustomAttribute<AutoRegister>()!;

                var registration = builder.RegisterType(type);

                if(autoRegisterInfo.InstanceOf is not null)
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
                    .OnRelease(s => serviceWatcher.UnregisterService(s)) // pretty sure this is never called for singletons?
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

        foreach(var callback in AddServicesCallbacks)
            callback(builder, configuration, serviceWatcher);

        if(InitialGameState is null)
            throw new ArgumentException("No initial game state set! You must call GameStateManagerBuilder's SetInitialGameState method before calling its Run method.");

        var gameStateManagerConfig = new GameStateManagerConfig(
            InitialGameState,
            LostFocusGameState,
            WindowSize,
            WindowTitle,
            GameAssets
        );

        // let's-a go!
        using var container = builder.Build();
        using var scope = container.BeginLifetimeScope();

        InstantiateLoadContentAndInitializedServices(scope);

        using var game = scope.Resolve<GameStateManager>(new TypedParameter(typeof(GameStateManagerConfig), gameStateManagerConfig));

        game.IsFixedTimeStep = FixedTimeStep;

        // wahoo!
        game.Run();
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

    public bool ContainsAsset<T>(Func<T, bool> predicate) where T: IAsset
        => GameAssets.Any(asset => asset is T typedAsset && predicate(typedAsset));
}
