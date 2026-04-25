using System;
using BenMakesGames.PlayPlayMini.UI.Services;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using BenMakesGames.PlayPlayMini.UI.Model;

namespace BenMakesGames.PlayPlayMini.UI.UIElements;

public class Label : IUIElement
{
    public UIService UI { get; set; }

    public string Text { get; private set; }

    public int X { get; set; }
    public int Y { get; set; }
    public bool Visible { get; set; } = true;
    public virtual int Width => ForcedWidth ?? UI.Font.ComputeWidth(Text);
    public virtual int Height => UI.Font.MaxCharacterHeight;
    public Color Color { get; set; }

    protected int? ForcedWidth { get; }

    public Action<ClickEvent>? DoClick { get; set; }
    public Action<DoubleClickEvent>? DoDoubleClick { get; set; }
    public Action? DoMouseEnter { get; set; }
    public Action? DoMouseExit { get; set; }

    public IReadOnlyList<IUIElement> Children => new List<IUIElement>();

    public Label(UIService ui, int x, int y, string text, Color color)
    {
        UI = ui;

        X = x;
        Y = y;
        Text = text;
        Color = color;
    }

    public Label(UIService ui, int x, int y, string text, int width, Color color)
    {
        UI = ui;

        X = x;
        Y = y;
        Text = text;
        ForcedWidth = width;
        Color = color;
    }

    public virtual void Draw(int xOffset, int yOffset, GameTime gameTime)
    {
        var font = UI.Font;
        UI.Graphics.DrawText(font, X + (Width - font.ComputeWidth(Text)) / 2 + xOffset, Y + yOffset, Text, Color);
    }
}
