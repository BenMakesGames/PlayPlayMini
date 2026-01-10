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

    public StoryStep SetCharacterDialog(Character speaker, string text, int? rows = null)
    {
        Dialog = new CharacterDialog(text, speaker, rows ?? VNSettings.DialogDefaultRows);
        return this;
    }

    public StoryStep SetTransparentDialog(string text, bool invertedText = false, int? rows = null)
    {
        Dialog = new TransparentDialog(text, invertedText, rows ?? VNSettings.DialogDefaultRows);
        return this;
    }

    public StoryStep SetThinkingDialog(string text, int? rows = null)
    {
        Dialog = new ThinkingDialog(text, rows ?? VNSettings.DialogDefaultRows);
        return this;
    }
}

public abstract record MiniGameConfig
{
    internal AbstractGameState? PreviousState { get; set; }
    internal ISceneController? SceneController { get; set; }
}
