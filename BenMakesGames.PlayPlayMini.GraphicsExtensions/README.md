# What Is It?

`PlayPlayMini.GraphicsExtensions` contains extensions for PlayPlayMini that add more functionality to PlayPlayMini's `GraphicsManager`.

This is a super-early release which includes one extension.

[![Buy Me a Coffee at ko-fi.com](https://raw.githubusercontent.com/BenMakesGames/AssetsForNuGet/main/buymeacoffee.png)](https://ko-fi.com/A0A12KQ16)

# Method Reference

## `DrawTextWithOutline`

PlayPlayMini has a built-in `DrawTextWithOutline` method. GraphicsExtensions's version is easier to use (you do not need to create an outline version of your font), but is less computationally efficient (2.5x the draw calls).

Most games won't notice this performance difference, but if you're tight on CPU cycles for some reason, you may not want to rely on this method.

```c#
GraphicsManage.DrawTextWithOutline(
    string fontName,
    int x,
    int y,
    string text,
    Color fillColor,
    Color outlineColor
);
```

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