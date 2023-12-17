using BenMakesGames.PlayPlayMini.Model;

namespace BenMakesGames.PlayPlayMini;

public interface IServiceLoadContent
{
    /// <summary>
    /// True when all assets have been loaded.
    /// </summary>
    bool FullyLoaded { get; }

    /// <summary>
    /// Called during PlayPlayMini's LoadContent step. Should not be called manually.
    /// </summary>
    void LoadContent(AssetCollection assetCollection);
}