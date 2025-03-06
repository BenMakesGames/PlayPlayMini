using System;
using BenMakesGames.PlayPlayMini.Model;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions;

public static class DrawTextWithSpans
{
    /// <summary>
    /// Use a font to draw text. Newline characters are respected. For automatic wrapping, use DrawTextWithWordWrap.
    /// </summary>
    /// <param name="graphics"></param>
    /// <param name="fontName"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="text"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public static (int x, int y) DrawText(this GraphicsManager graphics, string fontName, int x, int y, ReadOnlySpan<char> text, Color color)
        => graphics.DrawText(graphics.Fonts[fontName], x, y, text, color);

    /// <summary>
    /// Use a font to draw text. Newline characters are respected. For automatic wrapping, use DrawTextWithWordWrap.
    /// </summary>
    /// <param name="graphics"></param>
    /// <param name="font"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="text"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public static (int x, int y) DrawText(this GraphicsManager graphics, Font font, int x, int y, ReadOnlySpan<char> text, Color color)
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
    /// <param name="graphics"></param>
    /// <param name="fontName"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="text"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public static (int x, int y) DrawText(this GraphicsManager graphics, string fontName, int x, int y, Span<char> text, Color color)
        => graphics.DrawText(graphics.Fonts[fontName], x, y, text, color);

    /// <summary>
    /// Use a font to draw text. Newline characters are respected. For automatic wrapping, use DrawTextWithWordWrap.
    /// </summary>
    /// <param name="graphics"></param>
    /// <param name="font"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="text"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public static (int x, int y) DrawText(this GraphicsManager graphics, Font font, int x, int y, Span<char> text, Color color)
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
    /// <param name="graphics"></param>
    /// <param name="fontName"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="text"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public static (int x, int y) DrawText(this GraphicsManager graphics, string fontName, int x, int y, char[] text, Color color)
        => graphics.DrawText(graphics.Fonts[fontName], x, y, text.AsSpan(), color);

    /// <summary>
    /// Use a font to draw text. Newline characters are respected. For automatic wrapping, use DrawTextWithWordWrap.
    /// </summary>
    /// <param name="graphics"></param>
    /// <param name="font"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="text"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public static (int x, int y) DrawText(this GraphicsManager graphics, Font font, int x, int y, char[] text, Color color)
        => graphics.DrawText(font, x, y, text.AsSpan(), color);
}
