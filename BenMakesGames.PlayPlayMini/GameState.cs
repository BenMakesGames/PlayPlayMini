using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini;

public abstract class GameState
{
    public virtual void ActiveInput(GameTime gameTime) { }
    public virtual void ActiveUpdate(GameTime gameTime) { }
    public virtual void AlwaysUpdate(GameTime gameTime) { }
    public virtual void ActiveDraw(GameTime gameTime) { }
    public virtual void AlwaysDraw(GameTime gameTime) { }

    public virtual void Enter() { }
    public virtual void Leave() { }
}