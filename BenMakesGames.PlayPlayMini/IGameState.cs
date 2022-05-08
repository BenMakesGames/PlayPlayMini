using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini;

public interface IGameState
{
    void ActiveInput(GameTime gameTime);
    void ActiveUpdate(GameTime gameTime);
    void AlwaysUpdate(GameTime gameTime);
    void ActiveDraw(GameTime gameTime);
    void AlwaysDraw(GameTime gameTime);
}