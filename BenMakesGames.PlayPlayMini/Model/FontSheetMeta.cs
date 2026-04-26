namespace BenMakesGames.PlayPlayMini.Model;

/// <param name="Path">Relative path to image, excluding file extension (ex: "Fonts/Consolas")</param>
/// <param name="Width">Width of an individual character</param>
/// <param name="Height">Height of an individual character</param>
public sealed record FontSheetMeta(string Path, int Width, int Height)
{
    /// <summary>
    /// The character that the first cell of the sheet represents. Glyph lookup at draw time is
    /// <c>cellIndex = c - FirstCharacter</c>, so a sheet whose first row of glyphs is space, exclamation
    /// mark, etc. should leave this at the default. For sheets that cover a non-Latin range (CJK, Cyrillic,
    /// custom symbols), set this to the codepoint of the first glyph in the sheet. Defaults to <c>' '</c> (space).
    /// </summary>
    public char FirstCharacter { get; init; } = ' ';

    /// <summary>
    /// Pixels inserted between glyphs at draw time. Set to <c>0</c> if your source image already has
    /// space between glyphs baked into each cell. Defaults to <c>1</c>.
    /// </summary>
    public int HorizontalSpacing { get; init; } = 1;

    /// <summary>
    /// Pixels inserted between lines at draw time. Set to <c>0</c> if your source image already has
    /// space between lines baked into each cell. Defaults to <c>1</c>.
    /// </summary>
    public int VerticalSpacing { get; init; } = 1;
}
