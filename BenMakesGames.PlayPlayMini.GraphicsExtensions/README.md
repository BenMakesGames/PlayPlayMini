# What Is It?

`PlayPlayMini.GraphicsExtensions` contains classes and extensions for PlayPlayMini that add more functionality to PlayPlayMini.

> [🧚 **Hey, listen!** You can support my development of open-source software on Patreon](https://www.patreon.com/BenMakesGames)

# Game States Transitions

## `ScreenWipe`

The `ScreenWipe` game state allows you to add a screen wipe transition between two game states.

Example usage:

```c#
public void GoToNextScene()
{
    GSM.ChangeState<ScreenWipe, ScreenWipeConfig>(new() {
        PreviousState = this,
        NextState = GSM.CreateState<NextScene>(),
        Color = Color.Black,
        WipeTime = 0.25, // in seconds
        Direction = ScreenWipeDirection.RightToLeft
    });
}
```

There are also options which let you hold on the black screen while some other process finishes (`HoldUntil`), and/or show a message (`Message` and `MessageColor`), which may be useful when performing a long-running operation, such as saving the game, loading the next level from disk, or making an web API call.

# Drawing "Primitives"

`PlayPlayMini.GraphicsExtensions` contains methods for drawing "basic shapes", beyond just rectangles (which `PlayPlayMini` itself already supports).

At the moment, only lines, ellipses (including circles), and filled triangles are supported.

## `DrawCircle`

Draws the outline of a circle, given a center position, and radius.

```c#
GraphicsManager.DrawCircle(
    int centerX,
    int centerY,
    int radius,
    Color outlineColor
);
```

```c#
GraphicsManager.DrawCircle(
    Vector2 center,
    int radius,
    Color outlineColor
);
```

## `DrawEllipse`

Draws an ellipse that fits inside a rectangle.

```c#
GraphicsManager.DrawEllipse(
    int x,
    int y,
    int width,
    int height,
    Color outlineColor
)
```

```c#
GraphicsManager.DrawEllipse(
    Rectangle rectangle,
    Color outlineColor
)
```

## `DrawFilledCircle`

Same as `DrawCircle`, above, but draws a solid circle, instead of just its outline.

## `DrawFilledEllipse`

Same as `DrawEllipse`, above, but draws a solid ellipse, instead of just its outline.

## `DrawFilledTriangle`

Draws a filled triangle, given three points.

```c#
GraphicsManager.DrawFilledTriangle(
    float x1,
    float y1,
    float x2,
    float y2,
    float x3,
    float y3,
    Color color
)
```

```c#
GraphicsManager.DrawFilledTriangle(
    Vector2 v1,
    Vector2 v2,
    Vector2 v3,
    Color color
)
```

**Notes**
* There is no `DrawTriangle` method for drawing only a triangle's outline (yet...)
* For those with a 3D background: the order your pass the verticies in does not matter - the triangle will be drawn whether the points are in clockwise or counterclockwise order

## `DrawLine`

Draws a straight line between two points.

```c#
GraphicsManager.DrawLine(int x1, int y1, int x2, int y2, Color color)
```

```c#
GraphicsManager.DrawLine(Vector2 start, Vector2 end, Color color)
```

# Drawing Text with Effects

## `DrawTextWithOutline`

PlayPlayMini has a built-in `DrawTextWithOutline` method. GraphicsExtensions's version is easier to use (you do not need to create an outline version of your font), but is less computationally efficient (2.5x the draw calls).

Most games won't notice this performance difference, but if you're tight on CPU cycles for some reason, you may not want to rely on this method.

```c#
GraphicsManager.DrawTextWithOutline(
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

# Drawing Text with Spans

`DrawText` overloads have been added to `GraphicsManager` to allow you to draw text from a `Span<char>` or `ReadOnlySpan<char>`. (A `char[]` overload was also added, for completeness.)

These overloads may be useful if you only want to draw part of some text (for example in a dialog system where each letter is progressively revealed), since using a span is more efficient than calling `.Substring` or `[]`.
