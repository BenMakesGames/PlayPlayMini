using Autofac;
using BenMakesGames.PlayPlayMini;
using BenMakesGames.PlayPlayMini.Model;
using MyNamespace.GameStates;
using Serilog.Extensions.Autofac.DependencyInjection;
using Serilog;

var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
var appDataGameDirectory = Path.Join(appData, "MyNamespace");

Directory.CreateDirectory(appDataGameDirectory);

var gsmBuilder = new GameStateManagerBuilder();

gsmBuilder
    .SetWindowSize(1920 / 4, 1080 / 4, 2)
    .SetInitialGameState<Startup>()
    .SetLostFocusGameState<LostFocus>()

    // TODO: set a better window title
    .SetWindowTitle("MyNamespace")

    // TODO: add any resources needed (refer to PlayPlayMini documentation for more info)
    .AddAssets([
        new FontMeta("Font", [
            new FontSheetMeta("Graphics/Font", 6, 8) { VerticalSpacing = 1, HorizontalSpacing = 0 },
        ]),
        new PictureMeta("Cursor", "Graphics/Cursor", true),

        // new FontMeta(...)
        // new PictureMeta(...)
        // new SpriteSheetMeta(...)
        // new SongMeta(...)
        // new SoundEffectMeta(...)
    ])

    // TODO: any additional service registration (refer to PlayPlayMini and/or Autofac documentation for more info)
    .AddServices((s, c) => {
        var loggerConfig = new LoggerConfiguration()
            .WriteTo.File($"{appDataGameDirectory}{Path.DirectorySeparatorChar}Log.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
//-:cnd:noEmit
            #if DEBUG
                .WriteTo.Console()
            #endif
//+:cnd:noEmit
        ;

        s.RegisterSerilog(loggerConfig);
    })
;

gsmBuilder.Run();
