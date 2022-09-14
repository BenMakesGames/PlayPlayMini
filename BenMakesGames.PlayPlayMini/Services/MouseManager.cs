using BenMakesGames.PlayPlayMini.Attributes.DI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using BenMakesGames.PlayPlayMini.Model;

namespace BenMakesGames.PlayPlayMini.Services;

[AutoRegister(Lifetime.Singleton)]
public sealed class MouseManager : IServiceInput
{
    private GraphicsManager GraphicsManager { get; }
    private KeyboardManager Keyboard { get; }
    private GameStateManager GSM { get; }

    private MouseState PreviousMouseState { get; set; }
    private MouseState MouseState { get; set; }

    public string? PictureName { get; set; }
    public (int X, int Y) Hotspot { get; set; }

    public int X { get; private set; }
    public int Y { get; private set; }
    public bool LeftDown { get; private set; }
    public bool LeftClicked { get; private set; }
    public bool RightDown { get; private set; }
    public bool RightClicked { get; private set; }
    public int Wheel { get; private set; }

    /// <summary>
    /// When the mouse is not Enabled, *Down and *Click properties are always false, Wheel is 0, and X and Y are int.MinValue.
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
            if (DisableOnKeyPress && Keyboard.AnyKeyDown())
                Enabled = false;
        }
        else
        {
            if(EnableOnMove && (PreviousMouseState.X != MouseState.X || PreviousMouseState.Y != MouseState.Y))
                Enabled = true;
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
            LeftDown = false;
            RightDown = false;
            LeftClicked = false;
            RightClicked = false;
            Wheel = 0;
        }
    }

    public void ActiveDraw(GameTime gameTime)
    {
        if(DrawingMode != MouseDrawingMode.Custom)
            return;
            
        if(Enabled && PictureName is string pictureName)
            GraphicsManager.DrawPicture(GraphicsManager.Pictures[pictureName], X - Hotspot.X, Y - Hotspot.Y);
    }

    public void UseCustomCursor(string pictureName, (int x, int y) hotspot)
    {
        DrawingMode = MouseDrawingMode.Custom;
        PictureName = pictureName;
        Hotspot = hotspot;
        GSM.IsMouseVisible = false;
    }
        
    public void UseSystemCursor()
    {
        DrawingMode = MouseDrawingMode.System;
        GSM.IsMouseVisible = true;
    }

    public void UseNoCursor()
    {
        DrawingMode = MouseDrawingMode.None;
        GSM.IsMouseVisible = false;
    }
}