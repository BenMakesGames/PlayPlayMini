using System;
using BenMakesGames.PlayPlayMini.UI.Model;
using BenMakesGames.PlayPlayMini.UI.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.UI.UIElements;

public class Canvas: UIContainer, IUIElement
{
    public UIService UI { get; set; }

    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool Visible { get; set; } = true;

    public Color BackgroundColor { get; set; } = Color.Transparent;

    public Action<ClickEvent>? DoClick { get; set; }
    public Action<DoubleClickEvent>? DoDoubleClick { get; set; }
    public Action? DoMouseEnter { get; set; }
    public Action? DoMouseExit { get; set; }

    public Canvas(UIService ui)
    {
        UI = ui;
    }

    public void Draw(int xOffset, int yOffset, GameTime gameTime)
    {
        if(BackgroundColor.A > 0)
            UI.Graphics.DrawFilledRectangle(X + xOffset, Y + yOffset, Width, Height, BackgroundColor);
    }
}
