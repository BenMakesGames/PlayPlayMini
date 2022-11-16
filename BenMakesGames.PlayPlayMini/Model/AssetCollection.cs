using System.Collections.Generic;
using System.Linq;

namespace BenMakesGames.PlayPlayMini.Model;

public class AssetCollection : List<IAsset>
{
    public IEnumerable<T> GetAll<T>() => this.Where(a => a.GetType() == typeof(T)).Cast<T>();
}
