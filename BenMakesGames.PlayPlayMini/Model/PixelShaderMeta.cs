namespace BenMakesGames.PlayPlayMini.Model;

public sealed record PixelShaderMeta(string Key, string Path, bool PreLoaded = false): IAsset;
