namespace BenMakesGames.PlayPlayMini.Model;

/// <param name="Key">Name that uniquely identifies this font</param>
/// <param name="Path">Relative path to image, excluding file extension (ex: "Fonts/Consolas")</param>
/// <param name="Width">Width of an individual character</param>
/// <param name="Height">Height of an individual character</param>
/// <param name="PreLoaded">Whether to load this resource BEFORE entering the first GameState</param>
public sealed record FontMeta(string Key, string Path, int Width, int Height, bool PreLoaded = false) : IAsset
{
    public char FirstCharacter { get; init; } = ' ';
    public int HorizontalSpacing { get; init; } = 1;
    public int VerticalSpacing { get; init; } = 1;

    /// <param name="keyAndPath">If the key and path are the same in your application, use this constructor</param>
    /// <param name="width">Width of an individual character</param>
    /// <param name="height">Height of an individual character</param>
    /// <param name="preLoaded">Whether to load this resource BEFORE entering the first GameState</param>
    public FontMeta(string keyAndPath, int width, int height, bool preLoaded = false)
        : this(keyAndPath, keyAndPath, width, height, preLoaded)
    {
    }
}
