using Microsoft.Xna.Framework.Media;

namespace BenMakesGames.PlayPlayMini.Model;

/// <param name="Key">Name that uniquely identifies this song</param>
/// <param name="Path">Relative path to image, excluding file extension (ex: "Music/TownTheme")</param>
public sealed record SongMeta(string Key, string Path) : GameAsset<Song>(Key, Path);
