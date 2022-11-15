namespace BenMakesGames.PlayPlayMini.Model;

/// <param name="Key">Name that uniquely identifies this font</param>
/// <param name="Path">Relative path to image, excluding file extension (ex: "Fonts/Consolas")</param>
/// <param name="Width">Width of an individual character</param>
/// <param name="Height">Height of an individual character</param>
public sealed record FontMeta(string Key, string Path, int Width, int Height) : Asset<FontMeta>;