namespace BenMakesGames.PlayPlayMini.Model;

/// <param name="Key">Name that uniquely identifies this font</param>
/// <param name="Path">Relative path to image, excluding file extension (ex: "Fonts/Consolas")</param>
/// <param name="Width">Width of an individual character</param>
/// <param name="Height">Height of an individual character</param>
/// <param name="PreLoaded">Whether or not to load this resource BEFORE entering the first GameState</param>
public sealed record FontMeta(string Key, string Path, int Width, int Height, bool PreLoaded = false): IAsset;