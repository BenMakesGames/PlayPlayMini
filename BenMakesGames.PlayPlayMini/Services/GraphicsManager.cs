using BenMakesGames.PlayPlayMini.Attributes.DI;
using BenMakesGames.PlayPlayMini.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Frozen;
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
    public int Zoom { get; private set; } = 2;
    public bool FullScreen { get; private set; }

    /// <summary>
    /// When <c>true</c>, the graphics device synchronizes its <c>Present</c> calls with the display's
    /// vertical retrace, capping the frame rate at the display's refresh rate and eliminating tearing.
    /// When <c>false</c>, the graphics device presents as fast as it can, which — combined with
    /// <see cref="Game.IsFixedTimeStep"/> set to <c>false</c> — leaves the game loop uncapped and can
    /// pin a CPU core at 100%.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c> to preserve historical behavior. Use <see cref="SetVSync(bool)"/> to
    /// change it at runtime; direct assignment is not supported because the underlying graphics device
    /// requires an explicit apply step for the change to take effect.
    /// </remarks>
    public bool VSync { get; private set; }

    public int Width { get; private set; } = 1920 / 3;
    public int Height { get; private set; } = 1080 / 3;

    public int DrawCalls { get; private set; }

    public GraphicsDevice GraphicsDevice => Game.GraphicsDevice;
    private ContentManager Content => Game.Content;
    internal RenderTarget2D RenderTarget { get; private set; } = null!;

    private Game Game = null!;
    private GraphicsDeviceManager Graphics = null!;
    public SpriteBatch SpriteBatch { get; set; } = null!;

    public IReadOnlyDictionary<string, Texture2D> Pictures { get; private set; } = new Dictionary<string, Texture2D>();
    public Texture2D WhitePixel { get; private set; } = null!;
    public IReadOnlyDictionary<string, SpriteSheet> SpriteSheets { get; private set; } = new Dictionary<string, SpriteSheet>();
    public IReadOnlyDictionary<string, Font> Fonts { get; private set; } = new Dictionary<string, Font>();
    public IReadOnlyDictionary<string, Effect> PixelShaders { get; private set; } = new Dictionary<string, Effect>();

    internal IBatchScope? CurrentBatchScope;
    internal SceneShaderScope? CurrentLayerScope;

    private readonly Dictionary<(int Width, int Height), Stack<RenderTarget2D>> _layerRenderTargetPools = new();
    private readonly Stack<ShaderScope> _shaderScopePool = new();
    private readonly Stack<SceneShaderScope> _sceneShaderScopePool = new();

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
        Graphics.SynchronizeWithVerticalRetrace = VSync;

        ApplyWindowMode(Width * Zoom, Height * Zoom, coverDisplay: FullScreen);

        SpriteBatch = new SpriteBatch(GraphicsDevice);

        GraphicsDevice.BlendState = BlendState.AlphaBlend;

        // PreserveContents so mid-draw scopes that rebind (e.g. WithSceneShader with bounds) don't
        // wipe what's already on the framebuffer when they set the render target back to it.
        RenderTarget = new RenderTarget2D(
            GraphicsDevice, Width, Height,
            mipMap: false,
            preferredFormat: SurfaceFormat.Color,
            preferredDepthFormat: DepthFormat.None,
            preferredMultiSampleCount: 0,
            usage: RenderTargetUsage.PreserveContents
        );
    }

    /// <inheritdoc />
    public void LoadContent(GameStateManager gsm)
    {
        WhitePixel = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
        WhitePixel.SetData([ Color.White ]);

        var pictures = gsm.Assets.GetAll<PictureMeta>().ToDictionary(meta => meta.Key, _ => (Texture2D)null!);
        var spriteSheets = gsm.Assets.GetAll<SpriteSheetMeta>().ToDictionary(meta => meta.Key, _ => (SpriteSheet)null!);
        var fonts = gsm.Assets.GetAll<FontMeta>().ToDictionary(meta => meta.Key, _ => (Font)null!);
        var pixelShaders = gsm.Assets.GetAll<PixelShaderMeta>().ToDictionary(meta => meta.Key, _ => (Effect)null!);

        // load immediately
        foreach(var meta in gsm.Assets.GetAll<PictureMeta>().Where(m => m.PreLoaded))
            LoadPicture(pictures, meta);

        Pictures = pictures.ToFrozenDictionary();

        foreach(var meta in gsm.Assets.GetAll<SpriteSheetMeta>().Where(m => m.PreLoaded))
            LoadSpriteSheet(spriteSheets, meta);

        SpriteSheets = spriteSheets.ToFrozenDictionary();

        foreach(var meta in gsm.Assets.GetAll<FontMeta>().Where(m => m.PreLoaded))
            LoadFont(fonts, meta);

        Fonts = fonts.ToFrozenDictionary();

        foreach(var meta in gsm.Assets.GetAll<PixelShaderMeta>().Where(m => m.PreLoaded))
            LoadPixelShader(pixelShaders, meta);

        PixelShaders = pixelShaders.ToFrozenDictionary();

        // deferred
        Task.Run(() =>
        {
            foreach(var meta in gsm.Assets.GetAll<PictureMeta>().Where(m => !m.PreLoaded))
                LoadPicture(pictures, meta);

            Pictures = pictures.ToFrozenDictionary();

            foreach(var meta in gsm.Assets.GetAll<SpriteSheetMeta>().Where(m => !m.PreLoaded))
                LoadSpriteSheet(spriteSheets, meta);

            SpriteSheets = spriteSheets.ToFrozenDictionary();

            foreach(var meta in gsm.Assets.GetAll<FontMeta>().Where(m => !m.PreLoaded))
                LoadFont(fonts, meta);

            Fonts = fonts.ToFrozenDictionary();

            foreach(var meta in gsm.Assets.GetAll<PixelShaderMeta>().Where(m => !m.PreLoaded))
                LoadPixelShader(pixelShaders, meta);

            PixelShaders = pixelShaders.ToFrozenDictionary();

            FullyLoaded = true;
        });
    }

    private void LoadFont(Dictionary<string, Font> fonts, FontMeta font)
    {
        List<FontSheet> fontSheets = [];

        foreach(var fontSheetMeta in font.FontSheets)
        {
            try
            {
                fontSheets.Add(new FontSheet(
                    Content.Load<Texture2D>(fontSheetMeta.Path),
                    fontSheetMeta.Width,
                    fontSheetMeta.Height,
                    fontSheetMeta.HorizontalSpacing,
                    fontSheetMeta.VerticalSpacing,
                    fontSheetMeta.FirstCharacter
                ));
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to load Font (Texture2D) {Path}: {Message}", fontSheetMeta.Path, e.Message);
            }
        }

        if(fontSheets.Count > 0)
            fonts[font.Key] = new Font(fontSheets);
    }

    private void LoadPicture(Dictionary<string, Texture2D> pictures, PictureMeta picture)
    {
        try
        {
            pictures[picture.Key] = Content.Load<Texture2D>(picture.Path);
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to load Picture (Texture2D) {Path}: {Message}", picture.Path, e.Message);
        }
    }

    private void LoadSpriteSheet(Dictionary<string, SpriteSheet> spriteSheets, SpriteSheetMeta spriteSheet)
    {
        try
        {
            spriteSheets[spriteSheet.Key] = new SpriteSheet(Content.Load<Texture2D>(spriteSheet.Path), spriteSheet.Width, spriteSheet.Height);
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to load SpriteSheet (Texture2D) {Path}: {Message}", spriteSheet.Path, e.Message);
        }
    }

    private void LoadPixelShader(Dictionary<string, Effect> pixelShaders, PixelShaderMeta pixelShader)
    {
        try
        {
            pixelShaders[pixelShader.Key] = Content.Load<Effect>(pixelShader.Path);
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

        foreach (var stack in _layerRenderTargetPools.Values)
            while (stack.TryPop(out var rt))
                rt.Dispose();
        _layerRenderTargetPools.Clear();
    }

    public void SetTransformMatrix(Matrix? matrix)
        => TransformMatrix = matrix;

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

        Graphics.SynchronizeWithVerticalRetrace = VSync;

        ApplyWindowMode(Zoom * Width, Zoom * Height, coverDisplay: FullScreen);

        return true;
    }

    /// <summary>
    /// Sets whether the graphics device synchronizes its <c>Present</c> calls with the display's
    /// vertical retrace. See <see cref="VSync"/> for the semantics; this is the only supported way
    /// to change that setting because the underlying graphics device requires an explicit apply
    /// step for the change to take effect.
    /// </summary>
    /// <remarks>
    /// No-op when the requested value equals the current <see cref="VSync"/>.
    /// </remarks>
    public void SetVSync(bool vsync)
    {
        if (VSync == vsync) return;

        VSync = vsync;
        Graphics.SynchronizeWithVerticalRetrace = vsync;
        Graphics.ApplyChanges();
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

        Graphics.SynchronizeWithVerticalRetrace = VSync;

        ApplyWindowMode(desiredWidth, desiredHeight, coverDisplay: FullScreen);
    }

    private void ApplyWindowMode(int desiredWidth, int desiredHeight, bool coverDisplay)
    {
        Game.Window.IsBorderless = coverDisplay;

        if (coverDisplay)
        {
            Game.Window.Position = Point.Zero;
        }
        else
        {
            var display = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            Game.Window.Position = new Point(
                Math.Max(0, (display.Width - desiredWidth) / 2),
                Math.Max(0, (display.Height - desiredHeight) / 2)
            );
        }

        Graphics.PreferredBackBufferWidth = desiredWidth;
        Graphics.PreferredBackBufferHeight = desiredHeight;
        Graphics.IsFullScreen = false;
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
        SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp);
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

    /// <summary>
    /// Applies the given pixel shader to each individual draw call inside the using block.
    /// The shader samples each draw's own source texture (a sprite, a font glyph, the white
    /// pixel for primitives), so it's the right tool for per-sprite effects (color tints,
    /// dithering, palette swaps, distortion of a single sprite's own pixels).
    /// </summary>
    /// <remarks>
    /// <para>
    /// For effects that need to sample <em>neighboring scene content</em> (ripples, blurs,
    /// chromatic aberration, refraction), use <see cref="WithSceneShader(Effect?, Action{Effect}?)"/>
    /// instead — that runs the shader at composite time over an assembled layer.
    /// </para>
    /// <para>
    /// The returned <see cref="IDisposable"/> is pooled and must be disposed exactly once
    /// (use a <c>using</c> statement); do not store it beyond its scope or dispose it twice.
    /// </para>
    /// </remarks>
    public IDisposable WithShader(Effect? pixelShader, Action<Effect>? configure = null)
    {
        var scope = AcquireShaderScope();
        scope.Initialize(pixelShader, configure);
        return scope;
    }

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
    /// <remarks>
    /// The returned <see cref="IDisposable"/> is pooled and must be disposed exactly once
    /// (use a <c>using</c> statement); do not store it beyond its scope or dispose it twice.
    /// </remarks>
    public IDisposable WithShader(string pixelShaderName, Action<Effect>? configure = null)
    {
        var scope = AcquireShaderScope();
        scope.Initialize(PixelShaders[pixelShaderName], configure);
        return scope;
    }

    /// <summary>
    /// Renders the wrapped graphics calls into a layer-sized render target, then composites
    /// the layer to the previous target through the given pixel shader. Use this for effects
    /// that need to sample the assembled scene (ripples, blurs, refraction, post-process
    /// distortion). The shader's source texture is the layer image — neighboring pixels
    /// sample correctly across draw-call boundaries.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The layer render target is the same size as the framebuffer (<see cref="Width"/> ×
    /// <see cref="Height"/>) and is acquired from a pool, so repeated use is cheap.
    /// </para>
    /// <para>
    /// Pass <c>null</c> to group draws into an isolated layer with no shader — useful for
    /// blend isolation. Nesting is supported: an inner <c>WithSceneShader</c> composites
    /// into the outer layer, which composites to the previous target.
    /// </para>
    /// <para>
    /// Use <see cref="WithShader(Effect?, Action{Effect}?)"/> for per-sprite effects; that
    /// shader sees each draw's own source texture, not the composited scene.
    /// </para>
    /// <para>
    /// The returned <see cref="IDisposable"/> is pooled and must be disposed exactly once
    /// (use a <c>using</c> statement); do not store it beyond its scope or dispose it twice.
    /// </para>
    /// </remarks>
    public IDisposable WithSceneShader(Effect? pixelShader, Action<Effect>? configure = null)
    {
        var scope = AcquireSceneShaderScope();
        scope.Initialize(pixelShader, bounds: null, configure);
        return scope;
    }

    /// <summary>
    /// Looks up a shader by name and applies it as a scene shader. See
    /// <see cref="WithSceneShader(Effect?, Action{Effect}?)"/>.
    /// </summary>
    /// <remarks>
    /// The returned <see cref="IDisposable"/> is pooled and must be disposed exactly once
    /// (use a <c>using</c> statement); do not store it beyond its scope or dispose it twice.
    /// </remarks>
    public IDisposable WithSceneShader(string pixelShaderName, Action<Effect>? configure = null)
    {
        var scope = AcquireSceneShaderScope();
        scope.Initialize(PixelShaders[pixelShaderName], bounds: null, configure);
        return scope;
    }

    /// <summary>
    /// Bounded variant of <see cref="WithSceneShader(Effect?, Action{Effect}?)"/>. Renders the
    /// wrapped graphics calls into a render target sized to <paramref name="bounds"/>, then
    /// composites that region back to the previous target through the given pixel shader at
    /// <paramref name="bounds"/>'s position.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Draws inside the scope use their usual world-space coordinates; the scope applies a
    /// translation transform so those coordinates land at the render target's local origin.
    /// The composite is a single sprite draw whose UVs span the full 0..1 range, so a shader
    /// that samples <c>input.TextureCoordinates</c> sees the bounded region as its whole
    /// canvas (useful for effects like plasma that should "fit" a shape rather than tile).
    /// </para>
    /// <para>
    /// Render targets are pooled per (width, height), so repeated use at the same size is
    /// cheap. A small RT for each bounded region is far cheaper than the framebuffer-sized RT
    /// used by the unbounded overload.
    /// </para>
    /// <para>
    /// <paramref name="bounds"/> is interpreted in the current render target's coordinate
    /// space (framebuffer coords, in the common case where <see cref="TransformMatrix"/> is
    /// <c>null</c>).
    /// </para>
    /// <para>
    /// The returned <see cref="IDisposable"/> is pooled and must be disposed exactly once
    /// (use a <c>using</c> statement); do not store it beyond its scope or dispose it twice.
    /// </para>
    /// </remarks>
    public IDisposable WithSceneShader(Effect? pixelShader, Rectangle bounds, Action<Effect>? configure = null)
    {
        var scope = AcquireSceneShaderScope();
        scope.Initialize(pixelShader, bounds, configure);
        return scope;
    }

    /// <summary>
    /// Looks up a shader by name and applies it as a bounded scene shader. See
    /// <see cref="WithSceneShader(Effect?, Rectangle, Action{Effect}?)"/>.
    /// </summary>
    /// <remarks>
    /// The returned <see cref="IDisposable"/> is pooled and must be disposed exactly once
    /// (use a <c>using</c> statement); do not store it beyond its scope or dispose it twice.
    /// </remarks>
    public IDisposable WithSceneShader(string pixelShaderName, Rectangle bounds, Action<Effect>? configure = null)
    {
        var scope = AcquireSceneShaderScope();
        scope.Initialize(PixelShaders[pixelShaderName], bounds, configure);
        return scope;
    }

    internal RenderTarget2D AcquireLayerRenderTarget()
        => AcquireLayerRenderTarget(Width, Height);

    internal RenderTarget2D AcquireLayerRenderTarget(int width, int height)
    {
        if (_layerRenderTargetPools.TryGetValue((width, height), out var stack) && stack.TryPop(out var rt))
            return rt;

        return new RenderTarget2D(GraphicsDevice, width, height);
    }

    internal void ReleaseLayerRenderTarget(RenderTarget2D rt)
    {
        var key = (rt.Width, rt.Height);
        if (!_layerRenderTargetPools.TryGetValue(key, out var stack))
        {
            stack = new Stack<RenderTarget2D>();
            _layerRenderTargetPools[key] = stack;
        }
        stack.Push(rt);
    }

    internal ShaderScope AcquireShaderScope()
        => _shaderScopePool.TryPop(out var s) ? s : new ShaderScope(this);

    internal void ReleaseShaderScope(ShaderScope scope)
        => _shaderScopePool.Push(scope);

    internal SceneShaderScope AcquireSceneShaderScope()
        => _sceneShaderScopePool.TryPop(out var s) ? s : new SceneShaderScope(this);

    internal void ReleaseSceneShaderScope(SceneShaderScope scope)
        => _sceneShaderScopePool.Push(scope);
}

internal interface IBatchScope : IDisposable
{
    /// <summary>
    /// Re-opens this scope's SpriteBatch. Called by a child scope's Dispose to restore the
    /// parent batch after the child's nested batch has ended.
    /// </summary>
    void BeginBatch();
}

internal sealed class ShaderScope : IBatchScope
{
    private readonly GraphicsManager Graphics;
    private Effect? Shader;
    private Action<Effect>? ShaderConfigureAction;
    private IBatchScope? PreviousScope;

    public ShaderScope(GraphicsManager graphics)
    {
        Graphics = graphics;
    }

    public void Initialize(Effect? shader, Action<Effect>? configure)
    {
        Shader = shader;
        ShaderConfigureAction = configure;
        PreviousScope = Graphics.CurrentBatchScope;

        if (PreviousScope is not null)
            Graphics.SpriteBatch.End();

        BeginBatch();
    }

    public void BeginBatch()
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

        Graphics.CurrentBatchScope = this;
    }

    public void Dispose()
    {
        Graphics.SpriteBatch.End();

        if (PreviousScope is not null)
            PreviousScope.BeginBatch();
        else
            Graphics.CurrentBatchScope = null;

        Shader = null;
        ShaderConfigureAction = null;
        PreviousScope = null;

        Graphics.ReleaseShaderScope(this);
    }
}

internal sealed class SceneShaderScope : IBatchScope
{
    private readonly GraphicsManager Graphics;
    private Effect? Shader;
    private Action<Effect>? ShaderConfigureAction;
    private IBatchScope? PreviousScope;
    private SceneShaderScope? PreviousLayerScope;
    private RenderTarget2D? PreviousRenderTarget;
    private Matrix? PreviousTransformMatrix;
    private Rectangle? Bounds;

    internal RenderTarget2D LayerRenderTarget { get; private set; } = null!;

    public SceneShaderScope(GraphicsManager graphics)
    {
        Graphics = graphics;
    }

    public void Initialize(Effect? shader, Rectangle? bounds, Action<Effect>? configure)
    {
        Shader = shader;
        ShaderConfigureAction = configure;
        Bounds = bounds;
        PreviousScope = Graphics.CurrentBatchScope;
        PreviousLayerScope = Graphics.CurrentLayerScope;
        PreviousRenderTarget = PreviousLayerScope?.LayerRenderTarget ?? Graphics.RenderTarget;
        PreviousTransformMatrix = Graphics.TransformMatrix;

        if (PreviousScope is not null)
            Graphics.SpriteBatch.End();

        var rtWidth = bounds?.Width ?? Graphics.Width;
        var rtHeight = bounds?.Height ?? Graphics.Height;
        LayerRenderTarget = Graphics.AcquireLayerRenderTarget(rtWidth, rtHeight);
        Graphics.GraphicsDevice.SetRenderTarget(LayerRenderTarget);
        Graphics.GraphicsDevice.Clear(Color.Transparent);

        if (bounds is { } b)
        {
            var translation = Matrix.CreateTranslation(-b.X, -b.Y, 0);
            Graphics.SetTransformMatrix(PreviousTransformMatrix is { } prev ? translation * prev : translation);
        }

        Graphics.CurrentLayerScope = this;

        BeginBatch();
    }

    public void BeginBatch()
    {
        // Inside the layer the inner batch uses no shader; the layer's effect runs at
        // composite time in Dispose.
        Graphics.SpriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone,
            effect: null,
            transformMatrix: Graphics.TransformMatrix
        );

        Graphics.CurrentBatchScope = this;
    }

    public void Dispose()
    {
        Graphics.SpriteBatch.End();

        Graphics.GraphicsDevice.SetRenderTarget(PreviousRenderTarget);
        Graphics.CurrentLayerScope = PreviousLayerScope;
        Graphics.SetTransformMatrix(PreviousTransformMatrix);

        if (Shader is not null && ShaderConfigureAction is not null)
            ShaderConfigureAction.Invoke(Shader);

        Graphics.SpriteBatch.Begin(
            SpriteSortMode.Immediate,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            effect: Shader
        );
        Graphics.SpriteBatch.Draw(
            LayerRenderTarget,
            Bounds is { } b ? new Vector2(b.X, b.Y) : Vector2.Zero,
            Color.White
        );
        Graphics.SpriteBatch.End();

        Graphics.ReleaseLayerRenderTarget(LayerRenderTarget);

        if (PreviousScope is not null)
            PreviousScope.BeginBatch();
        else
            Graphics.CurrentBatchScope = null;

        Shader = null;
        ShaderConfigureAction = null;
        PreviousScope = null;
        PreviousLayerScope = null;
        PreviousRenderTarget = null;
        PreviousTransformMatrix = null;
        Bounds = null;
        LayerRenderTarget = null!;

        Graphics.ReleaseSceneShaderScope(this);
    }
}
