using BenMakesGames.PlayPlayMini.Model;

namespace BenMakesGames.PlayPlayMini.NAudio.Model;

/// <param name="Key">Name that uniquely identifies this song</param>
/// <param name="Path">Relative path to image, excluding file extension (ex: "Music/TownTheme")</param>
/// <param name="PreLoaded">Whether or not to load this resource BEFORE entering the first GameState</param>
public sealed record NAudioSongMeta(string Key, string Path, bool PreLoaded = false) : IAsset;
