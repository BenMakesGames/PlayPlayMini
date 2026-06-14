using BenMakesGames.PlayPlayMini;
using BenMakesGames.PlayPlayMini.Model;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace MyNamespace.GameStates;

// inheriting game states is a path that leads to madness, so always seal your game states!
public sealed class LostFocus: GameState<LostFocusConfig>
{
    private GraphicsManager Graphics { get; }
    private GameStateManager GSM { get; }
    private MouseManager Mouse { get; }
    private AbstractGameState PreviousState { get; }

    public LostFocus(
        GraphicsManager graphics, GameStateManager gsm, MouseManager mouse,
        LostFocusConfig config
    )
    {
        Graphics = graphics;
        GSM = gsm;
        Mouse = mouse;

        PreviousState = config.PreviousState;
    }

    public override void Input(GameTime gameTime)
    {
        if(Mouse.LeftClicked)
            GSM.ChangeState(PreviousState);
    }

    public override void Draw(GameTime gameTime)
    {
        PreviousState.Draw(gameTime);

        Graphics.DrawFilledRectangle(Graphics.Width / 2 - 80, Graphics.Height / 2 - 14, 160, 28, Color.Black);

        Graphics.DrawText("Font", Graphics.Width / 2 - 70, Graphics.Height / 2 - 4, "Paused! Click to resume.", Color.White);

        Mouse.Draw(this);
    }
}
