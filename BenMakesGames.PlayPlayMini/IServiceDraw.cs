using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini;

/// <summary>
/// Services that implement this interface will have their Draw method called during the
/// <see cref="GameState.Draw"/> method.
/// </summary>
public interface IServiceDraw
{
    void Draw(GameTime gameTime);
}
