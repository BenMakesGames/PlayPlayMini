using Microsoft.Xna.Framework.Graphics;

namespace BenMakesGames.PlayPlayMini.Model;

/// <param name="Key">Name that uniquely identifies this picture</param>
/// <param name="Path">Relative path to image, excluding file extension (ex: "Pictures/GameOver")</param>
/// <param name="PreLoaded">Whether or not to load this resource BEFORE entering the first GameState</param>
public sealed record PictureMeta(string Key, string Path, bool PreLoaded = false) : GameAsset<Texture2D>(Path, PreLoaded);