using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions.GameStateTransitions;

/// <summary>
/// Extension methods for doing quick screen wipes between game states
/// </summary>
public static class GameStateManagerExtensions
{
    /// <param name="gsm"></param>
    extension(GameStateManager gsm)
    {
        public void ChangeStateWithScreenWipe<T>(ScreenWipeDirection direction,
            Color? color = null
        ) where T : GameState =>
            gsm.ChangeState<ScreenWipe, ScreenWipeConfig>(new ScreenWipeConfig()
            {
                PreviousState = gsm.CurrentState,
                NextState = gsm.CreateState<T>(),
                Color = color ?? Color.Black,
                Direction = direction
            });

        public void ChangeStateWithScreenWipe<T, TConfig>(TConfig config,
            ScreenWipeDirection direction,
            Color? color = null
        ) where T : GameState<TConfig> =>
            gsm.ChangeState<ScreenWipe, ScreenWipeConfig>(new ScreenWipeConfig()
            {
                PreviousState = gsm.CurrentState,
                NextState = gsm.CreateState<T, TConfig>(config),
                Color = color ?? Color.Black,
                Direction = direction
            });

        /// <summary>Change to the given game state, via a screen wipe.</summary>
        /// <param name="nextState">Game state to switch to.</param>
        /// <param name="direction">Direction of wipe.</param>
        /// <param name="color">Color of wipe (defaults to <see cref="P:Microsoft.Xna.Framework.Color.Black">Color.Black</see>)</param>
        public void ChangeStateWithScreenWipe(AbstractGameState nextState,
            ScreenWipeDirection direction,
            Color? color = null
        ) =>
            gsm.ChangeState<ScreenWipe, ScreenWipeConfig>(new ScreenWipeConfig()
            {
                PreviousState = gsm.CurrentState,
                NextState = nextState,
                Color = color ?? Color.Black,
                Direction = direction
            });
    }
}