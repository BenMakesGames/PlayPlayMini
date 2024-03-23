using BenMakesGames.PlayPlayMini.Attributes.DI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace BenMakesGames.PlayPlayMini.Services;

/// <summary>
/// Counts the FPS of the game.
///
/// To draw the FPS on-screen, include the `FrameCounter` in a game state's constructor arguments, and perform a
/// `Graphics.DrawText(...)` call to draw the `FrameCounter`'s `AverageFramesPerSecond` and/or `CurrentFramesPerSecond`.
/// For example:
///
///     private FrameCounter FrameCounter { get; }
/// 
///     public MyGameState(FrameCounter frameCounter)
///     {
///         FrameCounter = frameCounter;
///     }
///
///     public override void Draw(GameTime gameTime)
///     {
///         Graphics.DrawText("Font", 4, 4, $"FPS: {FrameCounter.AverageFramesPerSecond}", Color.Red);
///     }
/// </summary>
[AutoRegister]
public sealed class FrameCounter: IServiceDraw
{
    public long TotalFrames { get; private set; }
    public float TotalSeconds { get; private set; }
    public float AverageFramesPerSecond { get; private set; }
    public float CurrentFramesPerSecond { get; private set; }

    public const int MaximumSamples = 60;

    private Queue<float> SampleBuffer { get;} = new();

    public void Draw(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        CurrentFramesPerSecond = 1.0f / deltaTime;

        SampleBuffer.Enqueue(CurrentFramesPerSecond);

        if (SampleBuffer.Count > MaximumSamples)
        {
            SampleBuffer.Dequeue();
            AverageFramesPerSecond = SampleBuffer.Average(i => i);
        }
        else
        {
            AverageFramesPerSecond = CurrentFramesPerSecond;
        }

        TotalFrames++;
        TotalSeconds += deltaTime;
    }
}
