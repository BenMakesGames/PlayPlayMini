namespace BenMakesGames.PlayPlayMini.Model;

/// <param name="Key">Name that uniquely identifies this picture</param>
/// <param name="Path">Relative path to image, excluding file extension (ex: "Pictures/GameOver")</param>
/// <param name="PreLoaded">Whether to load this resource BEFORE entering the first GameState</param>
public sealed record PictureMeta(string Key, string Path, bool PreLoaded = false) : IAsset
{
    /// <param name="keyAndPath">If the key and path are the same in your application, use this constructor</param>
    /// <param name="preLoaded">Whether to load this resource BEFORE entering the first GameState</param>
    public PictureMeta(string keyAndPath, bool preLoaded = false)
        : this(keyAndPath, keyAndPath, preLoaded)
    {
    }
}
