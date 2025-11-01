using Autofac;
using BenMakesGames.PlayPlayMini.Attributes.DI;
using BenMakesGames.PlayPlayMini.Model;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;

namespace BenMakesGames.PlayPlayMini;

[AutoRegister]
public sealed class GameStateManager: Game
{
    /// <summary>
    /// The current state of the game.
    /// </summary>
    /// <example>
    /// <code>
    /// if(GSM.CurrentState == this)
    ///     Mouse.Draw(gameTime);
    /// </code>
    /// </example>
    public AbstractGameState CurrentState { get; private set; }

    public AbstractGameState? NextState { get; private set; }

    private ILifetimeScope IoCContainer { get; }
    private GraphicsManager Graphics { get; }
    private ServiceWatcher ServiceWatcher { get; }

    public AssetCollection Assets => Config.Assets;

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

    /// <inheritdoc />
    protected override void Initialize()
    {
        Window.Title = Config.InitialWindowTitle; // https://community.monogame.net/t/cant-set-window-title-in-game1-constructor/9465
        IsMouseVisible = false;

        foreach (var s in ServiceWatcher.InitializedServices)
            s.Initialize(this);

        base.Initialize(); // calls LoadContent, btw

        ChangeState(Config.InitialGameState);
    }

    /// <inheritdoc />
    protected override void LoadContent()
    {
        base.LoadContent();

        foreach (var s in ServiceWatcher.ContentLoadingServices)
            s.LoadContent(this);
    }

    /// <inheritdoc />
    protected override void UnloadContent()
    {
        base.UnloadContent();

        foreach (var s in ServiceWatcher.ContentLoadingServices)
            s.UnloadContent();
    }

    private void Input(GameTime gameTime)
    {
        // ReSharper disable once ForCanBeConvertedToForeach
        // for loop, instead of foreach, reduces allocations
        for(var i = 0; i < ServiceWatcher.InputServices.Count; i++)
            ServiceWatcher.InputServices[i].Input(gameTime);

        CurrentState.Input(gameTime);
    }

    /// <inheritdoc />
    protected override void Update(GameTime gameTime)
    {
        SwitchState();

        if(!IsActive && LostFocusGameState is not null)
        {
            var currentStateType = CurrentState.GetType();

            if (currentStateType != LostFocusGameState && currentStateType != Config.InitialGameState && currentStateType != typeof(NoState))
            {
                ChangeState(LostFocusGameState, new LostFocusConfig(CurrentState));
                SwitchState();
            }
        }

        Input(gameTime);

        base.Update(gameTime);

        // ReSharper disable once ForCanBeConvertedToForeach
        // for loop, instead of foreach, reduces allocations
        for (var i = 0; i < ServiceWatcher.UpdatedServices.Count; i++)
            ServiceWatcher.UpdatedServices[i].Update(gameTime);

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

    /// <inheritdoc />
    protected override void Draw(GameTime gameTime)
    {
        Graphics.BeginDraw();

        using (Graphics.WithShader((Effect?)null))
        {
            base.Draw(gameTime);

            CurrentState.Draw(gameTime);

            foreach (var s in ServiceWatcher.DrawnServices)
                s.Draw(gameTime);
        }

        Graphics.EndDraw();
    }

    private void SwitchState()
    {
        while (NextState is not null)
        {
            CurrentState.Leave();

            CurrentState = NextState;
            NextState = null;

            CurrentState.Enter();
        }
    }

    /// <summary>
    /// Queue up a state change. It will be changed at the beginning of the next frame's Input step.
    /// </summary>
    /// <seealso cref="ChangeState{T}()"/>
    /// <param name="nextState"></param>
    /// <exception cref="InvalidOperationException">If ChangeState was already called this update cycle.</exception>
    public void ChangeState(AbstractGameState nextState)
    {
        if (NextState is not null)
            throw new InvalidOperationException("A next state is already ready!");

        NextState = nextState;
    }

    /// <summary>
    /// Queue up a state change. It will be changed at the beginning of the next frame's Input step.
    /// </summary>
    /// <seealso cref="ChangeState{T}()"/>
    /// <param name="nextStateType"></param>
    /// <exception cref="InvalidOperationException">If ChangeState was already called this update cycle.</exception>
    public AbstractGameState ChangeState(Type nextStateType)
    {
        if (NextState is not null)
            throw new InvalidOperationException("A next state is already ready!");

        NextState = CreateState(nextStateType);

        return NextState;
    }

    public AbstractGameState ChangeState<TConfig>(Type nextStateType, TConfig config)
    {
        if (NextState is not null)
            throw new InvalidOperationException("A next state is already ready!");

        NextState = CreateState(nextStateType, config);

        return NextState;
    }

    /// <summary>
    /// Queue up a state change. It will be changed at the beginning of the next frame's Input step.
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
    /// Queue up a state change. It will be changed at the beginning of the next frame's Input step.
    ///
    /// The given config object will be passed to the constructor of the new state.
    /// </summary>
    /// <example><code>
    /// ChangeState&lt;Playing, PlayingConfig&gt;(new PlayingConfig(123, "abc"));
    ///&nbsp;
    /// ...
    ///&nbsp;
    /// public sealed class Playing: GameState&lt;PlayingConfig&gt;
    /// {
    ///     public Playing(PlayingConfig config, ...)
    ///     {
    ///         ...
    ///     }
    /// }
    ///&nbsp;
    /// public sealed record PlayingConfig(int Foo, string Bar);
    /// </code></example>
    /// <param name="config"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TConfig"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">If ChangeState was already called this update cycle.</exception>
    public T ChangeState<T, TConfig>(TConfig config) where T: GameState<TConfig>
    {
        if (NextState is not null)
            throw new InvalidOperationException("A next state is already ready!");

        NextState = CreateState<T, TConfig>(config);

        return (T)NextState;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T CreateState<T>() where T: GameState => IoCContainer.Resolve<T>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T CreateState<T, TConfig>(TConfig config) where T : GameState<TConfig>
        => IoCContainer.Resolve<T>(new TypedParameter(typeof(TConfig), config));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GameState CreateState(Type nextStateType)
    {
        if(!nextStateType.IsSubclassOf(typeof(GameState)))
            throw new ArgumentException("nextStateType must be a GameState", nameof(nextStateType));

        return (GameState)IoCContainer.Resolve(nextStateType);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GameState<TConfig> CreateState<TConfig>(Type nextStateType, TConfig config)
    {
        if(!nextStateType.IsSubclassOf(typeof(GameState<TConfig>)))
            throw new ArgumentException("nextStateType must be a GameState", nameof(nextStateType));

        return (GameState<TConfig>)IoCContainer.Resolve(nextStateType, new TypedParameter(typeof(TConfig), config));
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
