using System;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions.ParticleEffects;

/// <summary>
/// A circle that shrinks over time.
/// </summary>
public sealed class ShrinkingCircle: IParticle
{
    public required float CenterX { get; set; }
    public required float CenterY { get; set; }
    public required float Radius { get; set; }
    public required Color Color { get; set; }

    /// <summary>
    /// How quickly the circle should shink, in pixels per second.
    /// </summary>
    public required float ShrinkRate { get; set; }

    /// <summary>
    /// The velocity of the circle, in pixels per second.
    /// </summary>
    public Vector2 Velocity { get; set; } = Vector2.Zero;

    /// <inheritdoc />
    public bool IsAlive => Radius > 0;

    /// <inheritdoc />
    public void Update(IParticleSpawner particleSpawner, GameTime gameTime)
    {
        Radius -= (float)(gameTime.ElapsedGameTime.TotalSeconds * ShrinkRate);
        CenterX += Velocity.X * (float)gameTime.ElapsedGameTime.TotalSeconds;
        CenterY += Velocity.Y * (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    /// <inheritdoc />
    public void Draw(GraphicsManager graphics)
    {
        graphics.DrawFilledCircle(
            (int)Math.Floor(CenterX),
            (int)Math.Floor(CenterY),
            (int)Math.Ceiling(Radius),
            Color
        );
    }
}