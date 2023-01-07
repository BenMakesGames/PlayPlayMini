[![Buy Me a Coffee at ko-fi.com](https://raw.githubusercontent.com/BenMakesGames/AssetsForNuGet/main/buymeacoffee.png)](https://ko-fi.com/A0A12KQ16)

# What Is It?

`PlayPlayMini.GraphicsExtensions` contains extensions for PlayPlayMini that add more functionality to PlayPlayMini's `GraphicsManager`.

This is a super-early release which includes one extension.

## `DrawWavyText`

`DrawWavyText` allows you to draw text that moves up and down in a wavy pattern. There are a few overloads of the method which allow you to customize color and positioning of the text.

Draw waving text starting at the specified x/y position:

```c#
GraphicsManager.DrawWavyText(
    string fontName,
    GameTime gameTime,
    int x, int y,
    string text,
    Color color = Color.White
);
```

Draw waving text horizontally-centered at the specified y position:

```c#
GraphicsManager.DrawWavyText(
    string fontName,
    GameTime gameTime,
    int y,
    string text,
    Color color = Color.White
);
```

Draw waving text in the dead center of the screen:

```c#
GraphicsManager.DrawWavyText(
    string fontName,
    GameTime gameTime,
    string text,
    Color color = Color.White
);
```