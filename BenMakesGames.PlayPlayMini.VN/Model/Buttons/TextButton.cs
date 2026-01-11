using BenMakesGames.PlayPlayMini.Model;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.VN.Model.Buttons;

public sealed class TextButton: IButton
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; }
    public int Height { get; }
    public Action? Click { get; set; }
    public string Label { get; }
    public Font Font { get; }

    public TextButton(int x, int y, Action? click, Font font, string label, int? minWidth = null)
    {
        X = x;
        Y = y;
        Width = Math.Max(minWidth ?? 0, (font.CharacterWidth + font.HorizontalSpacing) * label.Length - font.HorizontalSpacing + 8);
        Height = font.CharacterHeight + 4;
        Label = label;
        Click = click;
        Font = font;
    }

    public void Draw(GraphicsManager graphics, bool isHovered)
    {
        var fillColor = isHovered ? VNSettings.ChoiceHoveredBackgroundColor : VNSettings.ChoiceBackgroundColor;
        var textColor = isHovered ? VNSettings.ChoiceHoveredTextColor : VNSettings.ChoiceTextColor;

        graphics.DrawFilledRectangle(X, Y, Width, Height, fillColor);
        graphics.DrawText(Font, X + 4, Y + 2, Label, textColor);
    }
}
