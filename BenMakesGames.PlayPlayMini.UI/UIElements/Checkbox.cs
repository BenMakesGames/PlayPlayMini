using BenMakesGames.PlayPlayMini.Model;
using BenMakesGames.PlayPlayMini.UI.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using BenMakesGames.PlayPlayMini.UI.Model;

namespace BenMakesGames.PlayPlayMini.UI.UIElements;

public class Checkbox : IUIElement
{
    public UIService UI { get; set; }

    public int X { get; set; }
    public int Y { get; set; }
    public int Width => CheckBox.SpriteWidth + 1 + Label.Length * UI.Font.CharacterWidth;
    public int Height => Math.Max(CheckBox.SpriteHeight, UI.Font.CharacterHeight);
    public bool Visible { get; set; } = true;
    public bool Checked { get; private set; }
    public string Label { get; private set; }
    public IReadOnlyList<IUIElement> Children => new List<IUIElement>();

    private SpriteSheet CheckBox => UI.Graphics.SpriteSheets[UI.GetTheme().CheckboxSpriteSheetName];

    public Action<DoubleClickEvent>? DoDoubleClick { get; set; }
    public Action<ClickEvent> DoClick => _ => { Checked = ToggleHandler(!Checked); };
    public Action? DoMouseEnter { get; set; }
    public Action? DoMouseExit { get; set; }

    private ChangeDelegate ToggleHandler { get; set; }

    public delegate bool ChangeDelegate(bool proposedValue);

    public Checkbox(UIService ui, int x, int y, string label, bool initialValue, ChangeDelegate toggle)
    {
        UI = ui;
        X = x;
        Y = y;
        Label = label;
        Checked = initialValue;
        //Width = ;
        ToggleHandler = toggle;
    }

    public void ForceValue(bool value)
    {
        Checked = value;
    }

    public void Draw(int xOffset, int yOffset, GameTime gameTime)
    {
        UI.Graphics.DrawSprite(CheckBox, X + xOffset, Y + yOffset, Checked ? 1 : 0);
        UI.Graphics.DrawText(UI.Font, X + CheckBox.SpriteWidth + 1 + xOffset, Y + yOffset, Label, Color.Black);
    }
}