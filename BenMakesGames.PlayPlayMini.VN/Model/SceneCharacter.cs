using BenMakesGames.PlayPlayMini.Services;
using BenMakesGames.PlayPlayMini.VN.Extensions;
using BenMakesGames.PlayPlayMini.VN.Model.CharacterAnimations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BenMakesGames.PlayPlayMini.VN.Model;

public sealed class SceneCharacter
{
    public List<ICharacterAnimation> Animations { get; } = [ ];
    public CharacterPosition BasePosition { get; }

    private Character Character { get; }
    private CharacterPosition Position { get; }

    public SceneCharacter(Character character, int x, int y, bool flippedHorizontally)
    {
        Character = character;
        BasePosition = new()
        {
            X = x,
            Y = y,
            FlippedHorizontally = flippedHorizontally
        };
        Position = new();
    }

    public void Update(GameTime gameTime)
    {
        foreach (var animation in Animations)
            animation.Update(gameTime);

        // reset position
        Position.X = BasePosition.X;
        Position.Y = BasePosition.Y;
        Position.SpriteIndex = BasePosition.SpriteIndex;
        Position.FlippedHorizontally = BasePosition.FlippedHorizontally;
        Position.FlippedVertically = false;

        for (int i = Animations.Count - 1; i >= 0; i--)
        {
            if (Animations[i].IsAlive)
                Animations[i].Apply(Position);
            else
                Animations.RemoveAt(i);
        }
    }

    public void Draw(GraphicsManager graphics)
    {
        if (VNSettings.CharacterOutlineColor.HasValue)
        {
            graphics.DrawSpriteFlippedWithOutline(
                Character.SpriteSheet,
                (int)Position.X,
                (int)Position.Y,
                Position.SpriteIndex,
                (Position.FlippedHorizontally ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (Position.FlippedVertically ? SpriteEffects.FlipVertically : SpriteEffects.None),
                VNSettings.CharacterOutlineColor.Value
            );
        }
        else
        {
            graphics.DrawSpriteFlipped(
                Character.SpriteSheet,
                (int)Position.X,
                (int)Position.Y,
                Position.SpriteIndex,
                (Position.FlippedHorizontally ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (Position.FlippedVertically ? SpriteEffects.FlipVertically : SpriteEffects.None)
            );
        }
    }
}
