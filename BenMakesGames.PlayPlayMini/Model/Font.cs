using System.Collections.Generic;

namespace BenMakesGames.PlayPlayMini.Model;

public sealed record Font(List<FontSheet> Sheets)
{
    public Font(FontSheet fontSheet): this([ fontSheet ])
    {
    }

    /// <summary>
    /// The vertical advance, in pixels, used between lines of text drawn with this font. Equal to the maximum of
    /// <see cref="FontSheet.CharacterHeight"/> + <see cref="FontSheet.VerticalSpacing"/> across every sheet in
    /// <see cref="Sheets"/>, so a tall glyph contributed by one sheet is never clipped by a shorter sheet's line
    /// height. Computed once at construction.
    /// </summary>
    public int LineHeight { get; } = ComputeLineHeight(Sheets);

    /// <summary>
    /// The height, in pixels, of the tallest glyph in this font, ignoring vertical spacing. Equal to the maximum
    /// of <see cref="FontSheet.CharacterHeight"/> across every sheet in <see cref="Sheets"/>. Useful for laying out
    /// single-line text (vertical centering, row heights) where the inter-line gap of <see cref="LineHeight"/>
    /// is not wanted. Computed once at construction.
    /// </summary>
    public int MaxCharacterHeight { get; } = ComputeMaxCharacterHeight(Sheets);

    private int TrailingVerticalSpacing { get; } = ComputeTrailingVerticalSpacing(Sheets);

    private static int ComputeLineHeight(List<FontSheet> sheets)
    {
        var max = 0;

        foreach (var sheet in sheets)
        {
            var lineHeight = sheet.CharacterHeight + sheet.VerticalSpacing;

            if (lineHeight > max)
                max = lineHeight;
        }

        return max;
    }

    private static int ComputeMaxCharacterHeight(List<FontSheet> sheets)
    {
        var max = 0;

        foreach (var sheet in sheets)
        {
            if (sheet.CharacterHeight > max)
                max = sheet.CharacterHeight;
        }

        return max;
    }

    private static int ComputeTrailingVerticalSpacing(List<FontSheet> sheets)
    {
        var maxLineHeight = 0;
        var trailing = 0;

        foreach (var sheet in sheets)
        {
            var lineHeight = sheet.CharacterHeight + sheet.VerticalSpacing;

            if (lineHeight > maxLineHeight)
            {
                maxLineHeight = lineHeight;
                trailing = sheet.VerticalSpacing;
            }
        }

        return trailing;
    }

    /// <summary>
    /// Looks up which <see cref="FontSheet"/> in this font is responsible for drawing the given character.
    /// </summary>
    /// <remarks>
    /// Sheets are tried in the order they appear in <see cref="Sheets"/>; the first sheet whose character range
    /// contains <paramref name="c"/> wins. When building a multi-sheet font that mixes ranges (for example, a Latin
    /// sheet alongside a CJK sheet), put the more-specific range first so it is matched before any wider fallback.
    /// </remarks>
    /// <param name="c">The character to look up.</param>
    /// <param name="sheet">When this method returns <c>true</c>, the <see cref="FontSheet"/> that draws <paramref name="c"/>; otherwise <c>null</c>.</param>
    /// <returns><c>true</c> if a sheet in <see cref="Sheets"/> contains <paramref name="c"/>; otherwise <c>false</c>.</returns>
    public bool TryGetSheet(char c, out FontSheet? sheet)
    {
        foreach (var s in Sheets)
        {
            if (c >= s.FirstCharacter && c < s.LastCharacter)
            {
                sheet = s;
                return true;
            }
        }

        sheet = null;
        return false;
    }

    /// <summary>
    /// Returns the width of the string, in pixels. Newlines and carriage returns ARE accounted for.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public int ComputeWidth(string text)
    {
        var maxWidth = 0;
        var lineWidth = 0;
        var lastSpacing = 0;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (c == '\r')
                continue;

            if (c == '\n')
            {
                if (lineWidth - lastSpacing > maxWidth)
                    maxWidth = lineWidth - lastSpacing;

                lineWidth = 0;
                lastSpacing = 0;
                continue;
            }

            if (TryGetSheet(c, out var sheet))
            {
                lineWidth += sheet!.CharacterWidth + sheet.HorizontalSpacing;
                lastSpacing = sheet.HorizontalSpacing;
            }
        }

        if (lineWidth - lastSpacing > maxWidth)
            maxWidth = lineWidth - lastSpacing;

        return maxWidth;
    }

    /// <summary>
    /// Returns the height of the string, in pixels. Newlines are counted as line breaks; carriage returns are
    /// skipped (so <c>\r\n</c> contributes a single line break via the <c>\n</c>). The trailing
    /// <see cref="FontSheet.VerticalSpacing"/> of the tallest sheet is subtracted off the end so the bottom edge
    /// is the glyph edge, not the inter-line gap — mirroring the trailing-spacing convention of
    /// <see cref="ComputeWidth"/>.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <returns>The pixel height of <paramref name="text"/>, or <c>0</c> for empty input.</returns>
    public int ComputeHeight(string text)
    {
        if (text.Length == 0)
            return 0;

        var lineCount = 1;

        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
                lineCount++;
        }

        return lineCount * LineHeight - TrailingVerticalSpacing;
    }
}
