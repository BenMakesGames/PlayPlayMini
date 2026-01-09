using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

namespace BenMakesGames.PlayPlayMini.Services;

public sealed partial class GraphicsManager
{
    /// <summary>
    /// Draws a texture rotated and scaled. The full texture will be centered on the given (x, y) coordinates, and
    /// rotated around that point.
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="centerX"></param>
    /// <param name="centerY"></param>
    /// <param name="angle"></param>
    /// <param name="scale"></param>
    /// <param name="color"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawTextureRotatedAndScaled(Texture2D texture, int centerX, int centerY, float angle, float scale, Color color)
    {
        SpriteBatch.Draw(
            texture,
            new Rectangle(centerX, centerY, (int)(texture.Width * scale), (int)(texture.Height * scale)),
            null,
            color,
            angle,
            // ReSharper disable PossibleLossOfFraction
            new Vector2(texture.Width / 2, texture.Height / 2),
            SpriteEffects.None,
            0
        );

        DrawCalls++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawTextureWithTransformations(Texture2D texture, int centerX, int centerY, Rectangle? clippingRectangle, SpriteEffects flip, float angle, float scaleX, float scaleY, Color c)
    {
        var rectangle = clippingRectangle ?? new Rectangle(0, 0, texture.Width, texture.Height);

        SpriteBatch.Draw(
            texture,
            new Rectangle(centerX, centerY, (int)(rectangle.Width * scaleX), (int)(rectangle.Height * scaleY)),
            clippingRectangle,
            c,
            angle,
            new Vector2(rectangle.Width / 2, rectangle.Height / 2),
            flip,
            0
        );

        DrawCalls++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawTextureRotatedAndScaled(Texture2D texture, int centerX, int centerY, Rectangle? clippingRectangle, float angle, float scale, Color c)
        => DrawTextureWithTransformations(
            texture,
            centerX,
            centerY,
            clippingRectangle,
            SpriteEffects.None,
            angle,
            scale,
            scale,
            c
        );

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
        SpriteBatch.Draw(texture, new Vector2(x, y), clippingRectangle, color);
        DrawCalls++;
    }

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
    public void DrawTextureFlipped(Texture2D texture, int x, int y, SpriteEffects flip) =>
        DrawTextureFlipped(texture, x, y, new Rectangle(0, 0, texture.Width, texture.Height), flip, Color.White);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawTextureFlipped(Texture2D texture, int x, int y, SpriteEffects flip, Color tint) =>
        DrawTextureFlipped(texture, x, y, new Rectangle(0, 0, texture.Width, texture.Height), flip, tint);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawTextureFlipped(Texture2D texture, int x, int y, Rectangle clippingRectangle, SpriteEffects flip) =>
        DrawTextureFlipped(texture, x, y, clippingRectangle, flip, Color.White);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawTextureFlipped(Texture2D texture, int x, int y, Rectangle clippingRectangle, SpriteEffects flip, Color tint)
    {
        SpriteBatch.Draw(texture, new Rectangle(x, y, clippingRectangle.Width, clippingRectangle.Height), clippingRectangle, tint, 0, Vector2.Zero, flip, 0);
        DrawCalls++;
    }
}
