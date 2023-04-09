using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions;

public static class TextWithOutline
{
    /// <summary>
    /// Less performant that the PlayPlayMini built-in DrawTextWithOutline method (2.5x the draw calls), but easier to
    /// use, and in many cases you don't need the performance.
    /// </summary>
    public static void DrawTextWithOutline(this GraphicsManager graphics, string fontName, int x, int y, string text, Color fillColor, Color outlineColor)
    {
        graphics.DrawText(fontName, x, y + 1, text, outlineColor);
        graphics.DrawText(fontName, x, y - 1, text, outlineColor);
        graphics.DrawText(fontName, x - 1, y, text, outlineColor);
        graphics.DrawText(fontName, x - 1, y, text, outlineColor);

        graphics.DrawText(fontName, x, y, text, fillColor);
    }
}