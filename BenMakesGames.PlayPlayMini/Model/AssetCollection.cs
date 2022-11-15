using System.Collections.Generic;
using System.Linq;

namespace BenMakesGames.PlayPlayMini.Model;

public class AssetCollection : List<Asset>
{
    public IEnumerable<T> GetAll<T>() => this.Where(a => a.AssetType == typeof(T)).Cast<T>();
}
