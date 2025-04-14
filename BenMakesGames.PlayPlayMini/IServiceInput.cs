using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini;

/// <summary>
/// Services that implement this interface will have their Input method called during the
/// <see cref="GameState.Input"/> method.
/// </summary>
public interface IServiceInput
{
    void Input(GameTime gameTime);
}
