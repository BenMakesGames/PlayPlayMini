using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions.ParticleEffects;

public sealed class FlashOfInsight: IParticle
{
    private Color Color { get; }
    private double TTL { get; set; } = 0.1;

    /// <inheritdoc />
    public bool IsAlive => TTL > 0;

    public FlashOfInsight(Color color)
    {
        Color = color;
    }

    /// <inheritdoc />
    public void Update(GameTime gameTime)
    {
        TTL -= gameTime.ElapsedGameTime.TotalSeconds;
    }

    /// <inheritdoc />
    public void Draw(GraphicsManager graphics)
    {
        var partingHeight = (int)(graphics.Height * TTL * 5);
        graphics.DrawFilledRectangle(0, 0, graphics.Width, partingHeight, Color);
        graphics.DrawFilledRectangle(0, graphics.Height - partingHeight, graphics.Width, partingHeight, Color);
    }
}
