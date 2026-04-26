using System.Collections.Generic;
using System.Linq;

namespace BenMakesGames.PlayPlayMini.Model;

/// <summary>
/// Asset metadata for a bitmap font. A font is composed of one or more <see cref="FontSheetMeta"/>s; using more
/// than one sheet enables multi-lingual fonts (for example a Latin sheet alongside a CJK sheet) and fonts that
/// mix glyph sizes across character ranges.
/// </summary>
public sealed class FontMeta: IAsset
{
    /// <summary>Name that uniquely identifies this font.</summary>
    public string Key { get; }

    /// <summary>Whether to load this resource BEFORE entering the first GameState.</summary>
    public bool PreLoaded { get; }

    /// <summary>
    /// The font sheets that make up this font. Order matters: at draw time, the first sheet whose character
    /// range contains the glyph being drawn wins, so put more-specific ranges (e.g. a small Latin sheet)
    /// before any wider fallback (e.g. a large CJK sheet).
    /// </summary>
    public List<FontSheetMeta> FontSheets { get; }

    /// <summary>
    /// Creates a multi-sheet font. Use this constructor for multi-lingual fonts, or any font whose glyphs do
    /// not all share the same cell size.
    /// </summary>
    /// <param name="key">Name that uniquely identifies this font.</param>
    /// <param name="fontSheets">
    /// The sheets that make up this font. Order matters — see <see cref="FontSheets"/>.
    /// </param>
    /// <param name="preLoaded">Whether to load this resource BEFORE entering the first GameState.</param>
    public FontMeta(string key, IEnumerable<FontSheetMeta> fontSheets, bool preLoaded = false)
    {
        Key = key;
        PreLoaded = preLoaded;
        FontSheets = fontSheets.ToList();
    }

    /// <summary>
    /// Creates a single-sheet font. For multi-lingual or mixed-glyph-size fonts, use the
    /// <see cref="FontMeta(string, IEnumerable{FontSheetMeta}, bool)"/> constructor instead.
    /// </summary>
    /// <param name="key">Name that uniquely identifies this font.</param>
    /// <param name="path">Relative path to the font sheet image, excluding file extension (ex: "Fonts/Consolas").</param>
    /// <param name="width">Width of an individual character cell, in pixels.</param>
    /// <param name="height">Height of an individual character cell, in pixels.</param>
    /// <param name="horizontalSpacing">Pixels inserted between glyphs at draw time. Defaults to <c>1</c>.</param>
    /// <param name="verticalSpacing">Pixels inserted between lines at draw time. Defaults to <c>1</c>.</param>
    /// <param name="firstCharacter">The character that the first cell of the sheet represents. Defaults to <c>' '</c> (space).</param>
    /// <param name="preLoaded">Whether to load this resource BEFORE entering the first GameState.</param>
    public FontMeta(string key, string path, int width, int height, int horizontalSpacing = 1, int verticalSpacing = 1, char firstCharacter = ' ', bool preLoaded = false)
        : this(
            key,
            [
                new FontSheetMeta(path, width, height)
                {
                    HorizontalSpacing = horizontalSpacing,
                    VerticalSpacing = verticalSpacing,
                    FirstCharacter = firstCharacter
                }
            ],
            preLoaded
        )
    {
    }

    /// <summary>
    /// Creates a single-sheet font, using the same value for both the asset key and the sheet path. For
    /// multi-lingual or mixed-glyph-size fonts, use the
    /// <see cref="FontMeta(string, IEnumerable{FontSheetMeta}, bool)"/> constructor instead.
    /// </summary>
    /// <param name="keyAndPath">If the key and path are the same in your application, use this constructor.</param>
    /// <param name="width">Width of an individual character cell, in pixels.</param>
    /// <param name="height">Height of an individual character cell, in pixels.</param>
    /// <param name="horizontalSpacing">Pixels inserted between glyphs at draw time. Defaults to <c>1</c>.</param>
    /// <param name="verticalSpacing">Pixels inserted between lines at draw time. Defaults to <c>1</c>.</param>
    /// <param name="firstCharacter">The character that the first cell of the sheet represents. Defaults to <c>' '</c> (space).</param>
    /// <param name="preLoaded">Whether to load this resource BEFORE entering the first GameState.</param>
    public FontMeta(string keyAndPath, int width, int height, int horizontalSpacing = 1, int verticalSpacing = 1, char firstCharacter = ' ', bool preLoaded = false)
        : this(
            keyAndPath,
            [
                new FontSheetMeta(keyAndPath, width, height)
                {
                    HorizontalSpacing = horizontalSpacing,
                    VerticalSpacing = verticalSpacing,
                    FirstCharacter = firstCharacter
                }
            ],
            preLoaded
        )
    {
    }
}
