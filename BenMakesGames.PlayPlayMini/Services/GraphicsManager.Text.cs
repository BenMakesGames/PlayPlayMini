using System.Linq;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;
using BenMakesGames.PlayPlayMini.Extensions;
using BenMakesGames.PlayPlayMini.Model;

namespace BenMakesGames.PlayPlayMini.Services;

public sealed partial class GraphicsManager
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Rectangle FontRectangle(FontSheet font, int character) => new(
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
    public (int x, int y) DrawText(FontSheet font, int x, int y, string text, Color color)
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
    public (int x, int y) DrawText(Font font, int x, int y, string text, Color color)
    {
        if (font.Sheets.Count == 1)
            return DrawText(font.Sheets[0], x, y, text, color);

        var currentX = x;
        var currentY = y;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (c is '\r' or '\n')
            {
                currentX = x;
                currentY += font.LineHeight;
            }
            else if (font.TryGetSheet(c, out var sheet))
            {
                DrawTexture(sheet!.Texture, currentX, currentY, FontRectangle(sheet, c - sheet.FirstCharacter), color);
                currentX += sheet.CharacterWidth + sheet.HorizontalSpacing;
            }
        }

        return (currentX, currentY);
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
    public (int x, int y) DrawText(FontSheet font, int x, int y, char character, Color color)
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
    /// Use a multi-sheet font to draw a single character.
    /// </summary>
    /// <remarks>
    /// When the font has a single sheet, this delegates to the <see cref="FontSheet"/>-typed overload. With multiple
    /// sheets, the character is routed through <see cref="Font.TryGetSheet"/>; if no sheet covers it, nothing is
    /// drawn and the cursor does not advance. Line advances use <see cref="Font.LineHeight"/>.
    /// </remarks>
    /// <param name="font"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="character"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public (int x, int y) DrawText(Font font, int x, int y, char character, Color color)
    {
        if (font.Sheets.Count == 1)
            return DrawText(font.Sheets[0], x, y, character, color);

        var position = (x, y);

        if (character is '\r' or '\n')
        {
            position.x = x;
            position.y += font.LineHeight;
        }
        else if (font.TryGetSheet(character, out var sheet))
        {
            DrawTexture(sheet!.Texture, position.x, position.y, FontRectangle(sheet, character - sheet.FirstCharacter), color);
            position.x += sheet.CharacterWidth + sheet.HorizontalSpacing;
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
    public (int Width, int Height) ComputeDimensionsWithWordWrap(FontSheet font, int maxWidth, string text)
    {
        var wrappedLines = text.WrapText(font, maxWidth).Split('\n');
        var longestLine = wrappedLines.Max(l => l.Length);

        return (
            longestLine == 0 ? 0 : (longestLine * (font.CharacterWidth + font.HorizontalSpacing) - font.HorizontalSpacing),
            wrappedLines.Length == 0 ? 0 : (wrappedLines.Length * (font.CharacterHeight + font.VerticalSpacing) - font.VerticalSpacing)
        );
    }

    /// <summary>
    /// Compute the total width and height needed to draw text within a given width, using a given multi-sheet font.
    /// </summary>
    /// <remarks>
    /// When the font has a single sheet, this delegates to the <see cref="FontSheet"/>-typed overload. With multiple
    /// sheets, the text is wrapped via <see cref="Extensions.StringExtensions.WrapText(string, Font, int)"/> and
    /// then measured by <see cref="Font.ComputeWidth"/> and <see cref="Font.ComputeHeight"/>.
    /// </remarks>
    /// <param name="font"></param>
    /// <param name="maxWidth"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public (int Width, int Height) ComputeDimensionsWithWordWrap(Font font, int maxWidth, string text)
    {
        if (font.Sheets.Count == 1)
            return ComputeDimensionsWithWordWrap(font.Sheets[0], maxWidth, text);

        var wrapped = text.WrapText(font, maxWidth);

        return (font.ComputeWidth(wrapped), font.ComputeHeight(wrapped));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int, int) DrawTextWithWordWrap(string fontName, int x, int y, int maxWidth, string text, Color color)
        => DrawTextWithWordWrap(Fonts[fontName], x, y, maxWidth, text, color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int, int) DrawTextWithWordWrap(FontSheet font, int x, int y, int maxWidth, string text, Color color)
        => DrawText(font, x, y, text.WrapText(font, maxWidth), color);

    /// <summary>
    /// Use a multi-sheet font to draw text, wrapping it to fit within a given width.
    /// </summary>
    /// <remarks>
    /// When the font has a single sheet, this delegates to the <see cref="FontSheet"/>-typed overload. With multiple
    /// sheets, the text is wrapped via <see cref="Extensions.StringExtensions.WrapText(string, Font, int)"/> and
    /// then drawn by the <see cref="Font"/>-typed <see cref="DrawText(Font, int, int, string, Color)"/>.
    /// </remarks>
    /// <param name="font"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="maxWidth"></param>
    /// <param name="text"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int, int) DrawTextWithWordWrap(Font font, int x, int y, int maxWidth, string text, Color color)
        => font.Sheets.Count == 1
            ? DrawTextWithWordWrap(font.Sheets[0], x, y, maxWidth, text, color)
            : DrawText(font, x, y, text.WrapText(font, maxWidth), color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int, int) PretendDrawText(string fontName, int x, int y, string text)
        => PretendDrawText(Fonts[fontName], x, y, text);

    public (int, int) PretendDrawText(FontSheet font, int x, int y, string text)
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

    /// <summary>
    /// Compute the cursor position that would result from drawing <paramref name="text"/> with this multi-sheet
    /// font, without actually drawing anything. Newlines reset <c>x</c> and advance <c>y</c> by
    /// <see cref="Font.LineHeight"/>; characters that no sheet covers are skipped.
    /// </summary>
    /// <remarks>
    /// When the font has a single sheet, this delegates to the <see cref="FontSheet"/>-typed overload.
    /// </remarks>
    /// <param name="font"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public (int, int) PretendDrawText(Font font, int x, int y, string text)
    {
        if (font.Sheets.Count == 1)
            return PretendDrawText(font.Sheets[0], x, y, text);

        var currentX = x;
        var currentY = y;

        foreach (var c in text)
        {
            if (c is '\r' or '\n')
            {
                currentX = x;
                currentY += font.LineHeight;
            }
            else if (font.TryGetSheet(c, out var sheet))
            {
                currentX += sheet!.CharacterWidth + sheet.HorizontalSpacing;
            }
        }

        return (currentX, currentY);
    }
}
