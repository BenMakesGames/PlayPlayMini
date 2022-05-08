using BenMakesGames.PlayPlayMini.Attributes.DI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace BenMakesGames.PlayPlayMini.Services;

[AutoRegister(Lifetime.Singleton)]
public class FrameCounter: IServiceDraw
{
    public FrameCounter()
    {
    }

    public long TotalFrames { get; private set; }
    public float TotalSeconds { get; private set; }
    public float AverageFramesPerSecond { get; private set; }
    public float CurrentFramesPerSecond { get; private set; }

    public const int MAXIMUM_SAMPLES = 60;

    private Queue<float> SampleBuffer { get;} = new Queue<float>();

    public void Draw(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        CurrentFramesPerSecond = 1.0f / deltaTime;

        SampleBuffer.Enqueue(CurrentFramesPerSecond);

        if (SampleBuffer.Count > MAXIMUM_SAMPLES)
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