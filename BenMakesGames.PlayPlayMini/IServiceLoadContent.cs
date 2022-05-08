namespace BenMakesGames.PlayPlayMini;

public interface IServiceLoadContent
{
    bool FullyLoaded { get; }
    void LoadContent(GameStateManager gsm);
}