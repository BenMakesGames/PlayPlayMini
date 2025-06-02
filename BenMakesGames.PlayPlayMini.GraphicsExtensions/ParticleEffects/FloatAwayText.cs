using System;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions.ParticleEffects;

public sealed class FloatAwayText: IParticle
{
    public required string Text { get; init; }
    public required Color FillColor { get; init; }
    public required Color OutlineColor { get; init; }
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
    public void Update(GameTime gameTime)
    {
        TimeAlive += gameTime.ElapsedGameTime.TotalSeconds;
    }

    /// <inheritdoc />
    public void Draw(GraphicsManager graphics)
    {
        graphics.DrawTextWithOutline("Font", X, Y, Text, FillColor, OutlineColor);
    }
}
