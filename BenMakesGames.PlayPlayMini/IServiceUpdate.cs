using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini;

/// <summary>
/// Services that implement this interface will have their Update method called during the
/// <see cref="GameState.Update"/> method.
/// </summary>
public interface IServiceUpdate
{
    /// <summary>
    /// Called during PlayPlayMini's Update step. Should not be called manually.
    /// </summary>
    /// <param name="gameTime"></param>
    void Update(GameTime gameTime);
}
