using BenMakesGames.PlayPlayMini;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace MyNamespace.GameStates;

// sealed classes execute faster than non-sealed, so always seal your game states!
public sealed class Playing: GameState
{
    private GraphicsManager Graphics { get; }
    private GameStateManager GSM { get; }
    private MouseManager Mouse { get; }

    public Playing(GraphicsManager graphics, GameStateManager gsm, MouseManager mouse)
    {
        Graphics = graphics;
        GSM = gsm;
        Mouse = mouse;
    }

    // overriding lifecycle methods is optional; feel free to delete any overrides you're not using.
    // note: you do NOT need to call the `base.` for lifecycle methods. so save some CPU cycles,
    // and don't call them :P

    public override void Input(GameTime gameTime)
    {
        // TODO: get input from keyboard, mouse, or gamepad (refer to PlayPlayMini documentation for more info)
    }

    public override void FixedUpdate(GameTime gameTime)
    {
        // TODO: update game objects based on user input, AI logic, etc
        // called 60 times per second regardless of frame rate; useful for physics logic
    }

    public override void Update(GameTime gameTime)
    {
        // TODO: update game objects based on user input, AI logic, etc
    }

    public override void Draw(GameTime gameTime)
    {
        // TODO: draw game scene (refer to PlayPlayMini documentation for more info)

        Graphics.Clear(Color.DarkSlateGray);

        Graphics.DrawText("Font", Graphics.Width / 2 - 30, Graphics.Height / 2 - 4, "Oh, hi! :D", Color.White);

        Mouse.Draw(this);
    }
}
