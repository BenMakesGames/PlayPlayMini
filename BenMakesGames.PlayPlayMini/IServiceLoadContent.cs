namespace BenMakesGames.PlayPlayMini;

public interface IServiceLoadContent
{
    /// <summary>
    /// True when all assets have been loaded.
    /// </summary>
    bool FullyLoaded { get; }

    /// <summary>
    /// Called during MonoGame's LoadContent step. Should not be called manually.
    /// </summary>
    /// <param name="gsm"></param>
    void LoadContent(GameStateManager gsm);
    
    /// <summary>
    /// Called during MonoGame's UnloadContent step. Should not be called manually.
    /// </summary>
    void UnloadContent();
}