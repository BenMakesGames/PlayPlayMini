using System.Collections.Generic;
using System.Linq;

namespace BenMakesGames.PlayPlayMini.Model;

public class AssetCollection : List<Asset>
{
    public IEnumerable<Asset<T>> GetAll<T>() => this.Where(a => a.AssetType == typeof(T)).Cast<Asset<T>>();
    public IEnumerable<Asset<T>> GetPreloadable<T>() => GetAll<T>().Where(a => a.PreLoaded);
    public IEnumerable<Asset<T>> GetDeferred<T>() => GetAll<T>().Where(a => !a.PreLoaded);
}
