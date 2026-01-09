using BenMakesGames.PlayPlayMini.VN.Model.Dialog;
using BenMakesGames.PlayPlayMini.VN.Model.SceneInstructions;

namespace BenMakesGames.PlayPlayMini.VN.Model;

public sealed class StoryStep
{
    public List<ISceneInstructions> Instructions { get; } = new();
    public IDialog? Dialog { get; private set; }
    public Func<GameStateManager, ISceneController, AbstractGameState>? MiniGame { get; private set; }

    public StoryStep SetDialog(IDialog dialog)
    {
        Dialog = dialog;
        return this;
    }

    public StoryStep SetMiniGame<TGameState, TGameStateConfig>(TGameStateConfig config)
        where TGameState : GameState<TGameStateConfig>
        where TGameStateConfig : MiniGameConfig
    {
        MiniGame = (gsm, sceneController) =>
        {
            config.PreviousState = gsm.CurrentState;
            config.SceneController = sceneController;

            return gsm.CreateState<TGameState, TGameStateConfig>(config);
        };

        return this;
    }

    public StoryStep SetCharacterDialog(Character speaker, string text, int lines = 2)
    {
        Dialog = new CharacterDialog(text, speaker, lines);
        return this;
    }

    public StoryStep SetTransparentDialog(string text, bool invertedText = false, int lines = 2)
    {
        Dialog = new TransparentDialog(text, invertedText, lines);
        return this;
    }

    public StoryStep SetThinkingDialog(string text, int lines = 2)
    {
        Dialog = new ThinkingDialog(text, lines);
        return this;
    }
}

public abstract record MiniGameConfig
{
    internal AbstractGameState? PreviousState { get; set; }
    internal ISceneController? SceneController { get; set; }
}
