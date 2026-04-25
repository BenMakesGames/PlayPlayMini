using System.Collections.Generic;
using System.Linq;

namespace BenMakesGames.PlayPlayMini.Model;

public sealed class FontMeta: IAsset
{
    public string Key { get; }
    public bool PreLoaded { get; }
    public List<FontSheetMeta> FontSheets { get; }

    public FontMeta(string key, IEnumerable<FontSheetMeta> fontSheets, bool preLoaded = false)
    {
        Key = key;
        PreLoaded = preLoaded;
        FontSheets = fontSheets.ToList();
    }

    public FontMeta(string key, string path, int width, int height, bool preLoaded = false)
        : this(key, [ new FontSheetMeta(path, width, height) ], preLoaded)
    {
    }

    public FontMeta(string keyAndPath, int width, int height, bool preLoaded = false)
        : this(keyAndPath, [ new FontSheetMeta(keyAndPath, width, height) ], preLoaded)
    {
    }
}
