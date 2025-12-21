using System;
using BenMakesGames.PlayPlayMini.Model;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions;

/// <summary>
/// Extension methods for drawing text with spans and arrays.
/// </summary>
public static class DrawTextWithSpans
{
    /// <param name="graphics"></param>
    extension(GraphicsManager graphics)
    {
        /// <summary>
        /// Use a font to draw text. Newline characters are respected. For automatic wrapping, use DrawTextWithWordWrap.
        /// </summary>
        /// <param name="fontName"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public (int x, int y) DrawText(string fontName, int x, int y, ReadOnlySpan<char> text, Color color)
            => graphics.DrawText(graphics.Fonts[fontName], x, y, text, color);

        /// <summary>
        /// Use a font to draw text. Newline characters are respected. For automatic wrapping, use DrawTextWithWordWrap.
        /// </summary>
        /// <param name="font"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public (int x, int y) DrawText(Font font, int x, int y, ReadOnlySpan<char> text, Color color)
        {
            var position = (x, y);

            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] is '\r' or '\n')
                    position = (x, position.y + font.CharacterHeight + font.VerticalSpacing);
                else
                    position = graphics.DrawText(font, position.x, position.y, text[i], color);
            }

            return position;
        }

        /// <summary>
        /// Use a font to draw text. Newline characters are respected. For automatic wrapping, use DrawTextWithWordWrap.
        /// </summary>
        /// <param name="fontName"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public (int x, int y) DrawText(string fontName, int x, int y, Span<char> text, Color color)
            => graphics.DrawText(graphics.Fonts[fontName], x, y, text, color);

        /// <summary>
        /// Use a font to draw text. Newline characters are respected. For automatic wrapping, use DrawTextWithWordWrap.
        /// </summary>
        /// <param name="font"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public (int x, int y) DrawText(Font font, int x, int y, Span<char> text, Color color)
        {
            var position = (x, y);

            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] is '\r' or '\n')
                    position = (x, position.y + font.CharacterHeight + font.VerticalSpacing);
                else
                    position = graphics.DrawText(font, position.x, position.y, text[i], color);
            }

            return position;
        }

        /// <summary>
        /// Use a font to draw text. Newline characters are respected. For automatic wrapping, use DrawTextWithWordWrap.
        /// </summary>
        /// <param name="fontName"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public (int x, int y) DrawText(string fontName, int x, int y, char[] text, Color color)
            => graphics.DrawText(graphics.Fonts[fontName], x, y, text.AsSpan(), color);

        /// <summary>
        /// Use a font to draw text. Newline characters are respected. For automatic wrapping, use DrawTextWithWordWrap.
        /// </summary>
        /// <param name="font"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public (int x, int y) DrawText(Font font, int x, int y, char[] text, Color color)
            => graphics.DrawText(font, x, y, text.AsSpan(), color);
    }
}