using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini;

public abstract class GameState
{
    /// <summary>
    /// This method is called once per frame. It's intended to be used to capture inputs from input devices,
    /// such as the KeyboardManager, MouseManager, or a gamepad. Simpler games may get away with doing this
    /// work in an Update method, instead.
    /// </summary>
    /// <param name="gameTime"></param>
    public virtual void Input(GameTime gameTime) { }

    /// <summary>
    /// This method is called once per frame. It's intended to be used to run your game logic.
    /// </summary>
    /// <param name="gameTime"></param>
    public virtual void Update(GameTime gameTime) { }

    /// <summary>
    /// This method is called an average of 60 times per second, regardless of the current frame rate. This can be
    /// useful for certain physics-based updates.
    ///
    /// If you configured your application to use a fixed time step, then Update will ALSO be called about 60 times
    /// per second. In that case, there's no reason to use both Update and FixedUpdate.
    /// </summary>
    /// <param name="gameTime"></param>
    public virtual void FixedUpdate(GameTime gameTime) { }

    /// <summary>
    /// This method is called once per frame.
    /// </summary>
    /// <param name="gameTime"></param>
    public virtual void Draw(GameTime gameTime) { }

    /// <summary>
    /// This method is called when the GameStateManager's current state changes to this state. It runs before the
    /// first Input, Update, FixedUpdate, or Draw method is called.
    /// </summary>
    public virtual void Enter() { }

    /// <summary>
    /// This method is called when the GameStateManager's current state changes away from this state.
    /// </summary>
    public virtual void Leave() { }
}
