namespace BenMakesGames.PlayPlayMini.Model;

/// <param name="Key">Name that uniquely identifies this song</param>
/// <param name="Path">Relative path to song, excluding file extension (ex: "Music/TownTheme")</param>
/// <param name="PreLoaded">Whether to load this resource BEFORE entering the first GameState</param>
public sealed record SongMeta(string Key, string Path, bool PreLoaded = false) : IAsset
{
    /// <param name="keyAndPath">If the key and path are the same in your application, use this constructor</param>
    /// <param name="preLoaded">Whether to load this resource BEFORE entering the first GameState</param>
    public SongMeta(string keyAndPath, bool preLoaded = false)
        : this(keyAndPath, keyAndPath, preLoaded)
    {
    }
}
