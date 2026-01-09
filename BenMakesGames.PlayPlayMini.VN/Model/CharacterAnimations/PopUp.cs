using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.VN.Model.CharacterAnimations;

/// <see cref="FallDown" />
/// <remarks>
/// This animation is likely to be deprecated in the future, in favor of a more general character movement & positioning system.
/// </remarks>
public sealed class PopUp: ICharacterAnimation
{
    public bool IsAlive => true;

    private int GraphicsHeight { get; }

    private double AnimationTime { get; set; }

    public PopUp(GraphicsManager graphics)
    {
        GraphicsHeight = graphics.Height;
    }

    public void Update(GameTime gameTime)
    {
        AnimationTime += gameTime.ElapsedGameTime.TotalSeconds;
    }

    public void Apply(CharacterPosition characterPosition)
    {
        characterPosition.Y = Math.Max(characterPosition.Y, GraphicsHeight - (int)(Math.Sqrt(AnimationTime) * 200));
    }
}
