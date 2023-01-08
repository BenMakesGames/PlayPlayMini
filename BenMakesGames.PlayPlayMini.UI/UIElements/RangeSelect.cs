using BenMakesGames.PlayPlayMini.UI.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using BenMakesGames.PlayPlayMini.UI.Model;

namespace BenMakesGames.PlayPlayMini.UI.UIElements;

public class RangeSelect: IUIElement
{
    public UIService UI { get; set; }

    public string Label => Labels[Value];

    public int X { get; set; }
    public int Y { get; set; }
    public bool Visible { get; set; } = true;
    public int Width { get;}
    public int Height => Math.Max(
        UI.Graphics.SpriteSheets[UI.GetTheme().ButtonSpriteSheetName].SpriteHeight,
        UI.Font.CharacterHeight
    );

    private bool _enabled = true;

    public bool Enabled
    {
        get => _enabled;

        set
        {
            _enabled = value;
            EnableOrDisableButtons();
        }
    }

    private bool ValueWrap { get; set; }

    public int Value { get; private set; }
        
    private IList<string> Labels { get; }

    public Action<ClickEvent>? DoClick { get; set; }
    public Action<DoubleClickEvent>? DoDoubleClick { get; set; }
    public Action? DoMouseEnter { get; set; }
    public Action? DoMouseExit { get; set; }

    private RangeChangeDelegate ChangeHandler { get; set; }

    public IReadOnlyList<IUIElement> Children { get; }

    public delegate int RangeChangeDelegate(int proposedValue);

    private Button Decrement { get; }
    private Button Increment { get; }

    public RangeSelect(UIService ui, int x, int y, int width, int initialValue, IList<string> valueLabels, RangeChangeDelegate changeHandler)
    {
        UI = ui;
        X = x;
        Y = y;
        Width = width;
        Labels = valueLabels;
        Value = initialValue;

        ChangeHandler = changeHandler;

        Decrement = new Button(UI, 0, 0, "<", 16, DecrementValue);
        Increment = new Button(UI, Width - 16, 0, ">", 16, IncrementValue);

        EnableOrDisableButtons();

        Children = new List<IUIElement>() { Increment, Decrement };
    }

    public void EnableValueWrap(bool wrap)
    {
        ValueWrap = wrap;
        EnableOrDisableButtons();
    }

    public void ForceValue(int value)
    {
        Value = value;
        EnableOrDisableButtons();
    }

    public void Draw(int xOffset, int yOffset, GameTime gameTime)
    {
        UI.Graphics.DrawText(UI.GetFont(), X + 18 + xOffset, Y + 4 + yOffset, Label, Color.Black);
    }

    private void EnableOrDisableButtons()
    {
        Decrement.Enabled = Enabled && (ValueWrap || Value > 0);
        Increment.Enabled = Enabled && (ValueWrap || Value < Labels.Count - 1);
    }

    private void DecrementValue(ClickEvent e)
    {
        if(Value > 0)
        {
            Value = ChangeHandler(Value - 1);
        }
        else if(ValueWrap)
        {
            Value = ChangeHandler(Labels.Count - 1);
        }

        EnableOrDisableButtons();
    }

    private void IncrementValue(ClickEvent e)
    {
        if(Value < Labels.Count - 1)
        {
            Value = ChangeHandler(Value + 1);
        }
        else if(ValueWrap)
        {
            Value = ChangeHandler(0);
        }

        EnableOrDisableButtons();
    }
}