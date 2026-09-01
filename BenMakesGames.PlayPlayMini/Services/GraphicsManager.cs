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

    /// <summary>
    /// An ordered list of post-process shaders applied to the whole scene every frame, wrapped
    /// around the current game state's draw and every <see cref="IServiceDraw"/>'s draw by
    /// <see cref="GameStateManager"/>. Populate it once (for example, in a service's
    /// <see cref="IServiceLoadContent.LoadContent"/>, after the shaders have loaded) and every
    /// frame — including frames drawn by game states added later — runs through it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Entries apply in list order: index 0's shader runs first, on the raw scene; each later
    /// entry's shader runs on the previous entry's output; the last entry's output is what feeds
    /// the final upscale blit. A chain of <c>[Bloom, CRT, Vignette]</c> therefore applies bloom,
    /// then CRT over that, then vignette last.
    /// </para>
    /// <para>
    /// Shaders here run at the game's native resolution (<see cref="Width"/> ×
    /// <see cref="Height"/>) — one texel per game pixel, before the point-clamp upscale to
    /// <see cref="Zoom"/>. Reach for the chain for effects that reason in game-pixel space
    /// (color grading, palette treatment, colorblind simulation, sample-the-scene distortion).
    /// For effects that need to see monitor-pixel space after the upscale (scanlines, phosphor
    /// grid, CRT curvature that ignores the pixel-art grid), use
    /// <see cref="BackbufferPostProcessChain"/> instead.
    /// </para>
    /// <para>
    /// An empty chain costs nothing: <see cref="GameStateManager"/> short-circuits it, so there is
    /// no extra render-target hop and no extra allocation per frame.
    /// </para>
    /// <para>
    /// Chain iteration and the underlying <see cref="WithSceneShader(string, Action{Effect}?)"/>
    /// scopes are allocation-free. The one thing that isn't is a
    /// <see cref="PostProcessEntry.Configure"/> lambda written inline at registration time that
    /// captures variables — cache such delegates in a field and hand the same instance to the
    /// entry, so nothing is allocated on the hot path.
    /// </para>
    /// <para>
    /// Names are resolved against <see cref="PixelShaders"/> at draw time, so entries may be added
    /// before their shaders load; a name that is still unknown when the frame draws throws. Mutate
    /// the chain from input/update code or at startup — mutating it during <c>Draw</c> is not
    /// supported.
    /// </para>
    /// </remarks>
    public IList<PostProcessEntry> PostProcessChain { get; } = new List<PostProcessEntry>();

    /// <summary>
    /// An ordered list of post-process shaders applied at physical-pixel resolution, after the
    /// point-clamp upscale to <see cref="Zoom"/>. Populate it once (for example, in a service's
    /// <see cref="IServiceLoadContent.LoadContent"/>, after the shaders have loaded) and every
    /// frame runs through it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Shaders here see the scene at <c>Width × Zoom</c> by <c>Height × Zoom</c> — one texel per
    /// monitor pixel. Reach for the chain for effects that must align to the physical grid:
    /// scanlines, phosphor persistence, CRT curvature, chromatic aberration at monitor scale, a
    /// pixel-sharp vignette. For effects that reason in game-pixel space (color grading, sample-
    /// the-scene distortion of the pixel art itself), use <see cref="PostProcessChain"/> instead —
    /// its shaders run before the upscale, at the native game resolution.
    /// </para>
    /// <para>
    /// Entries apply in list order: index 0's shader runs first on an upscaled copy of the scene,
    /// each later entry's shader runs on the previous entry's output, and the last entry's output
    /// is what reaches the actual backbuffer. A chain of <c>[Curvature, Scanlines, Vignette]</c>
    /// therefore curves first, then adds scanlines over that, then vignettes last.
    /// </para>
    /// <para>
    /// An empty chain costs nothing: <see cref="EndDraw"/> short-circuits it, so there is no extra
    /// render-target hop and no extra allocation per frame. A non-empty chain always adds one
    /// initial unshaded upscale blit (so every entry sees monitor-pixel input, without exception),
    /// plus one shaded blit per entry.
    /// </para>
    /// <para>
    /// Iteration is allocation-free and reuses at most two pooled render targets (ping-ponged) at
    /// the current scaled dimensions, so the steady-state cost is just the shader work itself.
    /// A <see cref="PostProcessEntry.Configure"/> lambda that captures variables is the one thing
    /// that can allocate — cache such delegates in a field, as with <see cref="PostProcessChain"/>.
    /// </para>
    /// <para>
    /// Names are resolved against <see cref="PixelShaders"/> at draw time, so entries may be added
    /// before their shaders load; a name that is still unknown when the frame draws throws. Mutate
    /// the chain from input/update code or at startup — mutating it during <c>Draw</c> is not
    /// supported.
    /// </para>
    /// </remarks>
    public IList<PostProcessEntry> BackbufferPostProcessChain { get; } = new List<PostProcessEntry>();

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

    private RenderTarget2D? _pendingCaptureTarget;

    /// <summary>
    /// Arms a one-shot capture of the next completed frame's fully-composited image (post-
    /// <see cref="BackbufferPostProcessChain"/>, at <see cref="Width"/> × <see cref="Zoom"/> by
    /// <see cref="Height"/> × <see cref="Zoom"/>) into <paramref name="target"/>. The next
    /// <see cref="GameStateManager"/> <c>Draw</c> writes the composited pixels into <paramref name="target"/>
    /// in addition to the normal backbuffer blit, then clears the arm — one call captures one frame.
    /// </summary>
    /// <param name="target">
    /// A caller-owned <see cref="RenderTarget2D"/> whose <see cref="Texture2D.Width"/>,
    /// <see cref="Texture2D.Height"/>, and <see cref="Texture2D.Format"/> match
    /// <see cref="Width"/> × <see cref="Zoom"/>, <see cref="Height"/> × <see cref="Zoom"/>, and
    /// <see cref="SurfaceFormat.Color"/> respectively. Mismatches throw immediately, not on the next
    /// frame. PPM never disposes, resizes, or reallocates the target — the caller owns it. Allocate
    /// with <see cref="RenderTargetUsage.PreserveContents"/> if the pixels must survive other
    /// render-target rebinds between the capture and the read.
    /// </param>
    /// <remarks>
    /// <para>
    /// Empty and populated <see cref="BackbufferPostProcessChain"/> both honor the arm: on an empty
    /// chain the target receives the point-upscaled native <c>RenderTarget</c>; on a populated chain
    /// it receives the last chain entry's shaded output — always the same pixels that reach the
    /// backbuffer that frame. Anything later added to <see cref="BackbufferPostProcessChain"/>
    /// automatically appears in captures.
    /// </para>
    /// <para>
    /// Same-frame read is not supported. The compositing happens in the framework's end-of-frame
    /// step, which runs after every <see cref="IServiceDraw"/>'s <c>Draw</c> — so an
    /// <see cref="IServiceDraw"/> that both arms and reads in one <c>Draw</c> sees stale (or zero)
    /// pixels. Read the target from the next frame's <c>Draw</c> or later, once the arming frame
    /// has presented.
    /// </para>
    /// <para>
    /// Arming twice in one frame silently replaces the pending target (last-writer-wins). Frames
    /// with no arm cost nothing new: <see cref="EndDraw"/> checks the field once and short-circuits.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="target"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="target"/>'s dimensions or surface format do not match the required
    /// <see cref="Width"/> × <see cref="Zoom"/>, <see cref="Height"/> × <see cref="Zoom"/>,
    /// <see cref="SurfaceFormat.Color"/>.
    /// </exception>
    public void RequestFrameCapture(RenderTarget2D target)
    {
        ArgumentNullException.ThrowIfNull(target);

        var expectedWidth = Width * Zoom;
        var expectedHeight = Height * Zoom;

        if (target.Width != expectedWidth || target.Height != expectedHeight)
            throw new ArgumentException(
                $"Capture target must be {expectedWidth}x{expectedHeight}, got {target.Width}x{target.Height}.",
                nameof(target)
            );

        if (target.Format != SurfaceFormat.Color)
            throw new ArgumentException(
                $"Capture target must be SurfaceFormat.Color, got {target.Format}.",
                nameof(target)
            );

        _pendingCaptureTarget = target;
    }

    internal void BeginDraw()
    {
        DrawCalls = 0;
        Graphics.GraphicsDevice.SetRenderTarget(RenderTarget);
        Graphics.GraphicsDevice.Clear(Color.Transparent);
    }

    internal void EndDraw()
    {
        var chain = BackbufferPostProcessChain;
        var scaledWidth = Width * Zoom;
        var scaledHeight = Height * Zoom;
        var scaledRect = new Rectangle(0, 0, scaledWidth, scaledHeight);

        if (chain.Count == 0)
        {
            // Fast path: byte-identical to pre-BackbufferPostProcessChain behavior. Skip the
            // intermediate RT hop entirely so a game that never touches the chain pays nothing.
            Graphics.GraphicsDevice.SetRenderTarget(null);
            SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp);
            SpriteBatch.Draw(RenderTarget, scaledRect, Color.White);
            SpriteBatch.End();

            if (_pendingCaptureTarget is not null)
            {
                // Repeat the same upscale blit into the caller's target. The two blits produce
                // identical pixels because the input (RenderTarget) and the destination-region
                // rectangle are the same on both.
                Graphics.GraphicsDevice.SetRenderTarget(_pendingCaptureTarget);
                SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp);
                SpriteBatch.Draw(RenderTarget, scaledRect, Color.White);
                SpriteBatch.End();
                _pendingCaptureTarget = null;
            }

            return;
        }

        // Chain path: upscale the native RenderTarget to a scaled intermediate first (unshaded,
        // PointClamp), so every entry's shader sees monitor-pixel-resolution input. Then run each
        // entry's shader, ping-ponging between two scaled intermediates. The last entry draws
        // straight to the actual backbuffer, so a chain of N entries costs one unshaded upscale
        // blit plus N shaded blits, and never more than two pooled intermediate RTs regardless of
        // chain length.
        var intermediateA = AcquireLayerRenderTarget(scaledWidth, scaledHeight);
        RenderTarget2D? intermediateB = null;

        Graphics.GraphicsDevice.SetRenderTarget(intermediateA);
        SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp);
        SpriteBatch.Draw(RenderTarget, scaledRect, Color.White);
        SpriteBatch.End();

        Texture2D source = intermediateA;

        // for loop, instead of foreach, reduces allocations
        for (var i = 0; i < chain.Count; i++)
        {
            var entry = chain[i];
            var shader = PixelShaders[entry.ShaderName];
            var isLast = i == chain.Count - 1;

            RenderTarget2D? destination;
            if (isLast)
            {
                // When a capture is armed, land the last shaded blit on the caller's target
                // (instead of the backbuffer) and then do one plain PointClamp blit from there to
                // the backbuffer. That way the capture and the on-screen result are the same
                // shader's output, not two runs of it, and the whole detour costs one extra blit.
                destination = _pendingCaptureTarget;
            }
            else if (ReferenceEquals(source, intermediateA))
            {
                intermediateB ??= AcquireLayerRenderTarget(scaledWidth, scaledHeight);
                destination = intermediateB;
            }
            else
            {
                destination = intermediateA;
            }

            Graphics.GraphicsDevice.SetRenderTarget(destination);

            if (entry.Configure is not null)
                entry.Configure(shader);

            SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, effect: shader);
            SpriteBatch.Draw(source, scaledRect, Color.White);
            SpriteBatch.End();

            if (!isLast)
                source = destination!;
        }

        if (_pendingCaptureTarget is not null)
        {
            // The last-entry blit landed on _pendingCaptureTarget instead of the backbuffer;
            // finish the job with an unshaded PointClamp copy to the actual backbuffer.
            Graphics.GraphicsDevice.SetRenderTarget(null);
            SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp);
            SpriteBatch.Draw(_pendingCaptureTarget, scaledRect, Color.White);
            SpriteBatch.End();
            _pendingCaptureTarget = null;
        }

        ReleaseLayerRenderTarget(intermediateA);
        if (intermediateB is not null)
            ReleaseLayerRenderTarget(intermediateB);
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

        // PreserveContents so a nested WithSceneShader that rebinds to its own smaller layer RT
        // doesn't wipe this layer on the rebind back. Symptom without this: opening a bounded
        // WithSceneShader inside a full-scene WithSceneShader loses every draw between the
        // outer-open and the bounded-open — the pool RT gets DiscardContents by default, and
        // the driver drops its content on the next SetRenderTarget. Same reason RenderTarget
        // (the framebuffer) is PreserveContents; layer RTs need the same guarantee for nesting
        // patterns (PostProcessChain around a state that uses bounded scene shaders — plasma
        // highlights, per-tile shaders — is the common case).
        return new RenderTarget2D(
            GraphicsDevice, width, height,
            mipMap: false,
            preferredFormat: SurfaceFormat.Color,
            preferredDepthFormat: DepthFormat.None,
            preferredMultiSampleCount: 0,
            usage: RenderTargetUsage.PreserveContents
        );
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
