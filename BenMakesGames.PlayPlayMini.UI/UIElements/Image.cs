using System;
using BenMakesGames.PlayPlayMini.Model;
using BenMakesGames.PlayPlayMini.UI.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using BenMakesGames.PlayPlayMini.UI.Model;

namespace BenMakesGames.PlayPlayMini.UI.UIElements;

public class Image: IUIElement
{
    public UIService UI { get; set; }

    public int X { get; set; }
    public int Y { get; set; }
    public bool Visible { get; set; } = true;
    public int Width => SpriteRectangle.Width;
    public int Height => SpriteRectangle.Height;
    public IReadOnlyList<IUIElement> Children => new List<IUIElement>();

    private Texture2D Texture { get; }
    private Rectangle SpriteRectangle { get; }

    public Action<ClickEvent>? DoClick { get; set; }
    public Action<DoubleClickEvent>? DoDoubleClick { get; set; }
    public Action? DoMouseEnter { get; set; }
    public Action? DoMouseExit { get; set; }

    public Image(UIService ui, int x, int y, Texture2D texture, Rectangle spriteRectangle)
    {
        UI = ui;
        X = x;
        Y = y;
        Texture = texture;
        SpriteRectangle = spriteRectangle;
    }

    public Image(UIService ui, int x, int y, SpriteSheet ss, int spriteIndex)
    {
        UI = ui;
        X = x;
        Y = y;
        Texture = ss.Texture;
        SpriteRectangle = new Rectangle((spriteIndex % ss.Columns) * ss.SpriteWidth, (spriteIndex / ss.Columns) * ss.SpriteHeight, ss.SpriteWidth, ss.SpriteHeight);
    }

    public void Draw(int xOffset, int yOffset, GameTime gameTime)
    {
        UI.Graphics.DrawTexture(Texture, X + xOffset, Y + yOffset, SpriteRectangle);
    }
}