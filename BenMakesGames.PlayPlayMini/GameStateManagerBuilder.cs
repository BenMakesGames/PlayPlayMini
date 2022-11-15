using Autofac;
using Autofac.Core;
using BenMakesGames.PlayPlayMini.Attributes.DI;
using BenMakesGames.PlayPlayMini.Model;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BenMakesGames.PlayPlayMini;

public class GameStateManagerBuilder
{
    private Type? InitialGameState { get; set; }

    private AssetCollection GameAssets { get; set; }

    private Action<ContainerBuilder>? AddServicesCallback { get; set; }
    private string WindowTitle { get; set; } = "MonoGame Game";
    private (int Width, int Height, int Zoom) WindowSize { get; set; } = (1920 / 3, 1080 / 3, 2);
    private bool FixedTimeStep { get; set; }

    public GameStateManagerBuilder()
    {
        GameAssets = new();
    }

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

    public GameStateManagerBuilder SetInitialGameState<T>() where T:GameState
    {
        InitialGameState = typeof(T);
            
        return this;
    }

    public GameStateManagerBuilder UseFixedTimeStep()
    {
        FixedTimeStep = true;

        return this;
    }

    public GameStateManagerBuilder AddAssets(IEnumerable<Asset> assets)
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

    private static List<Assembly> GetListOfEntryAssemblyWithReferences()
    {
        List<Assembly> listOfAssemblies = new List<Assembly>();
        var mainAsm = Assembly.GetEntryAssembly()!;
        listOfAssemblies.Add(mainAsm);

        foreach (var refAsmName in mainAsm.GetReferencedAssemblies())
        {
            listOfAssemblies.Add(Assembly.Load(refAsmName));
        }
        return listOfAssemblies;
    }

    public void Run()
    {
        var builder = new ContainerBuilder();
        var serviceWatcher = new ServiceWatcher();
        var assemblies = GetListOfEntryAssemblyWithReferences();

        builder.RegisterInstance(serviceWatcher);

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

        // manually register some C# built-in stuff:
        builder.RegisterType<Random>().AsSelf().SingleInstance();

        builder.Register(_ => LoggerFactory.Create(f => { f.AddConsole(); }))
            .As<ILoggerFactory>()
            .SingleInstance();
        builder.RegisterGeneric(typeof(Logger<>))
            .As(typeof(ILogger<>))
            .SingleInstance();
        
        AddServicesCallback?.Invoke(builder);

        if(InitialGameState == null)
            throw new ArgumentException("No initial game state set! You must call GameStateManagerBuilder's SetInitialGameState method before calling its Run method.");

        // here we go!
        using (var container = builder.Build())
        using (var scope = container.BeginLifetimeScope())
        using (var game = scope.Resolve<GameStateManager>())
        {
            InstantiateLoadContentAndInitializedServices(scope);

            game.InitialGameState = InitialGameState;
            game.InitialWindowSize = WindowSize;
            game.InitialWindowTitle = WindowTitle;

            game.Run();
        }
    }

    private void InstantiateLoadContentAndInitializedServices(ILifetimeScope scope)
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