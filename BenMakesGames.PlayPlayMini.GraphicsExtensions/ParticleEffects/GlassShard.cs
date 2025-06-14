using System;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions.ParticleEffects;

/// <summary>
/// A tumbling shard of glass that launches away from its starting point, and falls under gravity.
/// </summary>
public sealed class GlassShard : IParticle
{
    /// <inheritdoc />
    public bool IsAlive => Y < MaxY + Size;

    private double X { get; set; }
    private double Y { get; set; }
    private double XSpeed { get; set; }
    private double YSpeed { get; set; }
    private int Size { get; }

    private double AngleY { get; set; }
    private double AngleZ { get; set; }

    private double YSpin { get; }
    private double ZSpin { get; }

    private int MaxY { get; }

    private Color Color { get; }

    public GlassShard(int startX, int startY, int maxY, Color color)
    {
        X = startX;
        Y = startY;
        Size = Random.Shared.Next(3) + 1;
        XSpeed = Random.Shared.NextDouble() * 4 - 2;
        YSpeed = -(Random.Shared.NextDouble() * 2 + 1);
        MaxY = maxY;
        Color = color;

        if(Random.Shared.Next(2) == 0)
        {
            YSpin = Random.Shared.NextDouble() / 2 + 0.5;
            ZSpin = Random.Shared.NextDouble();
        }
        else
        {
            YSpin = Random.Shared.NextDouble();
            ZSpin = Random.Shared.NextDouble() / 2 + 0.5;
        }
    }

    /// <inheritdoc />
    public void Draw(GraphicsManager graphics)
    {
        if(Size == 1)
        {
            graphics.DrawTexture(
                graphics.WhitePixel,
                (int)X, (int)Y,
                Color
            );
        }
        else
        {
            graphics.DrawTextureWithTransformations(
                graphics.WhitePixel,
                (int)(X - Size / 2f), (int)(Y - Size / 2f),
                new Rectangle(0, 0, 1, 1),
                Microsoft.Xna.Framework.Graphics.SpriteEffects.None,
                (float)AngleZ,
                Size,
                (float)(Size * Math.Cos(AngleY)),
                Color
            );
        }
    }

    /// <inheritdoc />
    public void Update(IParticleSpawner particleSpawner, GameTime gameTime)
    {
        YSpeed += 0.2 * gameTime.ElapsedGameTime.TotalSeconds * 60;

        Y += YSpeed * gameTime.ElapsedGameTime.TotalSeconds * 60;
        X += XSpeed * gameTime.ElapsedGameTime.TotalSeconds * 60;

        if(Size > 1)
        {
            AngleY += YSpin * gameTime.ElapsedGameTime.TotalSeconds * 60;
            AngleZ += ZSpin * gameTime.ElapsedGameTime.TotalSeconds * 60;
        }
    }
}
