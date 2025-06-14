using System;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions.ParticleEffects;

public sealed class FloatAwayPicture: IParticle
{
    public required string PictureName { get; init; }
    public required int StartingX { get; init; }
    public required int StartingY { get; init; }
    public required double Angle { get; init; }
    public required double Speed { get; init; }
    public required double TTL { get; init; }
    public double TimeAlive { get; private set; }

    /// <inheritdoc />
    public bool IsAlive => TimeAlive < TTL;

    public int X => (int)(StartingX + Math.Cos(Angle) * TimeAlive * Speed);
    public int Y => (int)(StartingY - Math.Sin(Angle) * TimeAlive * Speed);

    /// <inheritdoc />
    public void Update(IParticleSpawner particleSpawner, GameTime gameTime)
    {
        TimeAlive += gameTime.ElapsedGameTime.TotalSeconds;
    }

    /// <inheritdoc />
    public void Draw(GraphicsManager graphics)
    {
        graphics.DrawPicture(PictureName, X, Y);
    }
}
