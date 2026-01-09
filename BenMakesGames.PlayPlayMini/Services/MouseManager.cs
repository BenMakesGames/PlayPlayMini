using BenMakesGames.PlayPlayMini.Attributes.DI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using BenMakesGames.PlayPlayMini.Model;

namespace BenMakesGames.PlayPlayMini.Services;

/// <summary>
/// Service for getting mouse input and drawing a custom mouse cursor.
/// </summary>
/// <example>
/// <code>
/// public sealed class YourGameState : GameState
/// {
///     private GameStateManager GSM { get; }
///     private MouseManager Mouse { get; }
///&nbsp;
///     public YourGameState(
///         GameStateManager gsm, MouseManager mouse
///     )
///     {
///         GSM = gsm;
///         Mouse = mouse
///     }
///&nbsp;
///     public override void Input(GameTime gameTime)
///     {
///         if(Mouse.LeftClicked &amp;&amp; Mouse.IsInWindow())
///         {
///             // do something, possibly using Mouse.X and/or Mouse.Y
///         }
///     }
///&nbsp;
///     public override void Draw(GameTime gameTime)
///     {
///         if(GSM.CurrentState == this)
///             Mouse.Draw(gameTime);
///     }
/// }
/// </code>
/// </example>
[AutoRegister]
public sealed class MouseManager : IServiceInput
{
    private GraphicsManager GraphicsManager { get; }
    private KeyboardManager Keyboard { get; }
    private GameStateManager GSM { get; }

    private MouseState PreviousMouseState { get; set; }
    private MouseState MouseState { get; set; }

    public string? PictureName { get; private set; }
    public (int X, int Y) Hotspot { get; private set; }

    /// <summary>
    /// The mouse's current X position in pixels, relative to the game window's upper-left corner.
    /// </summary>
    public int X { get; private set; }

    /// <summary>
    /// The mouse's current Y position in pixels, relative to the game window's upper-left corner.
    /// </summary>
    public int Y { get; private set; }

    /// <summary>
    /// True if the mouse moved since the last frame.
    /// </summary>
    public bool Moved { get; private set;}

    /// <summary>
    /// True if the left button is currently being pressed.
    /// </summary>
    public bool LeftDown { get; private set; }

    /// <summary>
    /// True if the left button was released this frame.
    /// </summary>
    public bool LeftClicked { get; private set; }

    /// <summary>
    /// True if the right button is currently being pressed.
    /// </summary>
    public bool RightDown { get; private set; }

    /// <summary>
    /// True if the right button was released this frame.
    /// </summary>
    public bool RightClicked { get; private set; }

    public int Wheel { get; private set; }

    /// <summary>
    /// When the mouse is not Enabled, *Down, *Click, and Moved properties are always false, Wheel is 0, and X and Y are int.MinValue.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// If true, <c>Enabled</c> will be set to true when the mouse is moved.
    /// </summary>
    public bool EnableOnMove { get; set; } = true;

    /// <summary>
    /// If true, <c>Enabled</c> will be set to false when any key is pressed.
    /// </summary>
    public bool DisableOnKeyPress { get; set; } = false;

    /// <summary>
    /// When ClampToWindow is set, <c>X</c> &amp; <c>Y</c> will be confined to the window, regardless of the mouse's physical position.
    /// </summary>
    public bool ClampToWindow { get; set; } = false;

    public MouseDrawingMode DrawingMode { get; private set; } = MouseDrawingMode.System;

    public MouseManager(GraphicsManager gm, KeyboardManager keyboard, GameStateManager gsm)
    {
        GraphicsManager = gm;
        Keyboard = keyboard;
        GSM = gsm;

        PreviousMouseState = Mouse.GetState();
        MouseState = Mouse.GetState();
    }

    /// <inheritdoc />
    public void Input(GameTime gameTime)
    {
        PreviousMouseState = MouseState;
        MouseState = Mouse.GetState();

        if(Enabled)
        {
            if(DisableOnKeyPress && Keyboard.AnyKeyDown())
            {
                Enabled = false;
                GSM.IsMouseVisible = false;
            }
        }
        else
        {
            if(EnableOnMove && (PreviousMouseState.X != MouseState.X || PreviousMouseState.Y != MouseState.Y))
            {
                Enabled = true;
                GSM.IsMouseVisible = DrawingMode == MouseDrawingMode.System;
            }
        }

        if (Enabled)
        {
            if (ClampToWindow)
            {
                X = Math.Min(Math.Max(MouseState.X / GraphicsManager.Zoom, 0), GraphicsManager.Width - 1);
                Y = Math.Min(Math.Max(MouseState.Y / GraphicsManager.Zoom, 0), GraphicsManager.Height - 1);
            }
            else
            {
                X = MouseState.X / GraphicsManager.Zoom;
                Y = MouseState.Y / GraphicsManager.Zoom;
            }

            Moved = PreviousMouseState.X != MouseState.X || PreviousMouseState.Y != MouseState.Y;

            LeftDown = MouseState.LeftButton == ButtonState.Pressed;
            RightDown = MouseState.RightButton == ButtonState.Pressed;

            LeftClicked = MouseState.LeftButton != ButtonState.Pressed && PreviousMouseState.LeftButton == ButtonState.Pressed;
            RightClicked = MouseState.RightButton != ButtonState.Pressed && PreviousMouseState.RightButton == ButtonState.Pressed;

            Wheel = MouseState.ScrollWheelValue - PreviousMouseState.ScrollWheelValue;
        }
        else
        {
            X = int.MinValue;
            Y = int.MinValue;
            Moved = false;
            LeftDown = false;
            RightDown = false;
            LeftClicked = false;
            RightClicked = false;
            Wheel = 0;
        }
    }

    /// <summary>
    /// Call in your GameState's Draw method to draw a custom mouse cursor.
    /// </summary>
    /// <remarks>If you will NEVER use a custom mouse cursor, you never need to call this method.</remarks>
    /// <param name="gameTime"></param>
    public void Draw(GameTime gameTime)
    {
        if(DrawingMode != MouseDrawingMode.Custom)
            return;

        if(Enabled && PictureName is { } pictureName)
            GraphicsManager.DrawPicture(pictureName, X - Hotspot.X, Y - Hotspot.Y);
    }

    /// <summary>
    /// The operating system's mouse cursor will be drawn as normal. Calling MouseManager.Draw will draw the picture
    /// specified here at the current mouse position.
    /// </summary>
    /// <param name="pictureName"></param>
    /// <param name="hotspot">The point in the picture that is visually where clicks take place - the end of an arrow,or center of a cross-hair, for example.</param>
    public void UseCustomCursor(string pictureName, (int x, int y) hotspot)
    {
        DrawingMode = MouseDrawingMode.Custom;
        PictureName = pictureName;
        Hotspot = hotspot;
        GSM.IsMouseVisible = false;
    }

    /// <summary>
    /// The operating system's mouse cursor will be drawn as normal. Calling MouseManager.Draw will have no effect.
    /// </summary>
    public void UseSystemCursor()
    {
        DrawingMode = MouseDrawingMode.System;
        GSM.IsMouseVisible = true;
    }

    /// <summary>
    /// The operating system's mouse cursor will be hidden while it is over the game window, and Calling
    /// MouseManager.Draw will have no effect.
    /// </summary>
    public void UseNoCursor()
    {
        DrawingMode = MouseDrawingMode.None;
        GSM.IsMouseVisible = false;
    }

    /// <summary>
    /// Returns true if the mouse is currently in the specified rectangle.
    /// </summary>
    /// <param name="rectangle"></param>
    /// <returns></returns>
    public bool IsInRectangle(Rectangle rectangle) => rectangle.Contains(X, Y);

    /// <summary>
    /// Returns true if the mouse is currently in the specified rectangle.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public bool IsIsRectangle(int x, int y, int width, int height) => X >= x && X < x + width && Y >= y && Y < y + height;

    /// <summary>
    /// Returns true if the mouse is currently in the window.
    /// </summary>
    /// <returns></returns>
    public bool IsInWindow() => X >= 0 && X < GraphicsManager.Width && Y >= 0 && Y < GraphicsManager.Height;

    /// <summary>
    /// Returns true if the mouse is currently in the specified circle.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public bool IsInCircle(int x, int y, int radius) => Math.Pow(X - x, 2) + Math.Pow(Y - y, 2) < Math.Pow(radius, 2);

    /// <summary>
    /// Returns true if the mouse is currently in the specified circle.
    /// </summary>
    /// <param name="center"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public bool IsInCircle(Point center, int radius) => Math.Pow(X - center.X, 2) + Math.Pow(Y - center.Y, 2) < Math.Pow(radius, 2);
}