using System;

namespace BenMakesGames.PlayPlayMini.Model;

public interface IGameAsset
{
    Type AssetType { get; }
    string Path { get; }
    bool PreLoaded { get; }
}

public abstract record GameAsset<T>(string Path, bool PreLoaded = false) : IGameAsset
{
    public Type AssetType => typeof(T);
}
