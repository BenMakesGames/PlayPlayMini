using BenMakesGames.PlayPlayMini.Attributes.DI;
using BenMakesGames.PlayPlayMini.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BenMakesGames.PlayPlayMini.Extensions;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using Color = System.Drawing.Color;

namespace BenMakesGames.PlayPlayMini.Services;

[AutoRegister]
public sealed class GraphicsManager: IServiceLoadContent, IServiceInitialize
{
    private ILogger<GraphicsManager> Logger { get; }

    public bool FullyLoaded { get; private set; }

    // TODO: restore this:
    //public Matrix? TransformMatrix { get; private set; }
    
    public int Zoom { get; private set; } = 2;
    public bool FullScreen { get; private set; }
    public int Width { get; private set; } = 1920 / 3;
    public int Height { get; private set; } = 1080 / 3;

    public int DrawCalls { get; private set; }

    private GL? GL { get; set; }
    
    // TODO: restore this
    //private RenderTarget2D RenderTarget { get; set; } = null!;

    public Dictionary<string, Texture2D> Pictures { get; } = new();
    public Dictionary<string, SpriteSheet> SpriteSheets { get; } = new();
    public Dictionary<string, Font> Fonts { get; } = new();

    private SpriteBatch SpriteBatch { get; } = new();

    public GraphicsManager(ILogger<GraphicsManager> logger)
    {
        Logger = logger;
    }

    public void Initialize(PlayPlayMiniApp playplayMini)
    {
        Width = 640;
        Height = 360;
        Zoom = 2;
        
        GL = GL.GetApi(playplayMini.SilkWindow);
        GL.Viewport(0, 0, (uint)Width, (uint)Height);
    }

    public void LoadContent(AssetCollection assetCollection)
    {
        Pictures.Clear();
        SpriteSheets.Clear();
        Fonts.Clear();

        // load immediately
        foreach(var meta in assetCollection.GetAll<PictureMeta>().Where(m => m.PreLoaded))
            LoadPicture(meta);

        if(!Pictures.ContainsKey("Pixel"))
        {
            var whitePixel = new Texture2D(GL!, 1, 1, new[] { Color.White });

            Pictures.Add("Pixel", whitePixel);
        }

        foreach(var meta in assetCollection.GetAll<SpriteSheetMeta>().Where(m => m.PreLoaded))
            LoadSpriteSheet(meta);

        foreach(var meta in assetCollection.GetAll<FontMeta>().Where(m => m.PreLoaded))
            LoadFont(meta);

        // deferred
        Task.Run(() => LoadDeferredContent(assetCollection));
    }

    private void LoadDeferredContent(AssetCollection assets)
    {
        foreach(var meta in assets.GetAll<PictureMeta>().Where(m => !m.PreLoaded))
            LoadPicture(meta);

        foreach(var meta in assets.GetAll<SpriteSheetMeta>().Where(m => !m.PreLoaded))
            LoadSpriteSheet(meta);

        foreach(var meta in assets.GetAll<FontMeta>().Where(m => !m.PreLoaded))
            LoadFont(meta);

        FullyLoaded = true;
    }

    private void LoadFont(FontMeta font)
    {
        try
        {
            Fonts.Add(font.Key, new Font()
            {
                Texture = Texture2D.FromFile(GL!, font.Path),
                CharacterWidth = font.Width,
                CharacterHeight = font.Height,
                HorizontalSpacing = font.HorizontalSpacing,
                VerticalSpacing = font.VerticalSpacing,
                FirstCharacter = font.FirstCharacter
            });
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to load {Path}: {Message}", font.Path, e.Message);
        }
    }

    private void LoadPicture(PictureMeta picture)
    {
        try
        {
            Pictures.Add(picture.Key, Texture2D.FromFile(GL!, picture.Path));
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to load {Path}: {Message}", picture.Path, e.Message);
        }
    }

    private void LoadSpriteSheet(SpriteSheetMeta spriteSheet)
    {
        try
        {
            SpriteSheets.Add(spriteSheet.Key, new SpriteSheet()
            {
                Texture = Texture2D.FromFile(GL!, spriteSheet.Path),
                SpriteWidth = spriteSheet.Width,
                SpriteHeight = spriteSheet.Height
            });
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to load {Path}: {Message}", spriteSheet.Path, e.Message);
        }
    }

    public void UnloadContent()
    {
        // TODO: anything??
        //SpriteBatch.Dispose();
    }

    // TODO: restore this
    /*
    public void SetTransformMatrix(Matrix? matrix)
    {
        TransformMatrix = matrix;
    }
    */

    // TODO: restore these:
    /*
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
    */

    internal void BeginDraw()
    {
        DrawCalls = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear(Color c)
    {
        GL!.ClearColor(c);
        GL.Clear(ClearBufferMask.ColorBufferBit);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Rectangle SpriteRectangle(SpriteSheet spriteSheet, int spriteIndex) => new(
        (spriteIndex % spriteSheet.Columns) * spriteSheet.SpriteWidth,
        (spriteIndex / spriteSheet.Columns) * spriteSheet.SpriteHeight,
        spriteSheet.SpriteWidth,
        spriteSheet.SpriteHeight
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Rectangle FontRectangle(Font font, int character) => new(
        (character % font.Columns) * font.CharacterWidth,
        (character / font.Columns) * font.CharacterHeight,
        font.CharacterWidth,
        font.CharacterHeight
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawFilledRectangle(int x, int y, int w, int h, Color c)
        => DrawFilledRectangle(new Rectangle(x, y, w, h), c);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawFilledRectangle(Rectangle rectangle, Color c)
    {
        SpriteBatch.Draw(Pictures["Pixel"], rectangle, null, c);
        DrawCalls++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawFilledRectangle(int x, int y, int w, int h, Color fill, Color outline)
    {
        DrawFilledRectangle(x + 1, y + 1, w - 2, h - 2, fill);
        DrawRectangle(x, y, w, h, outline);
    }

    /// <summary>
    /// Draw a series of points in the given color.
    /// </summary>
    /// <param name="points"></param>
    /// <param name="c"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawPoints(IEnumerable<(int x, int y)> points, Color c)
    {
        foreach (var p in points)
            DrawPoint(p.x, p.y, c);
    }

    /// <summary>
    /// Draws the outline of a rectangle.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="w"></param>
    /// <param name="h"></param>
    /// <param name="c"></param>
    public void DrawRectangle(int x, int y, int w, int h, Color c)
    {
        // top & bottom line
        SpriteBatch.Draw(Pictures["Pixel"], new Rectangle(x, y, w, 1), null, c);
        SpriteBatch.Draw(Pictures["Pixel"], new Rectangle(x, y + h - 1, w, 1), null, c);

        // left & right line
        SpriteBatch.Draw(Pictures["Pixel"], new Rectangle(x, y + 1, 1, h - 2), null, c);
        SpriteBatch.Draw(Pictures["Pixel"], new Rectangle(x + w - 1, y + 1, 1, h - 2), null, c);

        DrawCalls += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawPoint(int x, int y, Color c)
    {
        SpriteBatch.Draw(Pictures["Pixel"], new Rectangle(x, y, 1, 1), null, c);
        DrawCalls++;
    }

    /// <summary>
    /// Draws a picture rotated and scaled. The picture will be centered on the given (x, y) coordinates, and rotated
    /// around that point.
    /// </summary>
    /// <param name="pictureName"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="angle"></param>
    /// <param name="scale"></param>
    /// <param name="color"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawPictureRotatedAndScaled(string pictureName, int x, int y, float angle, float scale, Color color)
        => DrawTextureRotatedAndScaled(Pictures[pictureName], x, y, angle, scale, color);

    /// <summary>
    /// Draws a texture rotated and scaled. The full texture will be centered on the given (x, y) coordinates, and
    /// rotated around that point.
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="angle"></param>
    /// <param name="scale"></param>
    /// <param name="color"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawTextureRotatedAndScaled(Texture2D texture, int x, int y, float angle, float scale, Color color)
    {
        SpriteBatch.Draw(
            texture,
            new Rectangle(x, y, (int)(texture.Width * scale), (int)(texture.Height * scale)),
            new()
            {
                Tint = color,
                Rotation = angle,
            }
        );

        DrawCalls++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawPictureWithTransformations(string pictureName, int x, int y, Rectangle? clippingRectangle, RendererFlip flip, float angle, float scale, Color c)
        => DrawTextureWithTransformations(
            Pictures[pictureName],
            x, y,
            clippingRectangle,
            flip,
            angle,
            scale,
            scale,
            c
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawTextureWithTransformations(Texture2D texture, int x, int y, Rectangle? clippingRectangle, RendererFlip flip, float angle, float scaleX, float scaleY, Color c)
    {
        var rectangle = clippingRectangle ?? new Rectangle(0, 0, texture.Width, texture.Height);

        SpriteBatch.Draw(
            texture,
            new Rectangle(x, y, (int)(rectangle.Width * scaleX), (int)(rectangle.Height * scaleY)),
            new()
            {
                ClippingRectangle = clippingRectangle,
                Tint = c,
                Rotation = angle,
                Flip = flip
            }
        );

        DrawCalls++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSpriteWithTransformations(string spriteSheetName, int x, int y, int spriteIndex, RendererFlip flip, float angle, float scale, Color tint)
        => DrawTextureWithTransformations(
            SpriteSheets[spriteSheetName].Texture,
            x, y,
            SpriteRectangle(SpriteSheets[spriteSheetName], spriteIndex),
            flip,
            angle,
            scale,
            scale,
            tint
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSpriteWithTransformations(SpriteSheet spriteSheet, int x, int y, int spriteIndex, RendererFlip flip, float angle, float scale, Color tint)
        => DrawTextureWithTransformations(
            spriteSheet.Texture,
            x, y,
            SpriteRectangle(spriteSheet, spriteIndex),
            flip,
            angle,
            scale,
            scale,
            tint
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawPictureRotatedAndScaled(string pictureName, int x, int y, Rectangle? clippingRectangle, float angle, float scale, Color c)
        => DrawTextureRotatedAndScaled(Pictures[pictureName], x, y, clippingRectangle, angle, scale, c);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawTextureRotatedAndScaled(Texture2D texture, int x, int y, Rectangle? clippingRectangle, float angle, float scale, Color c)
        => DrawTextureWithTransformations(
            texture,
            x,
            y,
            clippingRectangle,
            RendererFlip.None,
            angle,
            scale,
            scale,
            c
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSpriteRotatedAndScaled(string spriteSheet, int x, int y, int spriteIndex, float angle, float scale, Color c) =>
        DrawTextureWithTransformations(
            SpriteSheets[spriteSheet].Texture,
            x,
            y,
            SpriteRectangle(SpriteSheets[spriteSheet], spriteIndex),
            RendererFlip.None,
            angle,
            scale,
            scale,
            c
        )
    ;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSpriteRotatedAndScaled(SpriteSheet spriteSheet, int x, int y, int spriteIndex, float angle, float scale, Color c) =>
        DrawTextureWithTransformations(
            spriteSheet.Texture,
            x,
            y,
            SpriteRectangle(spriteSheet, spriteIndex),
            RendererFlip.None,
            angle,
            scale,
            scale,
            c
        )
    ;

    /// <summary>
    /// Use a font to draw text. Newline characters are respected. For automatic wrapping, use DrawTextWithWordWrap.
    /// </summary>
    /// <param name="fontName"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="text"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int x, int y) DrawText(string fontName, int x, int y, string text, Color color)
        => DrawText(Fonts[fontName], x, y, text, color);

    /// <summary>
    /// Use a font to draw text. Newline characters are respected. For automatic wrapping, use DrawTextWithWordWrap.
    /// </summary>
    /// <param name="font"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="text"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public (int x, int y) DrawText(Font font, int x, int y, string text, Color color)
    {
        var position = (x, y);

        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] is '\r' or '\n')
                position = (x, position.y + font.CharacterHeight + font.VerticalSpacing);
            else
                position = DrawText(font, position.x, position.y, text[i], color);
        }

        return position;
    }

    /// <summary>
    /// Use a font to draw a single character.
    /// </summary>
    /// <param name="fontName"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="character"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int x, int y) DrawText(string fontName, int x, int y, char character, Color color)
        => DrawText(Fonts[fontName], x, y, character, color);

    /// <summary>
    /// Use a font to draw a single character.
    /// </summary>
    /// <param name="font"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="character"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public (int x, int y) DrawText(Font font, int x, int y, char character, Color color)
    {
        var position = (x, y);

        if (character is '\r' or '\n')
        {
            position.x = x;
            position.y += font.CharacterHeight + font.VerticalSpacing;
        }
        else if (character >= font.FirstCharacter)
        {
            DrawTexture(font.Texture, position.x, position.y, FontRectangle(font, character - font.FirstCharacter), color);

            position.x += font.CharacterWidth + font.HorizontalSpacing;
        }

        return position;
    }

    /// <summary>
    /// Compute the total width and height needed to draw text within a given width, using a given font.
    /// </summary>
    /// <param name="fontName"></param>
    /// <param name="maxWidth"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int Width, int Height) ComputeDimensionsWithWordWrap(string fontName, int maxWidth, string text)
        => ComputeDimensionsWithWordWrap(Fonts[fontName], maxWidth, text);

    /// <summary>
    /// Compute the total width and height needed to draw text within a given width, using a given font.
    /// </summary>
    /// <param name="font"></param>
    /// <param name="maxWidth"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public (int Width, int Height) ComputeDimensionsWithWordWrap(Font font, int maxWidth, string text)
    {
        var lines = text.Split(new[] { '\r', '\n' });

        var pixelY = 0;
        var maxPixelX = 0;

        for(var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            if (lineIndex > 0)
                pixelY += font.CharacterHeight + font.VerticalSpacing;

            var words = lines[lineIndex].Split(' ');
            var pixelX = 0;

            for (var i = 0; i < words.Length; i++)
            {
                if (i > 0)
                    pixelX += font.CharacterWidth + font.HorizontalSpacing;

                var word = words[i];

                if (pixelX + word.Length * font.CharacterWidth + (word.Length - 1) * font.HorizontalSpacing > maxWidth)
                {
                    pixelX = 0;
                    pixelY += font.CharacterHeight + font.VerticalSpacing;
                }

                if (pixelX > maxPixelX)
                    maxPixelX = pixelX;

                (pixelX, pixelY) = PretendDrawText(font, pixelX, pixelY, word);

                if (pixelX > maxPixelX)
                    maxPixelX = pixelX;
            }
        }

        return (maxPixelX, pixelY + font.CharacterHeight);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int, int) DrawTextWithWordWrap(string fontName, int x, int y, int maxWidth, string text, Color color)
        => DrawTextWithWordWrap(Fonts[fontName], x, y, maxWidth, text, color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int, int) DrawTextWithWordWrap(Font font, int x, int y, int maxWidth, string text, Color color)
        => DrawText(font, x, y, text.WrapText(font, maxWidth), color);

    public (int, int) PretendDrawText(Font font, int x, int y, string text)
    {
        var currentX = x;
        var currentY = y;

        foreach (var c in text)
        {
            if (c >= 32)
                currentX += font.CharacterWidth + font.HorizontalSpacing;
            else if (c == 10 || c == 13)
            {
                currentX = x;
                currentY += font.CharacterHeight + font.VerticalSpacing;
            }
        }

        return (currentX, currentY);
    }

    [Obsolete("Use the PlayPlayMini.GraphicsExtensions version of this method, instead, and ditch your outline font. (If you're truly counting frames, call DrawText twice, passing the appropriate fonts and colors... or inspect this method, and copy-paste its source into your code!)")]
    public void DrawTextWithOutline(Font font, Font outlineFont, int x, int y, string text, Color fill, Color outline)
    {
        var currentX = x;
        var currentY = y;

        foreach (char c in text)
        {
            if (c >= font.FirstCharacter)
            {
                DrawTexture(outlineFont.Texture, currentX - 1, currentY - 1, FontRectangle(outlineFont, c - font.FirstCharacter), outline);

                currentX += font.CharacterWidth + font.HorizontalSpacing;
            }
            else if (c == 10 || c == 13)
            {
                currentX = x;
                currentY += font.CharacterHeight + font.VerticalSpacing;
            }
        }

        currentX = x;
        currentY = y;

        DrawText(font, currentX, currentY, text, fill);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSprite(string spriteSheetName, int x, int y, int spriteIndex)
        => DrawSprite(SpriteSheets[spriteSheetName], x, y, spriteIndex, Color.White);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSprite(string spriteSheetName, (int x, int y) position, int spriteIndex)
        => DrawSprite(SpriteSheets[spriteSheetName], position.x, position.y, spriteIndex, Color.White);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSprite(SpriteSheet spriteSheet, int x, int y, int spriteIndex)
        => DrawSprite(spriteSheet, x, y, spriteIndex, Color.White);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSprite(SpriteSheet spriteSheet, (int x, int y) position, int spriteIndex)
        => DrawSprite(spriteSheet, position.x, position.y, spriteIndex, Color.White);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSprite(string spriteSheetName, int x, int y, int spriteIndex, Color tint)
        => DrawSprite(SpriteSheets[spriteSheetName], x, y, spriteIndex, tint);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSprite(string spriteSheetName, (int x, int y) position, int spriteIndex, Color tint)
        => DrawSprite(SpriteSheets[spriteSheetName], position.x, position.y, spriteIndex, tint);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSprite(SpriteSheet spriteSheet, (int x, int y) position, int spriteIndex, Color tint)
        => DrawSprite(spriteSheet, position.x, position.y, spriteIndex, tint);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSprite(SpriteSheet spriteSheet, int x, int y, int spriteIndex, Color tint) =>
        DrawTexture(
            spriteSheet.Texture,
            x, y,
            SpriteRectangle(spriteSheet, spriteIndex),
            tint
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawPicture(string pictureName, int x, int y)
        => DrawTexture(Pictures[pictureName], x, y, Color.White);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawPicture(string pictureName, int x, int y, Color tint)
        => DrawTexture(Pictures[pictureName], x, y, tint);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawTexture(Texture2D texture, int x, int y)
        => DrawTexture(texture, x, y, null, Color.White);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawTexture(Texture2D texture, int x, int y, Color color)
        => DrawTexture(texture, x, y, null, color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawTexture(Texture2D texture, int x, int y, Rectangle? clippingRectangle)
        => DrawTexture(texture, x, y, clippingRectangle, Color.White);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawTexture(Texture2D texture, int x, int y, Rectangle? clippingRectangle, Color color)
    {
        SpriteBatch.Draw(texture, new Rectangle(x, y, texture.Width, texture.Height), clippingRectangle, color);
        DrawCalls++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawPictureStretched(string pictureName, int x, int y, int width, int height, Rectangle? clippingRectangle = null)
        => DrawTextureStretched(Pictures[pictureName], x, y, width, height, clippingRectangle, Color.White);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawPictureStretched(string pictureName, int x, int y, int width, int height, Rectangle? clippingRectangle, Color c) =>
        DrawTextureStretched(Pictures[pictureName], x, y, width, height, clippingRectangle, c);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawTextureStretched(Texture2D texture, int x, int y, int width, int height, Rectangle? clippingRectangle = null) =>
        DrawTextureStretched(texture, x, y, width, height, clippingRectangle, Color.White);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawTextureStretched(Texture2D texture, int x, int y, int width, int height, Rectangle? clippingRectangle, Color c)
    {
        SpriteBatch.Draw(texture, new Rectangle(x, y, width, height), clippingRectangle, c);
        DrawCalls++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSpriteStretched(string spriteSheetName, int x, int y, int width, int height, int spriteIndex) =>
        DrawTextureStretched(
            SpriteSheets[spriteSheetName].Texture,
            x, y,
            width, height,
            SpriteRectangle(SpriteSheets[spriteSheetName], spriteIndex),
            Color.White
        )
    ;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSpriteStretched(SpriteSheet spriteSheet, int x, int y, int width, int height, int spriteIndex) =>
        DrawTextureStretched(
            spriteSheet.Texture,
            x, y,
            width, height,
            SpriteRectangle(spriteSheet, spriteIndex),
            Color.White
        )
    ;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawTextureFlipped(Texture2D texture, int x, int y, RendererFlip flip) =>
        DrawTextureFlipped(texture, x, y, new Rectangle(0, 0, texture.Width, texture.Height), flip, Color.White);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawTextureFlipped(Texture2D texture, int x, int y, RendererFlip flip, Color tint) =>
        DrawTextureFlipped(texture, x, y, new Rectangle(0, 0, texture.Width, texture.Height), flip, tint);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawTextureFlipped(Texture2D texture, int x, int y, Rectangle clippingRectangle, RendererFlip flip) =>
        DrawTextureFlipped(texture, x, y, clippingRectangle, flip, Color.White);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawTextureFlipped(Texture2D texture, int x, int y, Rectangle clippingRectangle, RendererFlip flip, Color tint)
    {
        SpriteBatch.Draw(texture, clippingRectangle with { X = x, Y = y }, new()
        {
            ClippingRectangle = clippingRectangle,
            Tint = tint,
            Flip = flip
        });
        
        DrawCalls++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSpriteFlipped(string spriteSheetName, int x, int y, int spriteIndex, RendererFlip flip) =>
        DrawTextureFlipped(
            SpriteSheets[spriteSheetName].Texture,
            x,
            y,
            SpriteRectangle(SpriteSheets[spriteSheetName], spriteIndex),
            flip,
            Color.White
        )
    ;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSpriteFlipped(SpriteSheet spriteSheet, int x, int y, int spriteIndex, RendererFlip flip) =>
        DrawTextureFlipped(
            spriteSheet.Texture,
            x,
            y,
            SpriteRectangle(spriteSheet, spriteIndex),
            flip,
            Color.White
        )
    ;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSpriteFlipped(SpriteSheet spriteSheet, int x, int y, int spriteIndex, RendererFlip flip, Color tint) =>
        DrawTextureFlipped(
            spriteSheet.Texture,
            x,
            y,
            SpriteRectangle(spriteSheet, spriteIndex),
            flip,
            tint
        )
    ;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawSpriteFlipped(string spriteName, int x, int y, int spriteIndex, RendererFlip flip, Color tint) =>
        DrawTextureFlipped(
            SpriteSheets[spriteName].Texture,
            x,
            y,
            SpriteRectangle(SpriteSheets[spriteName], spriteIndex),
            flip,
            tint
        );

    internal void EndDraw()
    {
        // TODO: draw sprites to screen
        

        // TODO: apply zoom
        /*
        GraphicsDevice.SetRenderTarget(null);

        SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp);
        SpriteBatch.Draw(RenderTarget, new Rectangle(0, 0, Width * Zoom, Height * Zoom), Color.White);
        SpriteBatch.End();
        */
    }
}
