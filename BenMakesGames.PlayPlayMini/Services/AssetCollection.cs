using BenMakesGames.PlayPlayMini.Model;
using System.Collections.Generic;
using System.Linq;

namespace BenMakesGames.PlayPlayMini.Services;

public class AssetCollection : List<IGameAsset>
{
    public IEnumerable<GameAsset<T>> GetAll<T>() => this.Where(a => a.AssetType == typeof(T)).Cast<GameAsset<T>>();
    public IEnumerable<GameAsset<T>> GetPreloadable<T>() => GetAll<T>().Where(a => a.PreLoaded);
    public IEnumerable<GameAsset<T>> GetDeferred<T>() => GetAll<T>().Where(a => !a.PreLoaded);
}
