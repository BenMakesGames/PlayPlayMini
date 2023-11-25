using System;
using BenMakesGames.PlayPlayMini.UI.Services;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using BenMakesGames.PlayPlayMini.UI.Model;

namespace BenMakesGames.PlayPlayMini.UI.UIElements;

public class Button : IUIElement
{
    public UIService UI { get; private set; }

    public string Label { get; set; }

    public int X { get; set; }
    public int Y { get; set; }
    public bool Visible { get; set; } = true;
    public bool Enabled { get; set; } = true;
    public int Width => ForcedWidth ?? (6 + Label.Length * UI.Font.CharacterWidth + (Label.Length - 1) * UI.Font.HorizontalSpacing);
    public int Height => UI.Graphics.SpriteSheets[UI.GetTheme().ButtonSpriteSheetName].SpriteHeight;

    private int? ForcedWidth { get; }

    public Action<ClickEvent>? ClickHandler { get; set; }

    public Action<ClickEvent> DoClick => e => {
        if (Enabled && ClickHandler != null)
            ClickHandler(e);
    };

    public bool CanClick => Enabled && ClickHandler != null;

    public Action<DoubleClickEvent>? DoDoubleClick { get; set; }

    public Action? DoMouseEnter { get; set; }
    public Action? DoMouseExit { get; set; }

    public IReadOnlyList<IUIElement> Children => new List<IUIElement>();

    public Button(UIService ui, int x, int y, string label, Action<ClickEvent> clickHandler)
    {
        UI = ui;
        X = x;
        Y = y;
        Label = label;
        ClickHandler = clickHandler;
    }

    public Button(UIService ui, int x, int y, string label, int width, Action<ClickEvent> clickHandler)
    {
        UI = ui;
        X = x;
        Y = y;
        Label = label;
        ForcedWidth = width;
        ClickHandler = clickHandler;
    }

    public void Draw(int xOffset, int yOffset, GameTime gameTime)
    {
        var spiteIndexOffset = Enabled ? 0 : 3;

        var button = UI.Graphics.SpriteSheets[UI.GetTheme().ButtonSpriteSheetName];

        // left edge
        UI.Graphics.DrawSprite(button, X + xOffset, Y + yOffset, spiteIndexOffset + 0);

        // middle
        UI.Graphics.DrawSpriteStretched(button, X + button.SpriteWidth + xOffset, Y + yOffset, Width - button.SpriteWidth * 2, button.SpriteHeight, spiteIndexOffset + 1);

        // right edge
        UI.Graphics.DrawSprite(button, X + Width - button.SpriteWidth + xOffset, Y + yOffset, spiteIndexOffset + 2);

        UI.Graphics.DrawText(UI.Font, X + (Width - (Label.Length * UI.Font.CharacterWidth + (Label.Length - 1) * UI.Font.HorizontalSpacing)) / 2 + xOffset, Y + 4 + yOffset, Label, Enabled ? UI.GetTheme().ButtonLabelColor : UI.ThemeProvider.GetTheme().ButtonLabelDisabledColor);
    }
}
