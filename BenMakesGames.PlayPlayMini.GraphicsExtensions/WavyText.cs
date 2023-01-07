using System;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions;

public static class WavyText
{
    public static void DrawWavyText(this GraphicsManager graphics, string fontName, GameTime gameTime, string text, Color color)
    {
        var x = (graphics.Width - text.Length * graphics.Fonts[fontName].CharacterWidth) / 2;
        var y = (graphics.Height - graphics.Fonts[fontName].CharacterHeight) / 2;

        graphics.DrawWavyText(fontName, x, y, gameTime, text, color);
    }

    public static void DrawWavyText(this GraphicsManager graphics, string fontName, GameTime gameTime, string text)
        => graphics.DrawWavyText(fontName, gameTime, text, Color.White);

    public static void DrawWavyText(this GraphicsManager graphics, string fontName, int y, GameTime gameTime, string text, Color color)
    {
        var x = (graphics.Width - text.Length * graphics.Fonts[fontName].CharacterWidth) / 2;

        graphics.DrawWavyText(fontName, x, y, gameTime, text, color);
    }

    public static void DrawWavyText(this GraphicsManager graphics, string fontName, int y, GameTime gameTime, string text)
        => graphics.DrawWavyText(fontName, y, gameTime, text, Color.White);

    public static void DrawWavyText(this GraphicsManager graphics, string fontName, int x, int y, GameTime gameTime, string text, Color color)
    {
        for (var i = 0; i < text.Length; i++)
        {
            var yOffset = (int) (Math.Cos(gameTime.TotalGameTime.TotalSeconds * 8 + i / 3.0) * 1.95);

            graphics.DrawText(fontName, x + i * 6, y + yOffset, text[i], color);
        }
    }

    public static void DrawWavyText(this GraphicsManager graphics, string fontName, int x, int y, GameTime gameTime, string text)
        => graphics.DrawWavyText(fontName, x, y, gameTime, text, Color.White);
}