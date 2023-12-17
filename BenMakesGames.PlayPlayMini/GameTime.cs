using System;

namespace BenMakesGames.PlayPlayMini;

public class GameTime
{
    public TimeSpan ElapsedGameTime { get; private set; } = TimeSpan.Zero;
    public TimeSpan TotalGameTime { get; private set; } = TimeSpan.Zero;

    internal void Add(double deltaMs)
    {
        ElapsedGameTime = TimeSpan.FromMilliseconds(deltaMs);
        TotalGameTime += ElapsedGameTime;
    }
}