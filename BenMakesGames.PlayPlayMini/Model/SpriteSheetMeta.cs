namespace BenMakesGames.PlayPlayMini.Model;

/// <param name="Key">Name that uniquely identifies this sprite sheet</param>
/// <param name="Path">Relative path to image, excluding file extension (ex: "Characters/Nina")</param>
/// <param name="Width">Width of an individual sprite</param>
/// <param name="Height">Height of an individual sprite</param>
/// <param name="PreLoaded">Whether to load this resource BEFORE entering the first GameState</param>
public sealed record SpriteSheetMeta(string Key, string Path, int Width, int Height, bool PreLoaded = false) : IAsset
{
    /// <param name="keyAndPath">If the key and path are the same in your application, use this constructor</param>
    /// <param name="width">Width of an individual sprite</param>
    /// <param name="height">Height of an individual sprite</param>
    /// <param name="preLoaded">Whether to load this resource BEFORE entering the first GameState</param>
    public SpriteSheetMeta(string keyAndPath, int width, int height, bool preLoaded = false)
        : this(keyAndPath, keyAndPath, width, height, preLoaded)
    {
    }
}
