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
        /// <param name="fontSheet"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public (int x, int y) DrawText(FontSheet fontSheet, int x, int y, ReadOnlySpan<char> text, Color color)
        {
            var position = (x, y);

            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] is '\r' or '\n')
                    position = (x, position.y + fontSheet.CharacterHeight + fontSheet.VerticalSpacing);
                else
                    position = graphics.DrawText(fontSheet, position.x, position.y, text[i], color);
            }

            return position;
        }

        /// <summary>
        /// Use a multi-sheet font to draw text. Newline characters are respected. For automatic wrapping, use DrawTextWithWordWrap.
        /// </summary>
        /// <remarks>
        /// When the font has a single sheet, this delegates to the <see cref="FontSheet"/>-typed overload for the
        /// fastest path. With multiple sheets, each character is routed through <see cref="Font.TryGetSheet"/> and
        /// drawn from the matched sheet; characters that no sheet covers are skipped (the cursor does not advance).
        /// Line advances use <see cref="Font.LineHeight"/> so glyphs from a tall sheet are not clipped by a shorter one.
        /// </remarks>
        /// <param name="font"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public (int x, int y) DrawText(Font font, int x, int y, ReadOnlySpan<char> text, Color color)
        {
            if (font.Sheets.Count == 1)
                return graphics.DrawText(font.Sheets[0], x, y, text, color);

            var position = (x, y);

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];

                if (c is '\r' or '\n')
                    position = (x, position.y + font.LineHeight);
                else if (font.TryGetSheet(c, out var sheet))
                    position = graphics.DrawText(sheet!, position.x, position.y, c, color);
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
        /// <param name="fontSheet"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public (int x, int y) DrawText(FontSheet fontSheet, int x, int y, Span<char> text, Color color)
        {
            var position = (x, y);

            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] is '\r' or '\n')
                    position = (x, position.y + fontSheet.CharacterHeight + fontSheet.VerticalSpacing);
                else
                    position = graphics.DrawText(fontSheet, position.x, position.y, text[i], color);
            }

            return position;
        }

        /// <summary>
        /// Use a multi-sheet font to draw text. Newline characters are respected. For automatic wrapping, use DrawTextWithWordWrap.
        /// </summary>
        /// <remarks>
        /// When the font has a single sheet, this delegates to the <see cref="FontSheet"/>-typed overload for the
        /// fastest path. With multiple sheets, each character is routed through <see cref="Font.TryGetSheet"/> and
        /// drawn from the matched sheet; characters that no sheet covers are skipped (the cursor does not advance).
        /// Line advances use <see cref="Font.LineHeight"/> so glyphs from a tall sheet are not clipped by a shorter one.
        /// </remarks>
        /// <param name="font"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public (int x, int y) DrawText(Font font, int x, int y, Span<char> text, Color color)
        {
            if (font.Sheets.Count == 1)
                return graphics.DrawText(font.Sheets[0], x, y, text, color);

            var position = (x, y);

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];

                if (c is '\r' or '\n')
                    position = (x, position.y + font.LineHeight);
                else if (font.TryGetSheet(c, out var sheet))
                    position = graphics.DrawText(sheet!, position.x, position.y, c, color);
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
        /// <param name="fontSheet"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public (int x, int y) DrawText(FontSheet fontSheet, int x, int y, char[] text, Color color)
            => graphics.DrawText(fontSheet, x, y, text.AsSpan(), color);

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
