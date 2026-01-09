using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.VN.Model.CharacterAnimations;

public interface ICharacterAnimation
{
    /// <summary>
    /// When an animation is no longer alive, it's automatically removed.
    /// </summary>
    bool IsAlive { get; }

    void Update(GameTime gameTime);

    /// <summary>
    /// Logic to apply the animation to the character.
    /// </summary>
    /// <param name="characterPosition">The character position to modify.</param>
    void Apply(CharacterPosition characterPosition);
}

public sealed class CharacterPosition
{
    public double X { get; set; }
    public double Y { get; set; }
    public int SpriteIndex { get; set; }
    public bool FlippedHorizontally { get; set; }
    public bool FlippedVertically { get; set; }
}
