using Microsoft.Xna.Framework;
using BenMakesGames.PlayPlayMini.Services;

namespace BenMakesGames.PlayPlayMini;

/// <remark>
/// Do not inherit from this class directly; your game states should inherit from one of
/// <see cref="GameState"/> or <see cref="GameState{TConfig}"/>.
/// </remark>
public abstract class AbstractGameState
{
    /// <summary>
    /// Called when input should be captured from input devices.
    /// </summary>
    /// <remarks>
    /// Override this method to capture input from a service such as <see cref="KeyboardManager"/>, <see cref="MouseManager"/>, etc.
    /// This method is called once per frame, immediately before <see cref="FixedUpdate(GameTime)"/> and <see cref="Update(GameTime)"/>.
    /// </remarks>
    /// <param name="gameTime">A <see cref="GameTime"/> instance containing the elapsed time since the last call to <see cref="Input"/> and the total time elapsed since the game started.</param>
    public virtual void Input(GameTime gameTime) { }

    /// <summary>
    /// Called when the game state should update
    /// </summary>
    /// <remarks>
    /// This method is called once per frame.
    /// </remarks>
    /// <seealso cref="FixedUpdate(GameTime)"/>
    /// <param name="gameTime">A <see cref="GameTime"/> instance containing the elapsed time since the last call to <see cref="Update"/> and the total time elapsed since the game started.</param>
    public virtual void Update(GameTime gameTime) { }

    /// <summary>
    /// Called when the game state should update.
    /// </summary>
    /// <remarks>
    /// This method is called an average of 60 times per second, regardless of the current frame rate. This can be useful for certain physics-based updates.
    /// If your <see cref="Game.IsFixedTimeStep"/> is set to <see langword="true" />, then <see cref="Update(GameTime)"/> will ALSO be called about 60 times per second.
    /// In that case, there's no reason to use both <see cref="Update(GameTime)"/> and <see cref="FixedUpdate(GameTime)"/>.
    /// <list>
    /// <listheader>See also:</listheader>
    /// <item><seealso cref="Update(GameTime)"/></item>
    /// </list>
    /// </remarks>
    /// <param name="gameTime">A <see cref="GameTime"/> instance containing the elapsed time since the last call to <see cref="Update"/> and the total time elapsed since the game started.</param>
    public virtual void FixedUpdate(GameTime gameTime) { }

    /// <summary>
    /// This method is called once per frame.
    /// </summary>
    /// <param name="gameTime">A <see cref="GameTime"/> instance containing the elapsed time since the last call to <see cref="Draw"/> and the total time elapsed since the game started.</param>
    public virtual void Draw(GameTime gameTime) { }

    /// <summary>
    /// This method is called when the <see cref="GameStateManager"/>'s current state changes to this state.
    /// It runs before the first <see cref="Input"/>, <see cref="Update"/>, <see cref="FixedUpdate"/>, or <see cref="Draw"/> method is called.
    /// </summary>
    public virtual void Enter() { }

    /// <summary>
    /// This method is called when the <see cref="GameStateManager"/>'s current state changes away from this state.
    /// </summary>
    public virtual void Leave() { }
}

/// <summary>
/// Inherit this class to create your own game states.
/// </summary>
/// <remarks>
/// If a game state needs access to a service, such as the <see cref="GameStateManager"/>, <see cref="GraphicsManager"/>, <see cref="SoundManager"/>, etc.,
/// include the service in the game state's constructor arguments. The IoC container will automatically inject them.
/// <para>
/// <example>
/// Example usage:
/// <code>
/// public sealed class MyGameState: GameState
/// {
///     private GameStateManager GSM { get; }
///     private GraphicsManager Graphics { get; }
///     private KeyboardManager Keyboard { get; }
///&nbsp;
///     public MyGameState(GameStateManager gsm, GraphicsManager graphics, KeyboardManager keyboard)
///     {
///         GSM = gsm;
///         Graphics = graphics;
///         Keyboard = keyboard;
///     }
///&nbsp;
///     public override void Input(GameTime gameTime)
///     {
///         if (Keyboard.KeyDown(Keys.Escape))
///             GSM.ChangeState&lt;MyPauseMenu&gt;(); // or maybe GSM.Exit();
///     }
/// }
/// </code>
/// </example>
/// </para>
/// </remarks>
public abstract class GameState: AbstractGameState
{
}

/// <summary>
/// Inherit this class to create your own game states.
/// </summary>
/// <remarks>
/// Use <see cref="GameState{TConfig}"/> instead of <see cref="GameState"/> when you want to require a configuration
/// object to be provided.
/// <para>
/// <example>
/// Example usage:
/// <code>
/// public sealed class MyGameState: GameState&lt;MyGameStateConfig&gt;
/// {
///     ...
///&nbsp;
///     public MyGameState(MyGameStateConfig config, ...)
///     {
///         this.SomeProperty = config.SomeProperty;
///     }
/// }
///&nbsp;
/// public sealed record MyGameStateConfig(int SomeProperty);
///&nbsp;
/// ...
///&nbsp;
/// GSM.ChangeState&lt;MyGameState&gt;(); // build error - config object is required
/// GSM.ChangeState&lt;MyGameState, MyGameStateConfig&gt;(new(3));
/// </code>
/// </example>
/// </para>
/// </remarks>
public abstract class GameState<TConfig>: AbstractGameState
{
}
