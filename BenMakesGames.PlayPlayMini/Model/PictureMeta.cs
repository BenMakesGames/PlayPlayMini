namespace BenMakesGames.PlayPlayMini.Model;

public struct PictureMeta
{
    public string Key { get; set; }
    public string Path { get; set; }
    public bool PreLoaded { get; set; }

    /// <param name="key"></param>
    /// <param name="path">Relative path to image, excluding file extension (ex: "Pictures/GameOver")</param>
    /// <param name="preLoaded">Whether or not to load this resource BEFORE entering the first IGameState</param>
    public PictureMeta(string key, string path, bool preLoaded = false)
    {
        Key = key;
        Path = path;
        PreLoaded = preLoaded;
    }
}