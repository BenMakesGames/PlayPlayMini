using System;
using BenMakesGames.PlayPlayMini.Model;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions;

/// <summary>
/// A set of extension methods for drawing text with an outline.
/// </summary>
public static class TextWithOutline
{
    extension(GraphicsManager graphics)
    {
        /// <summary>
        /// Draws text with an outline around it.
        /// </summary>
        /// <param name="fontName">The name of the font to use.</param>
        /// <param name="x">The x-coordinate of the text position.</param>
        /// <param name="y">The y-coordinate of the text position.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="fillColor">The color of the text fill.</param>
        /// <param name="outlineColor">The color of the outline.</param>
        public void DrawTextWithOutline(string fontName, int x, int y, string text, Color fillColor, Color outlineColor)
        {
            graphics.DrawText(fontName, x, y + 1, text, outlineColor);
            graphics.DrawText(fontName, x, y - 1, text, outlineColor);
            graphics.DrawText(fontName, x - 1, y, text, outlineColor);
            graphics.DrawText(fontName, x + 1, y, text, outlineColor);

            graphics.DrawText(fontName, x, y, text, fillColor);
        }

        /// <summary>
        /// Draws text with an outline around it.
        /// </summary>
        /// <param name="fontName">The name of the font to use.</param>
        /// <param name="x">The x-coordinate of the text position.</param>
        /// <param name="y">The y-coordinate of the text position.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="fillColor">The color of the text fill.</param>
        /// <param name="outlineColor">The color of the outline.</param>
        public void DrawTextWithOutline(string fontName, int x, int y, ReadOnlySpan<char> text, Color fillColor, Color outlineColor)
            => graphics.DrawTextWithOutline(graphics.Fonts[fontName], x, y, text, fillColor, outlineColor);

        /// <summary>
        /// Draws text with an outline around it.
        /// </summary>
        /// <param name="font">The font to use.</param>
        /// <param name="x">The x-coordinate of the text position.</param>
        /// <param name="y">The y-coordinate of the text position.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="fillColor">The color of the text fill.</param>
        /// <param name="outlineColor">The color of the outline.</param>
        public void DrawTextWithOutline(Font font, int x, int y, ReadOnlySpan<char> text, Color fillColor, Color outlineColor)
        {
            graphics.DrawText(font, x, y + 1, text, outlineColor);
            graphics.DrawText(font, x, y - 1, text, outlineColor);
            graphics.DrawText(font, x - 1, y, text, outlineColor);
            graphics.DrawText(font, x + 1, y, text, outlineColor);

            graphics.DrawText(font, x, y, text, fillColor);
        }
    }
}