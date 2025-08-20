using System.Linq;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;
using BenMakesGames.PlayPlayMini.Extensions;
using BenMakesGames.PlayPlayMini.Model;

namespace BenMakesGames.PlayPlayMini.Services;

public sealed partial class GraphicsManager
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Rectangle FontRectangle(Font font, int character) => new(
        (character % font.Columns) * font.CharacterWidth,
        (character / font.Columns) * font.CharacterHeight,
        font.CharacterWidth,
        font.CharacterHeight
    );

    /// <summary>
    /// Use a font to draw text. Newline characters are respected. For automatic wrapping, use DrawTextWithWordWrap.
    /// </summary>
    /// <param name="fontName"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="text"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int x, int y) DrawText(string fontName, int x, int y, string text, Color color)
        => DrawText(Fonts[fontName], x, y, text, color);

    /// <summary>
    /// Use a font to draw text. Newline characters are respected. For automatic wrapping, use DrawTextWithWordWrap.
    /// </summary>
    /// <param name="font"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="text"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public (int x, int y) DrawText(Font font, int x, int y, string text, Color color)
    {
        var position = (x, y);

        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] is '\r' or '\n')
                position = (x, position.y + font.CharacterHeight + font.VerticalSpacing);
            else
                position = DrawText(font, position.x, position.y, text[i], color);
        }

        return position;
    }

    /// <summary>
    /// Use a font to draw a single character.
    /// </summary>
    /// <param name="fontName"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="character"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int x, int y) DrawText(string fontName, int x, int y, char character, Color color)
        => DrawText(Fonts[fontName], x, y, character, color);

    /// <summary>
    /// Use a font to draw a single character.
    /// </summary>
    /// <param name="font"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="character"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public (int x, int y) DrawText(Font font, int x, int y, char character, Color color)
    {
        var position = (x, y);

        if (character is '\r' or '\n')
        {
            position.x = x;
            position.y += font.CharacterHeight + font.VerticalSpacing;
        }
        else if (character >= font.FirstCharacter)
        {
            DrawTexture(font.Texture, position.x, position.y, FontRectangle(font, character - font.FirstCharacter), color);

            position.x += font.CharacterWidth + font.HorizontalSpacing;
        }

        return position;
    }

    /// <summary>
    /// Compute the total width and height needed to draw text within a given width, using a given font.
    /// </summary>
    /// <param name="fontName"></param>
    /// <param name="maxWidth"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int Width, int Height) ComputeDimensionsWithWordWrap(string fontName, int maxWidth, string text)
        => ComputeDimensionsWithWordWrap(Fonts[fontName], maxWidth, text);

    /// <summary>
    /// Compute the total width and height needed to draw text within a given width, using a given font.
    /// </summary>
    /// <param name="font"></param>
    /// <param name="maxWidth"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public (int Width, int Height) ComputeDimensionsWithWordWrap(Font font, int maxWidth, string text)
    {
        var wrappedLines = text.WrapText(font, maxWidth).Split('\n');
        var longestLine = wrappedLines.Max(l => l.Length);

        return (
            longestLine == 0 ? 0 : (longestLine * (font.CharacterWidth + font.HorizontalSpacing) - font.HorizontalSpacing),
            wrappedLines.Length == 0 ? 0 : (wrappedLines.Length * (font.CharacterHeight + font.VerticalSpacing) - font.VerticalSpacing)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int, int) DrawTextWithWordWrap(string fontName, int x, int y, int maxWidth, string text, Color color)
        => DrawTextWithWordWrap(Fonts[fontName], x, y, maxWidth, text, color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int, int) DrawTextWithWordWrap(Font font, int x, int y, int maxWidth, string text, Color color)
        => DrawText(font, x, y, text.WrapText(font, maxWidth), color);

    public (int, int) PretendDrawText(Font font, int x, int y, string text)
    {
        var currentX = x;
        var currentY = y;

        foreach (var c in text)
        {
            if (c >= 32)
                currentX += font.CharacterWidth + font.HorizontalSpacing;
            else if (c == 10 || c == 13)
            {
                currentX = x;
                currentY += font.CharacterHeight + font.VerticalSpacing;
            }
        }

        return (currentX, currentY);
    }
}
