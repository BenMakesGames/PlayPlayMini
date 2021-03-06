using Autofac;
using BenMakesGames.PlayPlayMini.Attributes.DI;
using BenMakesGames.PlayPlayMini.Model;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace BenMakesGames.PlayPlayMini;

[AutoRegister(Lifetime.Singleton)]
public class GameStateManager: Game
{
    private IGameState? CurrentState { get; set; }
    private IGameState? NextState { get; set; }

    private ILifetimeScope IoCContainer { get; }
    private GraphicsManager Graphics { get; }
    private ServiceWatcher ServiceWatcher { get; }

    public Type InitialGameState { get; set; } = null!;

    // TODO: this is dumb and bad (not extensible), and must be fixed/replaced:
    public List<PictureMeta> Pictures { get; set; } = null!;
    public List<SpriteSheetMeta> SpriteSheets { get; set; } = null!;
    public List<FontMeta> Fonts { get; set; } = null!;
    public List<SoundEffectMeta> SoundEffects { get; set; } = null!;
    public List<SongMeta> Songs { get; set; } = null!;
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

        foreach (IServiceLoadContent s in ServiceWatcher.ContentLoadingServices)
            s.LoadContent(this);
    }

    protected override void UnloadContent()
    {
        base.UnloadContent();
    }

    protected override void Initialize()
    {
        foreach (IServiceInitialize s in ServiceWatcher.InitializedServices)
            s.Initialize(this);

        IsMouseVisible = false; // configurable via MouseManager

        base.Initialize(); // calls LoadContent, btw

        Window.Title = InitialWindowTitle;

        ChangeState(InitialGameState);
    }

    private void Input(GameTime gameTime)
    {
        foreach(IServiceInput s in ServiceWatcher.InputServices)
            s.Input(gameTime);

        if (CurrentState != null)
            CurrentState.ActiveInput(gameTime);
    }

    protected override void Update(GameTime gameTime)
    {
        SwitchState();
        Input(gameTime);

        base.Update(gameTime);

        foreach (IServiceUpdate s in ServiceWatcher.UpdatedServices)
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

        foreach (IServiceDraw s in ServiceWatcher.DrawnServices)
            s.Draw(gameTime);
    }

    protected override void EndDraw()
    {
        Graphics.EndDraw();
        base.EndDraw();
    }

    private void SwitchState()
    {
        if (NextState != null)
        {
            if (CurrentState != null && CurrentState is IGameStateLifecycleLeave leaving)
                leaving.Leave();

            CurrentState = NextState;
            NextState = null;

            if (CurrentState is IGameStateLifecycleEnter entering)
                entering.Enter();
        }
    }

    public void ChangeState(IGameState nextState)
    {
        if (NextState != null)
            throw new ArgumentException("a next state is already ready.");

        NextState = nextState;
    }

    public IGameState ChangeState(Type T)
    {
        if (NextState != null)
            throw new ArgumentException("a next state is already ready.");

        NextState = (IGameState)IoCContainer.Resolve(T);

        return NextState;
    }

    public T ChangeState<T>() where T : IGameState
    {
        if (NextState != null)
            throw new ArgumentException("a next state is already ready.");

        NextState = CreateState<T>();

        return (T)NextState;
    }

    public T ChangeState<T, TConfig>(TConfig config) where T: IGameState
    {
        if (NextState != null)
            throw new ArgumentException("a next state is already ready.");

        NextState = CreateState<T, TConfig>(config);

        return (T)NextState;
    }

    public T CreateState<T>() where T: IGameState => IoCContainer.Resolve<T>();

    public T CreateState<T, TConfig>(TConfig config) where T : IGameState =>
        IoCContainer.Resolve<T>(new TypedParameter(typeof(TConfig), config))
    ;
}