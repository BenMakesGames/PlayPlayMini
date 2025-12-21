using BenMakesGames.PlayPlayMini.Attributes.DI;
using BenMakesGames.PlayPlayMini.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BenMakesGames.PlayPlayMini.Services;

/// <summary>
/// Service for drawing sprites, pictures, fonts, and primitives to the screen.
/// </summary>
[AutoRegister]
public sealed partial class GraphicsManager: IServiceLoadContent, IServiceInitialize
{
    private ILogger<GraphicsManager> Logger { get; }

    /// <inheritdoc />
    public bool FullyLoaded { get; private set; }

    public Matrix? TransformMatrix { get; private set; }
    public Effect? PostProcessingShader { get; private set; }
    public int Zoom { get; private set; } = 2;
    public bool FullScreen { get; private set; }
    public int Width { get; private set; } = 1920 / 3;
    public int Height { get; private set; } = 1080 / 3;

    public int DrawCalls { get; private set; }

    public GraphicsDevice GraphicsDevice => Game.GraphicsDevice;
    private ContentManager Content => Game.Content;
    private RenderTarget2D RenderTarget { get; set; } = null!;

    private Game Game = null!;
    private GraphicsDeviceManager Graphics = null!;
    public SpriteBatch SpriteBatch { get; set; } = null!;

    public Dictionary<string, Texture2D> Pictures { get; private set; } = new();
    public Texture2D WhitePixel { get; private set; } = null!;
    public Dictionary<string, SpriteSheet> SpriteSheets { get; private set; } = new();
    public Dictionary<string, Font> Fonts { get; private set; } = new();
    public Dictionary<string, Effect> PixelShaders { get; private set; } = new();

    internal ShaderScope? CurrentShaderScope;

    public GraphicsManager(ILogger<GraphicsManager> logger)
    {
        Logger = logger;
    }

    internal void SetGame(Game game)
    {
        if (Game is not null)
            throw new ArgumentException("SetGame can only be called once!");

        Game = game;
        Graphics = new GraphicsDeviceManager(Game);
    }

    /// <inheritdoc />
    public void Initialize(GameStateManager gsm)
    {
        var windowSize = gsm.Config.InitialWindowSize;

        Width = windowSize.Width;
        Height = windowSize.Height;
        Zoom = windowSize.Zoom;

        Graphics.HardwareModeSwitch = false;
        Graphics.SynchronizeWithVerticalRetrace = false;
        Graphics.PreferredBackBufferWidth = Width * Zoom;
        Graphics.PreferredBackBufferHeight = Height * Zoom;
        Graphics.IsFullScreen = FullScreen;
        Graphics.ApplyChanges();

        SpriteBatch = new SpriteBatch(GraphicsDevice);

        GraphicsDevice.BlendState = BlendState.AlphaBlend;

        RenderTarget = new RenderTarget2D(GraphicsDevice, Width, Height);
    }

    /// <inheritdoc />
    public void LoadContent(GameStateManager gsm)
    {
        WhitePixel = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
        WhitePixel.SetData([ Color.White ]);

        Pictures = gsm.Assets.GetAll<PictureMeta>().ToDictionary(meta => meta.Key, _ => (Texture2D)null!);
        SpriteSheets = gsm.Assets.GetAll<SpriteSheetMeta>().ToDictionary(meta => meta.Key, _ => (SpriteSheet)null!);
        Fonts = gsm.Assets.GetAll<FontMeta>().ToDictionary(meta => meta.Key, _ => (Font)null!);
        PixelShaders = gsm.Assets.GetAll<PixelShaderMeta>().ToDictionary(meta => meta.Key, _ => (Effect)null!);

        // load immediately
        foreach(var meta in gsm.Assets.GetAll<PictureMeta>().Where(m => m.PreLoaded))
            LoadPicture(meta);

        foreach(var meta in gsm.Assets.GetAll<SpriteSheetMeta>().Where(m => m.PreLoaded))
            LoadSpriteSheet(meta);

        foreach(var meta in gsm.Assets.GetAll<FontMeta>().Where(m => m.PreLoaded))
            LoadFont(meta);

        foreach(var meta in gsm.Assets.GetAll<PixelShaderMeta>().Where(m => m.PreLoaded))
            LoadPixelShader(meta);

        // deferred
        Task.Run(() => LoadDeferredContent(gsm.Assets));
    }

    private void LoadDeferredContent(AssetCollection assets)
    {
        foreach(var meta in assets.GetAll<PictureMeta>().Where(m => !m.PreLoaded))
            LoadPicture(meta);

        foreach(var meta in assets.GetAll<SpriteSheetMeta>().Where(m => !m.PreLoaded))
            LoadSpriteSheet(meta);

        foreach(var meta in assets.GetAll<FontMeta>().Where(m => !m.PreLoaded))
            LoadFont(meta);

        foreach(var meta in assets.GetAll<PixelShaderMeta>().Where(m => !m.PreLoaded))
            LoadPixelShader(meta);

        FullyLoaded = true;
    }

    private void LoadFont(FontMeta font)
    {
        try
        {
            Fonts[font.Key] = new Font(
                Content.Load<Texture2D>(font.Path),
                font.Width,
                font.Height,
                font.HorizontalSpacing,
                font.VerticalSpacing,
                font.FirstCharacter
            );
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to load Font (Texture2D) {Path}: {Message}", font.Path, e.Message);
        }
    }

    private void LoadPicture(PictureMeta picture)
    {
        try
        {
            Pictures[picture.Key] = Content.Load<Texture2D>(picture.Path);
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to load Picture (Texture2D) {Path}: {Message}", picture.Path, e.Message);
        }
    }

    private void LoadSpriteSheet(SpriteSheetMeta spriteSheet)
    {
        try
        {
            SpriteSheets[spriteSheet.Key] = new SpriteSheet(Content.Load<Texture2D>(spriteSheet.Path), spriteSheet.Width, spriteSheet.Height);
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to load SpriteSheet (Texture2D) {Path}: {Message}", spriteSheet.Path, e.Message);
        }
    }

    private void LoadPixelShader(PixelShaderMeta pixelShader)
    {
        try
        {
            PixelShaders[pixelShader.Key] = Content.Load<Effect>(pixelShader.Path);
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to load PixelShader (Effect) {Path}: {Message}", pixelShader.Path, e.Message);
        }
    }

    /// <inheritdoc />
    public void UnloadContent()
    {
        SpriteBatch.Dispose();
    }

    public void SetTransformMatrix(Matrix? matrix)
        => TransformMatrix = matrix;

    public void SetPostProcessingPixelShader(Effect? effect)
        => PostProcessingShader = effect;

    public void SetPostProcessingPixelShader(string pixelShaderName)
        => SetPostProcessingPixelShader(PixelShaders[pixelShaderName]);

    /// <summary>
    /// "Zoom" controls how large each "pixel" is.
    /// Zoom 1 => each pixel of your sprites, pictures, fonts, etc, takes up 1 pixel on the screen
    /// Zoom 2 => each pixel of your sprites, pictures, fonts, etc, takes up a 2x2 pixel square on the screen
    /// Zoom 3 => each pixel of your sprites, pictures, fonts, etc, takes up a 3x3 pixel square on the screen
    /// etc
    ///
    /// If Zoom * Width and Zoom * Height precisely fit the physical screen, the game will automatically go into full screen.
    ///
    /// If a Zoom value less than 1 is given, then a Zoom value of 1 is used.
    ///
    /// If Zoom * Width or Zoom * Height is larger than the available screen space, Zoom is unchanged, and SetZoom returns false.
    /// </summary>
    /// <param name="zoom"></param>
    /// <returns>True if zoom * Width and zoom * Height will fit within the available screen space; false, otherwise.</returns>
    public bool SetZoom(int zoom)
    {
        if (zoom > MaxZoom())
            return false;

        Zoom = zoom < 1 ? 1 : zoom;
        FullScreen = Zoom * Width == Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width && Zoom * Height == Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height;

        Graphics.SynchronizeWithVerticalRetrace = false;
        Graphics.PreferredBackBufferWidth = Zoom * Width;
        Graphics.PreferredBackBufferHeight = Zoom * Height;
        Graphics.IsFullScreen = FullScreen;
        Graphics.ApplyChanges();

        return true;
    }

    public void SetFullscreen(bool fullscreen)
    {
        FullScreen = fullscreen;

        int desiredWidth, desiredHeight;

        if (FullScreen)
        {
            desiredWidth = Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width;
            desiredHeight = Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height;

            Zoom = Math.Min(desiredWidth / Width, desiredHeight / Height);
        }
        else
        {
            desiredWidth = Zoom * Width;
            desiredHeight = Zoom * Height;
        }

        Graphics.SynchronizeWithVerticalRetrace = false;
        Graphics.PreferredBackBufferWidth = desiredWidth;
        Graphics.PreferredBackBufferHeight = desiredHeight;
        Graphics.IsFullScreen = FullScreen;
        Graphics.ApplyChanges();
    }

    /// <summary>
    /// Returns the maximum Zoom value that will fit within the available screen space.
    /// </summary>
    /// <returns></returns>
    public int MaxZoom()
    {
        return Math.Min(
            Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width / Width,
            Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height / Height
        );
    }

    internal void BeginDraw()
    {
        DrawCalls = 0;
        Graphics.GraphicsDevice.SetRenderTarget(RenderTarget);
        Graphics.GraphicsDevice.Clear(Color.Transparent);
    }

    internal void EndDraw()
    {
        Graphics.GraphicsDevice.SetRenderTarget(null);
        SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, effect: PostProcessingShader);
        SpriteBatch.Draw(RenderTarget, new Rectangle(0, 0, Width * Zoom, Height * Zoom), Color.White);
        SpriteBatch.End();
    }

    /// <summary>
    /// Clears the screen to the given color.
    /// </summary>
    /// <param name="c"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear(Color c) => GraphicsDevice.Clear(c);

    /// <summary>
    /// Clears the screen to black.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => GraphicsDevice.Clear(Color.Black);

    public IDisposable WithShader(Effect? pixelShader, Action<Effect>? configure = null)
        => new ShaderScope(this, pixelShader, configure);

    /// <summary>
    /// Applies the given pixel shader to the wrapped graphics calls.
    /// </summary>
    /// <example>
    /// Call for using a shader without parameters:
    /// <code>
    /// using(Graphics.WithShader("MyShader"))
    /// {
    ///     // draw sprites, rectangles, etc.
    /// }
    /// </code>
    /// Call for using a shader WITH parameters:
    /// <code>
    /// using(Graphics.WithShader("MyShader", e => e.Parameters["SomeShaderParameter"].SetValue(12345)))
    /// {
    ///     // draw sprites, rectangles, etc.
    /// }
    /// </code>
    /// </example>
    /// <param name="pixelShaderName">Name of the shader to use.</param>
    /// <param name="configure">Optional configuration delegate.</param>
    /// <returns></returns>
    public IDisposable WithShader(string pixelShaderName, Action<Effect>? configure = null)
        => new ShaderScope(this, PixelShaders[pixelShaderName], configure);
}

internal sealed class ShaderScope : IDisposable
{
    private readonly GraphicsManager Graphics;
    private readonly Effect? Shader;
    private readonly Action<Effect>? ShaderConfigureAction;
    private readonly ShaderScope? PreviousScope;

    public ShaderScope(GraphicsManager graphics, Effect? shader, Action<Effect>? configure)
    {
        Graphics = graphics;
        Shader = shader;
        ShaderConfigureAction = configure;
        PreviousScope = graphics.CurrentShaderScope;

        if (graphics.CurrentShaderScope is not null)
            Graphics.SpriteBatch.End();

        Begin();
    }

    private void Begin()
    {
        if(Shader is not null && ShaderConfigureAction is not null)
            ShaderConfigureAction.Invoke(Shader);

        Graphics.SpriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone,
            effect: Shader,
            transformMatrix: Graphics.TransformMatrix
        );

        Graphics.CurrentShaderScope = this;
    }

    public void Dispose()
    {
        Graphics.SpriteBatch.End();

        if (PreviousScope is not null)
            PreviousScope.Begin();
        else
            Graphics.CurrentShaderScope = null;
    }
}