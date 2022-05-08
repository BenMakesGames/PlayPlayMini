namespace BenMakesGames.PlayPlayMini.Model;

public struct SongMeta
{
    public string Key { get; set; }
    public string Path { get; set; }

    /// <param name="key"></param>
    /// <param name="path">Relative path to image, excluding file extension (ex: "Music/TownTheme")</param>
    /// <param name="preLoaded">Whether or not to load this resource BEFORE entering the first IGameState</param>
    public SongMeta(string key, string path, bool preLoaded = false)
    {
        Key = key;
        Path = path;
    }
}