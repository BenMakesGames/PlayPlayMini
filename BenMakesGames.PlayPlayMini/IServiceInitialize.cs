namespace BenMakesGames.PlayPlayMini;

public interface IServiceInitialize
{
    /// <summary>
    /// Called during PlayPlayMini's Initialize step. Should not be called manually.
    /// </summary>
    void Initialize(PlayPlayMiniApp app);
}