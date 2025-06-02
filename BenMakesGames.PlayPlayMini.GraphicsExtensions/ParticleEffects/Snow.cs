using System;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions.ParticleEffects;

public sealed class Snow: IParticle
{
    private Color Color { get; }
    private double Y { get; set; }
    private double X { get; set; }
    private double Wiggle { get; set; }
    private int MaxY { get; }
    private int Size { get; }
    private double Speed { get; }

    /// <inheritdoc />
    public bool IsAlive => Y < MaxY;

    public Snow(Color color, int maxY, int graphicsWidth)
    {
        Color = color;
        MaxY = maxY;
        Y = -2;
        Speed = Random.Shared.NextDouble() * 0.5 + 0.75;
        Wiggle = Random.Shared.NextDouble() * 100;
        Size = Random.Shared.Next(2) + 1;
        X = Random.Shared.Next(graphicsWidth + 30);
    }

    /// <inheritdoc />
    public void Update(GameTime gameTime)
    {
        X -= gameTime.ElapsedGameTime.TotalSeconds * 4 * Size * Speed;
        Y += gameTime.ElapsedGameTime.TotalSeconds * 16 * Size * Speed;
        Wiggle += gameTime.ElapsedGameTime.TotalSeconds * 2 * Speed / Size;
    }

    /// <inheritdoc />
    public void Draw(GraphicsManager graphics)
    {
        if (Size < 1)
            return;

        var pixelX = (int)(X + Math.Cos(Wiggle) * 5.5);
        graphics.DrawFilledRectangle(pixelX, (int)Y, Size, Size, Color);
    }
}
