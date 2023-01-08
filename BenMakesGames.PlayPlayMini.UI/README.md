[![Buy Me a Coffee at ko-fi.com](https://raw.githubusercontent.com/BenMakesGames/AssetsForNuGet/main/buymeacoffee.png)](https://ko-fi.com/A0A12KQ16)

# What Is It?

`PlayPlayMini.UI` is an extension for `PlayPlayMini`, adding a skinnable, object-oriented UI framework.

# Documentation; How to Use `PlayPlayMini.UI`

This documentation assumes you already have a project that uses the `PlayPlayMini` framework. If not, check out the `PlayPlayMini` framework documentation to get started with it!

## Load Required Resources

`PlayPlayMini.UI` requires a couple spritesheets, an image, and a font to be loaded. These are used for rendering buttons, checkbox, and the mouse cursor.

```C#
var gsmBuilder = new GameStateManagerBuilder()
    .AddAssets(new IAsset[] {
        ...
        new PictureMeta("Cursor", "Graphics/Cursor", true), // preload, so we can display on loading screen
        new SpriteSheetMeta("Button", "Graphics/Button", 4, 16),
        new SpriteSheetMeta("Checkbox", "Graphics/Checkbox", 10, 8),
        new FontMeta("Font", "Graphics/Font", 6, 8),
    })
    ...
;
```

At the time of this writing, `PlayPlayMini.UI` is not as flexible as it could be with the size of the UI elements it renders. For the best effect, use a font close to 8 pixels in height.

## Configure UI Framework

### Setup Custom Mouse

In the `PlayPlayMini`, it is best-practice to have a Startup GameState which displays a loading message to the player while your deferred resources load.

One of the primary reasons for using `PlayPlayMini.UI` is for a mouse-driven UI. If you're not already doing so, you should configure the `MouseManager` in your Startup `GameState`. For example:

```C#
public class sealed Startup : GameState
{
    private MouseManager Mouse { get; }

    public Startup(MouseManager mouse, ...)
    {
        Mouse = mouse;
        ...

        Mouse.UseCustomCursor("Cursor", (3, 1)); // the Picture named "Cursor", loaded in previous code sample
    }
}
```

### Create Theme Provider

`PlayPlayMini.UI` has a theming system that allows you to control the appearance of buttons, checkboxes, etc, and even change them at run-time. To facilitate this, you need to create a theme provider that tells `PlayPlayMini.UI` which theme to use.

The theme provider must extend `UIThemeProvider`, and be registered as a service. Here's an example implementation:

```c#
[AutoRegister(Lifetime.Singleton, InstanceOf = typeof(UIThemeProvider))]
public sealed class ThemeProvider: UIThemeProvider
{
    protected override Theme CurrentTheme { get; set; } = new(
        WindowColor: Colors.Orange,
        FontName: "Font",
        ButtonSpriteSheetName: "Button",
        ButtonLabelColor: Colors.White,
        ButtonLabelDisabledColor: Colors.Gray,
        CheckboxSpriteSheetName: "Checkbox"
    );
}
```

To change the theme, call `SetTheme`. The following example assumes you have a pre-defined list of themes you want to let the players from, but you're free to make more-complex UIs, of course:

```c#
public sealed class SettingsMenu: GameState
{
    private static readonly List<Theme> AvailableThemes = { ... };

    private ThemeProvider ThemeProvider { get; }
    ...

    public SettingsMenu(ThemeProvider themeProvider, ...)
    {
        ThemeProvider = themeProvider;
        ...
    }

    private void SelectTheme(int themeIndex)
    {
        ThemeProvider.SetTheme(AvailableThemes[themeIndex]); // set the theme!
    }

    ...
}
```

## In a `GameState`, Request an Instance of `UIService`, and Configure It

Each instance of `UIService` that's requested is a new instance. A `UIService` instance contains UI components, such as buttons, and is responsible for handling mouse events that take place within it, including hovers & clicks, and notifying the appropriate component(s) of these events.

Example:

```C#
public class sealed PauseMenu: GameState
{
    private UIService UI { get; }
    ...

    public PauseMenu(UIService ui, ...)
    {
        UI = ui;
        ...

        InitUI();
    }

    // it's not required to create a dedicated method for building the UI
    // (you'll only ever call it once), but it helps keep the constructor tidy.
    private void InitUI()
    {
        // Window is a UI component provided by PlayPlayMini.UI
        var window = new Window(
            UI,         // instance of the UIService
            20, 20,     // x, y coordinate to position the window
            150, 100,   // width, height of the window
            "Paused"    // title
        );

        var resume = new Button(UI, 4, 16, "Resume", ResumeGameHandler);
        var settings = new Button(UI, 4, 33, "Settings", SettingsHandler);
        var saveAndQuit = new Button(UI, 4, 50, "Save & Quit", SaveAndQuitHandler);

        // add the buttons to the window:
        window.AddUIElements(resume, settings, saveAndQuit);

        // and the window to the UI canvas:
        UI.Canvas.AddUIElements(window);
    }

    private void ResumeGameHandler(ClickEvent click)
    {
        // x, y coordinates that click took place, relative to the button's position
        // do something
    }

    private void SettingsHandler(ClickEvent click)
    {
        // do something
    }

    private void SaveAndQuitHandler(ClickEvent click)
    {
        // do something
    }

    public override void ActiveUpdate(GameTime gameTime)
    {
        UI.ActiveUpdate(gameTime);
    }

    public override void AlwaysDraw(GameTime gameTime)
    {
        UI.AlwaysDraw(gameTime);
    }

    public override void ActiveDraw(GameTime gameTime)
    {
        UI.ActiveDraw(gameTime);
        // note: there is no need to manually update or draw the mouse cursor;
        // if you're using a UIService, it will handle the mouse for you.
    }
}
```