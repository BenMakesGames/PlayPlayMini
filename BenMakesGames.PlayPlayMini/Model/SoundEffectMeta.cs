using Microsoft.Xna.Framework.Audio;

namespace BenMakesGames.PlayPlayMini.Model;

/// <param name="Key">Name that uniquely identifies this sound effect</param>
/// <param name="Path">Relative path to image, excluding file extension (ex: "Sounds/TakeDamage")</param>
/// <param name="PreLoaded">Whether or not to load this resource BEFORE entering the first GameState</param>
public sealed record SoundEffectMeta(string Key, string Path, bool PreLoaded = false) : GameAsset<SoundEffect>(Key, Path, PreLoaded);