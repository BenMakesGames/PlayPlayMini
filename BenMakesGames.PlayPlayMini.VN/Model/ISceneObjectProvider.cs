using System.Collections.Frozen;

namespace BenMakesGames.PlayPlayMini.VN.Model;

public interface ISceneObjectProvider
{
    T Get<T>() where T: class;
    object Get(Type type);
}

public sealed class SceneObjectProvider: ISceneObjectProvider
{
    private readonly FrozenDictionary<Type, object> Objects;

    public SceneObjectProvider(Dictionary<Type, object> objects)
    {
        Objects = objects.ToFrozenDictionary();
    }

    public T Get<T>() where T: class => (T)Objects[typeof(T)];

    public object Get(Type type) => Objects[type];
}
