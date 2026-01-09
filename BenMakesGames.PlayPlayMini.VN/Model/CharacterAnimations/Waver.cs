using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.VN.Model.CharacterAnimations;

/// <summary>
/// Causes a character to waver back and forth as well as up and down. The animation loops until it is explicitly removed.
/// </summary>
/// <example>
/// Add it with:
/// * `.AddAnimation("character id", new Waver())`, OR
/// * `.AddAnimation(character, new Waver())`
///
/// When you'd like to remove it:
/// * `.RemoveAnimations&lt;Waver&gt;("character id")`, OR
/// * `.RemoveAnimations&lt;Waver&gt;(character)`
///
/// See `RemoveAnimations` for more ways to remove animations.
/// </example>
public sealed class Waver: ICharacterAnimation
{
    public bool IsAlive => true;

    private double AnimationTime { get; set; }

    public void Update(GameTime gameTime)
    {
        AnimationTime += gameTime.ElapsedGameTime.TotalSeconds;
    }

    public void Apply(CharacterPosition characterPosition)
    {
        characterPosition.X += (int)(3.9 * Math.Sin(AnimationTime * Math.PI / 2));
        characterPosition.Y += (int)(1.9 * Math.Sin((AnimationTime + 0.1) * Math.PI)) + 1;
    }
}
