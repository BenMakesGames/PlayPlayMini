using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.VN.Model.CharacterAnimations;

/// <summary>
/// Causes a character to visibly bounce up and back down again over the course of a fifth of a second.
/// </summary>
/// <example>
/// Add it with:
/// * `.AddAnimation("character id", new Bounce())`, OR
/// * `.AddAnimation(character, new Bounce())`
/// </example>
public sealed class Bounce: ICharacterAnimation
{
    public bool IsAlive => AnimationTime < 0.2;

    private double AnimationTime { get; set; }

    public void Update(GameTime gameTime)
    {
        AnimationTime += gameTime.ElapsedGameTime.TotalSeconds;
    }

    public void Apply(CharacterPosition characterPosition)
    {
        characterPosition.Y -= (int)(3.9 * Math.Sin(AnimationTime * 5 * Math.PI));
    }
}
