using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.VN.Model.CharacterAnimations;

/// <summary>
/// Causes a character to wobble back and forth over the course of 400ms.
/// </summary>
/// <example>
/// Add it with:
/// * `.AddAnimation("character id", new Shake())`, OR
/// * `.AddAnimation(character, new Shake())`
/// </example>
public sealed class Shake: ICharacterAnimation
{
    public bool IsAlive => AnimationTime < 0.4;

    private double AnimationTime { get; set; }

    public void Update(GameTime gameTime)
    {
        AnimationTime += gameTime.ElapsedGameTime.TotalSeconds;
    }

    public void Apply(CharacterPosition characterPosition)
    {
        characterPosition.X += (int)(2.9 * Math.Sin(AnimationTime * 5 * Math.PI));
    }
}
