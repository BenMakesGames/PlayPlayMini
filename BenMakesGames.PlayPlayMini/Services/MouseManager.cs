using BenMakesGames.PlayPlayMini.Attributes.DI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using BenMakesGames.PlayPlayMini.Model;

namespace BenMakesGames.PlayPlayMini.Services;

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

    public int X { get; private set; }
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
    public bool EnableOnMove { get; set; } = true;

    public bool DisableOnKeyPress { get; set; } = false;

    /// <summary>
    /// When ClampToWindow is set, the on-screen mouse cursor will be confined to the window.
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
}
