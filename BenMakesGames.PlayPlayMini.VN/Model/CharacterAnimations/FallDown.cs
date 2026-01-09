using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.VN.Model.CharacterAnimations;

/// <summary>
/// Causes a character to visibly fall down, off the screen. They move exponentially more quickly until they're off the bottom of the screen.
///
/// This animation does not end. If you want the character to appear again, you must remove this animation.
/// </summary>
/// <remarks>
/// This animation will not push a character further than juuuust off the bottom of the screen. Keep
/// this in mind when combining with other animations; the order you add the animations _could_ matter.
/// </remarks>
/// <remarks>
/// This animation is likely to be deprecated in the future, in favor of a more general character movement & positioning system.
/// </remarks>
/// <example>
/// Add it with:
/// * `.AddAnimation("character id", new FallDown())`, OR
/// * `.AddAnimation(character, new FallDown())`
///
/// Simply adding a `PopUp` animation to a character that has fallen will not have the desired effect. To correctly pop a character back up, use:
///
/// ```c#
/// .RemoveAnimations&lt;FallDown&gt;(character)
/// .AddAnimations(character, new PopUp())
/// ```
/// </example>
public sealed class FallDown: ICharacterAnimation
{
    public bool IsAlive => true;

    private int GraphicsHeight { get; }

    private double AnimationTime { get; set; }

    public FallDown(GraphicsManager graphics)
    {
        GraphicsHeight = graphics.Height;
    }

    public void Update(GameTime gameTime)
    {
        AnimationTime += gameTime.ElapsedGameTime.TotalSeconds;
    }

    public void Apply(CharacterPosition characterPosition)
    {
        characterPosition.Y = Math.Min(GraphicsHeight, characterPosition.Y + (int)(AnimationTime * AnimationTime * 300));
    }
}
