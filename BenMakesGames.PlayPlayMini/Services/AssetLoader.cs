namespace BenMakesGames.PlayPlayMini.Services;

public abstract class AssetLoader
{
    public abstract T Load<T>(string path);
}
