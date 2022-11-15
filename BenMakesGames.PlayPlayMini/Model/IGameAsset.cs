using Microsoft.Xna.Framework.Content;
using System;

namespace BenMakesGames.PlayPlayMini.Model;

public interface IGameAsset
{
    Type AssetType { get; }
    string Key { get; }
    string Path { get; }
    bool PreLoaded { get; }
}

public abstract record GameAsset<T>(string Key, string Path, bool PreLoaded = false) : IGameAsset
{
    public Type AssetType => typeof(T);
    public T? Content { get; protected set; }

    public virtual T Load(ContentManagerLoader loader)
    {
        return loader.Load<T>(Path);
    }
}

public abstract class AssetLoader
{
    public abstract T Load<T>(string path);
}

public sealed class ContentManagerLoader : AssetLoader
{
    public ContentManager ContentManager { get; }

    public ContentManagerLoader(ContentManager contentManager)
    {
        ContentManager = contentManager;
    }

    public override T Load<T>(string assetName) => ContentManager.Load<T>(assetName);

    public static implicit operator ContentManagerLoader(ContentManager contentManager) => new(contentManager);
}