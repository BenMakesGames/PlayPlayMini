namespace BenMakesGames.PlayPlayMini;

public interface IServiceInitialize
{
    /// <summary>
    /// Called during MonoGame's Initialize step. Should not be called manually.
    /// </summary>
    /// <param name="gsm"></param>
    void Initialize(GameStateManager gsm);
}