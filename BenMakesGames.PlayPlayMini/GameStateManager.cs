using Autofac;
using BenMakesGames.PlayPlayMini.Attributes.DI;
using BenMakesGames.PlayPlayMini.Model;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.CompilerServices;

namespace BenMakesGames.PlayPlayMini;

[AutoRegister(Lifetime.Singleton)]
public sealed class GameStateManager: Game
{
    public GameState? CurrentState { get; private set; }
    public GameState? NextState { get; private set; }

    private ILifetimeScope IoCContainer { get; }
    private GraphicsManager Graphics { get; }
    private ServiceWatcher ServiceWatcher { get; }

    public AssetCollection Assets { get; set; } = new();
    public Type? InitialGameState { get; set; }
    public (int Width, int Height, int Zoom) InitialWindowSize { get; set; }
    public string InitialWindowTitle { get; set; } = "Untitled Game";

    public GameStateManager(
        ILifetimeScope iocContainer, GraphicsManager graphics, ServiceWatcher serviceWatcher,
        SoundManager soundManager
    )
    {
        IoCContainer = iocContainer;
        Graphics = graphics;
        ServiceWatcher = serviceWatcher;

        Content.RootDirectory = "Content";

        // TODO: this is dumb and bad (not extensible), and must be fixed/replaced:
        graphics.SetGame(this);
        soundManager.SetGame(this);
    }

    protected override void LoadContent()
    {
        base.LoadContent();

        foreach (var s in ServiceWatcher.ContentLoadingServices)
            s.LoadContent(this);
    }

    protected override void Initialize()
    {
        foreach (var s in ServiceWatcher.InitializedServices)
            s.Initialize(this);

        IsMouseVisible = false; // configurable via MouseManager

        base.Initialize(); // calls LoadContent, btw

        Window.Title = InitialWindowTitle;

        ChangeState(InitialGameState!);
    }

    private void Input(GameTime gameTime)
    {
        foreach(var s in ServiceWatcher.InputServices)
            s.Input(gameTime);

        CurrentState?.ActiveInput(gameTime);
    }

    protected override void Update(GameTime gameTime)
    {
        SwitchState();
        Input(gameTime);

        base.Update(gameTime);

        foreach (var s in ServiceWatcher.UpdatedServices)
            s.Update(gameTime);

        if (CurrentState != null)
        {
            CurrentState.AlwaysUpdate(gameTime);
            CurrentState.ActiveUpdate(gameTime);
        }
    }

    protected override bool BeginDraw()
    {
        Graphics.BeginDraw();
        return base.BeginDraw();
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        if (CurrentState != null)
        {
            CurrentState.AlwaysDraw(gameTime);
            CurrentState.ActiveDraw(gameTime);
        }

        foreach (var s in ServiceWatcher.DrawnServices)
            s.Draw(gameTime);
    }

    protected override void EndDraw()
    {
        Graphics.EndDraw();
        base.EndDraw();
    }

    private void SwitchState()
    {
        if(NextState == null)
            return;

        CurrentState?.Leave();

        CurrentState = NextState;
        NextState = null;

        CurrentState.Enter();
    }

    public void ChangeState(GameState nextState)
    {
        if (NextState != null)
            throw new ArgumentException("A next state is already ready!");

        NextState = nextState;
    }

    public GameState ChangeState(Type T)
    {
        if (NextState != null)
            throw new ArgumentException("A next state is already ready!");

        NextState = CreateState(T);

        return NextState;
    }

    [Obsolete("Use CurrentState, instead")]
    public bool IsCurrentState(GameState state) => CurrentState == state;

    [Obsolete("Use CurrentState, instead")]
    public bool IsCurrentState<T>() where T : GameState => CurrentState is T;

    public T ChangeState<T>() where T : GameState
    {
        if (NextState != null)
            throw new ArgumentException("A next state is already ready!");

        NextState = CreateState<T>();

        return (T)NextState;
    }

    public T ChangeState<T, TConfig>(TConfig config) where T: GameState
    {
        if (NextState != null)
            throw new ArgumentException("A next state is already ready!");

        NextState = CreateState<T, TConfig>(config);

        return (T)NextState;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T CreateState<T>() where T: GameState => IoCContainer.Resolve<T>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T CreateState<T, TConfig>(TConfig config) where T : GameState
        => IoCContainer.Resolve<T>(new TypedParameter(typeof(TConfig), config));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GameState CreateState(Type T) => (GameState)IoCContainer.Resolve(T);
}