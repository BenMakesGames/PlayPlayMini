using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions.ParticleEffects;

/// <summary>
/// "Tosses" a sprite; it spins as it moves, and is pulled to the bottom of the screen by gravity.
/// </summary>
public sealed class TumblingSprite : IParticle
{
    private string SpriteSheet { get; }
    private int SpriteIndex { get; }

    private double CenterX { get; set; }
    private double CenterY { get; set; }
    private int MaxY { get; }
    private double Angle { get; set; }
    private double SpinSpeed { get; }
    private double XVelocity { get; set; }
    private double YVelocity { get; set; }

    public double Gravity { get; set; } = 0.2;

    /// <inheritdoc />
    public bool IsAlive => CenterY < MaxY;

    public TumblingSprite(string spriteSheet, int spriteIndex, int x, int y, double xVelocity, double yVelocity, double spinSpeed, int maxY)
    {
        SpriteSheet = spriteSheet;
        SpriteIndex = spriteIndex;
        CenterX = x;
        CenterY = y;
        XVelocity = xVelocity;
        YVelocity = yVelocity;
        SpinSpeed = spinSpeed;
        MaxY = maxY;
    }

    /// <inheritdoc />
    public void Draw(GraphicsManager graphics)
    {
        graphics.DrawSpriteRotatedAndScaled(SpriteSheet, (int)CenterX, (int)CenterY, SpriteIndex, (float)Angle, 1, Color.White);
    }

    /// <inheritdoc />
    public void Update(IParticleSpawner particleSpawner, GameTime gameTime)
    {
        CenterX += XVelocity * gameTime.ElapsedGameTime.TotalSeconds * 60;
        CenterY += YVelocity * gameTime.ElapsedGameTime.TotalSeconds * 60;
        Angle += SpinSpeed * gameTime.ElapsedGameTime.TotalSeconds * 60;

        YVelocity += Gravity * gameTime.ElapsedGameTime.TotalSeconds * 60;
    }
}
