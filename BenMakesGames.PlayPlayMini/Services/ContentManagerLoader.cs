using Microsoft.Xna.Framework.Content;

namespace BenMakesGames.PlayPlayMini.Services;

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