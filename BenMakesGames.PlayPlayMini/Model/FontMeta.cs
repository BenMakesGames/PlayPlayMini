namespace BenMakesGames.PlayPlayMini.Model;

public struct FontMeta
{
    public string Key { get; set; }
    public string Path { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    /// <param name="key"></param>
    /// <param name="path">Relative path to image, excluding file extension (ex: "Fonts/Consolas")</param>
    /// <param name="width">Width of an individual character</param>
    /// <param name="height">Height of an individual character</param>
    public FontMeta(string key, string path, int width, int height)
    {
        Key = key;
        Path = path;
        Width = width;
        Height = height;
    }
}