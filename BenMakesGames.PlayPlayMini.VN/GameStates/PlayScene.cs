using Autofac;
using BenMakesGames.PlayPlayMini.Services;
using BenMakesGames.PlayPlayMini.VN.Model;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.VN.GameStates;

public sealed class PlayScene: GameState<PlaySceneConfig>
{
    private GameStateManager GSM { get; }
    private GraphicsManager Graphics { get; }
    private MouseManager Mouse { get; }

    private SceneController SceneController { get; }
    private Action OnComplete { get; }

    public PlayScene(
        PlaySceneConfig config,
        ILifetimeScope iocContainer, GraphicsManager graphics, MouseManager mouse, GameStateManager gsm
    )
    {
        Graphics = graphics;
        Mouse = mouse;
        GSM = gsm;

        SceneController = new SceneController(this, iocContainer, config.Steps);
        OnComplete = config.OnComplete;
    }

    /// <inheritdoc/>
    public override void Input(GameTime gameTime)
    {
        if (SceneController.IsDone)
            return;

        SceneController.Input(GSM, Graphics, Mouse);
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (SceneController.IsDone)
        {
            if(GSM.NextState is null)
                OnComplete();

            return;
        }

        SceneController.Update(gameTime);
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        SceneController.Draw(Graphics);

        if (!SceneController.IsDone)
            Mouse.Draw(this);
    }
}

public sealed record PlaySceneConfig(List<StoryStep> Steps, Action OnComplete, bool HideMouse = false);
