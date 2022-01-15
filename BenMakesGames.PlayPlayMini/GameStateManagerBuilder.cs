using Autofac;
using Autofac.Core;
using BenMakesGames.PlayPlayMini.Attributes.DI;
using BenMakesGames.PlayPlayMini.Model;
using BenMakesGames.PlayPlayMini.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BenMakesGames.PlayPlayMini
{
    public class GameStateManagerBuilder
    {
        private Type? InitialGameState { get; set; }

        // TODO: this is dumb and bad (not extensible), and must be fixed/replaced:
        private List<PictureMeta> PictureMeta { get; set; }
        private List<SpriteSheetMeta> SpriteSheetMeta { get; set; }
        private List<FontMeta> FontMeta { get; set; }
        private List<SongMeta> SongMeta { get; set; }
        private List<SoundEffectMeta> SoundEffectMeta { get; set; }

        private Action<ContainerBuilder>? AddServicesCallback { get; set; }
        private string WindowTitle { get; set; } = "MonoGame Game";
        private (int Width, int Height, int Zoom) WindowSize { get; set; } = (1920 / 3, 1080 / 3, 2);
        private bool FixedTimeStep { get; set; }

        public GameStateManagerBuilder()
        {
            PictureMeta = new List<PictureMeta>();
            SpriteSheetMeta = new List<SpriteSheetMeta>();
            FontMeta = new List<FontMeta>();
            SongMeta = new List<SongMeta>();
            SoundEffectMeta = new List<SoundEffectMeta>();
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

        public GameStateManagerBuilder SetInitialGameState<T>() where T:IGameState
        {
            InitialGameState = typeof(T);
            
            return this;
        }

        public GameStateManagerBuilder UseFixedTimeStep()
        {
            FixedTimeStep = true;

            return this;
        }

        public GameStateManagerBuilder AddPictures(IEnumerable<PictureMeta> pictures)
        {
            PictureMeta.AddRange(pictures);

            return this;
        }

        public GameStateManagerBuilder AddSpriteSheets(IEnumerable<SpriteSheetMeta> spriteSheets)
        {
            SpriteSheetMeta.AddRange(spriteSheets);

            return this;
        }

        public GameStateManagerBuilder AddFonts(IEnumerable<FontMeta> fonts)
        {
            FontMeta.AddRange(fonts);

            return this;
        }

        public GameStateManagerBuilder AddSongs(IEnumerable<SongMeta> songs)
        {
            SongMeta.AddRange(songs);

            return this;
        }

        public GameStateManagerBuilder AddSoundEffects(IEnumerable<SoundEffectMeta> soundEffects)
        {
            SoundEffectMeta.AddRange(soundEffects);

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
                    .Where(t => t.IsAssignableTo<IGameState>())
                    .AsSelf()
                    .InstancePerDependency()
                    .OnActivating(s => serviceWatcher.RegisterService(s.Instance))
                    .OnRelease(s => serviceWatcher.UnregisterService(s))
                ;
            }

            // manually register some C# built-in stuff:
            builder.RegisterType<Random>().AsSelf().SingleInstance();

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

                // TODO: this is dumb and bad (not extensible), and must be fixed/replaced:
                game.Pictures = PictureMeta;
                game.SpriteSheets = SpriteSheetMeta;
                game.Fonts = FontMeta;
                game.Songs = SongMeta;
                game.SoundEffects = SoundEffectMeta;
                game.IsFixedTimeStep = FixedTimeStep;

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
}
