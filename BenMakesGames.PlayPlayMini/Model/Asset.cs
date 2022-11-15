using System;
using BenMakesGames.PlayPlayMini.Services;

namespace BenMakesGames.PlayPlayMini.Model;

public abstract record Asset(Type AssetType, string Key, string Path, bool PreLoaded = false);

public abstract record Asset<T>(string Key, string Path, bool PreLoaded = false) : Asset(typeof(T), Key, Path, PreLoaded)
{
    public virtual T Load(ContentManagerLoader loader)
    {
        return loader.Load<T>(Path);
    }
}
