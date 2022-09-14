using BenchmarkDotNet.Attributes;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.Performance;

/// <summary>
/// Benchmark to determine whether base GameState should be an Interface, or Abstract,
/// AND how strongly to encourage developers to seal their game states.
///
/// BenchmarkDotNet=v0.13.2, OS=linuxmint 20.3
/// Intel Core i5-10210U CPU 1.60GHz, 1 CPU, 8 logical and 4 physical cores
/// .NET SDK=6.0.400
///   [Host]     : .NET 6.0.8 (6.0.822.36306), X64 RyuJIT AVX2
///   DefaultJob : .NET 6.0.8 (6.0.822.36306), X64 RyuJIT AVX2
///
///
/// |                                          Method |      Mean |     Error |    StdDev | Allocated |
/// |------------------------------------------------ |----------:|----------:|----------:|----------:|
/// |          CallingLifecycleEvents_SealedInterface | 1.2710 ns | 0.0223 ns | 0.0208 ns |         - |
/// |            CallingLifecycleEvents_OpenInterface | 1.2904 ns | 0.0254 ns | 0.0238 ns |         - |
/// |           CallingLifecycleEvents_SealedAbstract | 0.6178 ns | 0.0162 ns | 0.0144 ns |         - |
/// |             CallingLifecycleEvents_OpenAbstract | 1.2570 ns | 0.0294 ns | 0.0246 ns |         - |
/// | CallingLifecycleEvents_SealedOverridingAbstract | 0.5261 ns | 0.0192 ns | 0.0171 ns |         - |
/// |   CallingLifecycleEvents_OpenOverridingAbstract | 0.5828 ns | 0.0218 ns | 0.0193 ns |         - |
///
/// Suggesting that an Abstract base class is better, and that sealing should be strongly recommended
/// (on the assumption that most methods WON'T be overridden.)
/// </summary>
[MemoryDiagnoser(false)]
public class GameStateBenchmarks
{
    private GameTime GameTime { get; }

    private IGameState Sealed { get; }
    private IGameState Open { get; }
    private GameState SealedAbstract { get; }
    private GameState OpenAbstract { get; }
    private GameState SealedOverridingAbstract { get; }
    private GameState OpenOverridingAbstract { get; }

    private sealed class SealedIGameState : IGameState
    {
        public void ActiveDraw(GameTime gameTime) { }
    }

    private class OpenAbstractGameState : GameState
    {
    }

    private sealed class SealedAbstractGameState : GameState
    {
    }

    private abstract class GameState
    {
        public virtual void ActiveDraw(GameTime gameTime)
        {
        }
    }

    private class OpenOverridingAbstractGameState : GameState
    {
        public override void ActiveDraw(GameTime gameTime) { }
    }

    private sealed class SealedOverridingAbstractGameState : GameState
    {
        public override void ActiveDraw(GameTime gameTime) { }
    }

    private interface IGameState
    {
        void ActiveDraw(GameTime gameTime);
    }

    private class OpenIGameState : IGameState
    {
        public void ActiveDraw(GameTime gameTime) { }
    }

    public GameStateBenchmarks()
    {
        GameTime = new GameTime();
        Sealed = new SealedIGameState();
        Open = new OpenIGameState();
        SealedAbstract = new SealedAbstractGameState();
        OpenAbstract = new OpenAbstractGameState();
        SealedOverridingAbstract = new SealedOverridingAbstractGameState();
        OpenOverridingAbstract = new OpenOverridingAbstractGameState();
    }

    [Benchmark]
    public void CallingLifecycleEvents_SealedInterface()
    {
        Sealed.ActiveDraw(GameTime);
    }

    [Benchmark]
    public void CallingLifecycleEvents_OpenInterface()
    {
        Open.ActiveDraw(GameTime);
    }

    [Benchmark]
    public void CallingLifecycleEvents_SealedAbstract()
    {
        SealedAbstract.ActiveDraw(GameTime);
    }

    [Benchmark]
    public void CallingLifecycleEvents_OpenAbstract()
    {
        Open.ActiveDraw(GameTime);
    }

    [Benchmark]
    public void CallingLifecycleEvents_SealedOverridingAbstract()
    {
        SealedOverridingAbstract.ActiveDraw(GameTime);
    }

    [Benchmark]
    public void CallingLifecycleEvents_OpenOverridingAbstract()
    {
        OpenOverridingAbstract.ActiveDraw(GameTime);
    }
}