using Autofac;
using BenMakesGames.PlayPlayMini.Attributes.DI;
using BenMakesGames.PlayPlayMini.Model;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.CompilerServices;

namespace BenMakesGames.PlayPlayMini;

[AutoRegister]
public sealed class GameStateManager: Game
{
    public GameState CurrentState { get; private set; }
    public GameState? NextState { get; private set; }

    private ILifetimeScope IoCContainer { get; }
    private GraphicsManager Graphics { get; }
    private ServiceWatcher ServiceWatcher { get; }

    public AssetCollection Assets => Config.Assets;
    
    [Obsolete("Refer to Config.InitialWindowTitle instead.")] public (int Width, int Height, int Zoom) InitialWindowSize => Config.InitialWindowSize;
    [Obsolete("Refer to Config.InitialGameState instead.")] public Type InitialGameState => Config.InitialGameState;
    
    public Type? LostFocusGameState { get; set; }

    private double FixedUpdateAccumulator { get; set; }
    
    public GameStateManagerConfig Config { get; }

    public GameStateManager(
        ILifetimeScope iocContainer, GraphicsManager graphics, ServiceWatcher serviceWatcher,
        SoundManager soundManager, GameStateManagerConfig config
    )
    {
        IoCContainer = iocContainer;
        Graphics = graphics;
        ServiceWatcher = serviceWatcher;

        Content.RootDirectory = "Content";

        // TODO: this is dumb and bad (not extensible), and must be fixed/replaced:
        graphics.SetGame(this);
        soundManager.SetGame(this);

        Config = config;
        LostFocusGameState = config.InitialLostFocusGameState;
        
        CurrentState = new NoState();
    }

    protected override void LoadContent()
    {
        base.LoadContent();

        foreach (var s in ServiceWatcher.ContentLoadingServices)
            s.LoadContent(this);
    }

    protected override void UnloadContent()
    {
        base.UnloadContent();

        foreach (var s in ServiceWatcher.ContentLoadingServices)
            s.UnloadContent();
    }

    protected override void Initialize()
    {
        Window.Title = Config.InitialWindowTitle; // https://community.monogame.net/t/cant-set-window-title-in-game1-constructor/9465
        IsMouseVisible = false;

        foreach (var s in ServiceWatcher.InitializedServices)
            s.Initialize(this);

        base.Initialize(); // calls LoadContent, btw

        ChangeState(Config.InitialGameState);
    }

    private void Input(GameTime gameTime)
    {
        foreach(var s in ServiceWatcher.InputServices)
            s.Input(gameTime);

        CurrentState.Input(gameTime);
    }

    protected override void Update(GameTime gameTime)
    {
        SwitchState();

        if(!IsActive && LostFocusGameState is not null && CurrentState.GetType() != LostFocusGameState)
        {
            ChangeState(LostFocusGameState, new LostFocusConfig(CurrentState));
            SwitchState();
        }

        Input(gameTime);

        base.Update(gameTime);

        foreach (var s in ServiceWatcher.UpdatedServices)
            s.Update(gameTime);

        FixedUpdateAccumulator += gameTime.ElapsedGameTime.TotalMilliseconds;

        if (FixedUpdateAccumulator >= 16.6667)
        {
            CurrentState.FixedUpdate(new GameTime()
            {
                TotalGameTime = gameTime.TotalGameTime,
                IsRunningSlowly = gameTime.IsRunningSlowly,
                ElapsedGameTime = TimeSpan.FromMilliseconds(FixedUpdateAccumulator),
            });

            FixedUpdateAccumulator -= 16.6667;
        }

        CurrentState.Update(gameTime);
    }

    protected override bool BeginDraw()
    {
        Graphics.BeginDraw();
        return base.BeginDraw();
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        CurrentState.Draw(gameTime);

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
        if(NextState is null)
            return;

        CurrentState.Leave();

        CurrentState = NextState;
        NextState = null;

        CurrentState.Enter();
    }

    public void ChangeState(GameState nextState)
    {
        if (NextState is not null)
            throw new InvalidOperationException("A next state is already ready!");

        NextState = nextState;
    }

    public GameState ChangeState(Type nextStateType)
    {
        if (NextState is not null)
            throw new InvalidOperationException("A next state is already ready!");
        
        NextState = CreateState(nextStateType);

        return NextState;
    }

    public GameState ChangeState<TConfig>(Type nextStateType, TConfig config)
    {
        if (NextState is not null)
            throw new InvalidOperationException("A next state is already ready!");

        NextState = CreateState(nextStateType, config);

        return NextState;
    }

    /// <summary>
    /// Queue up a state change.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">If ChangeState was already called this update cycle.</exception>
    public T ChangeState<T>() where T : GameState
    {
        if (NextState is not null)
            throw new InvalidOperationException("A next state is already ready!");
        
        NextState = CreateState<T>();

        return (T)NextState;
    }

    /// <summary>
    /// Queue up a state change. The given config object will be passed to the constructor of the new state.
    ///
    /// <example><code>
    /// ChangeState&lt;Playing, PlayingConfig&gt;(new PlayingConfig(123, "abc"));
    /// 
    /// ...
    /// 
    /// public sealed class Playing: GameState
    /// {
    ///     public Playing(PlayingConfig config, GameStateManager gsm, GraphicsManager graphics, ...)
    ///     {
    ///         ...
    ///     }
    /// }
    /// 
    /// public sealed record PlayingConfig(int Foo, string Bar);
    /// </code></example>
    /// </summary>
    /// <param name="config"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TConfig"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">If ChangeState was already called this update cycle.</exception>
    public T ChangeState<T, TConfig>(TConfig config) where T: GameState
    {
        if (NextState is not null)
            throw new InvalidOperationException("A next state is already ready!");

        NextState = CreateState<T, TConfig>(config);

        return (T)NextState;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T CreateState<T>() where T: GameState => IoCContainer.Resolve<T>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T CreateState<T, TConfig>(TConfig config) where T : GameState
        => IoCContainer.Resolve<T>(new TypedParameter(typeof(TConfig), config));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GameState CreateState(Type nextStateType)
    {
        if(!nextStateType.IsSubclassOf(typeof(GameState)))
            throw new ArgumentException("nextStateType must be a GameState", nameof(nextStateType));

        return (GameState)IoCContainer.Resolve(nextStateType);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GameState CreateState<TConfig>(Type nextStateType, TConfig config)
    {
        if(!nextStateType.IsSubclassOf(typeof(GameState)))
            throw new ArgumentException("nextStateType must be a GameState", nameof(nextStateType));

        return (GameState)IoCContainer.Resolve(nextStateType, new TypedParameter(typeof(TConfig), config));
    }

    private sealed class NoState: GameState;
}

public sealed record GameStateManagerConfig(
    Type InitialGameState,
    Type? InitialLostFocusGameState,
    (int Width, int Height, int Zoom) InitialWindowSize,
    string InitialWindowTitle,
    AssetCollection Assets
);
