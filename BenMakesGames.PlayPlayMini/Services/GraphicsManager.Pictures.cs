using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

namespace BenMakesGames.PlayPlayMini.Services;

public sealed partial class GraphicsManager
{
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawPictureWithTransformations(string pictureName, int centerX, int centerY, Rectangle? clippingRectangle, SpriteEffects flip, float angle, float scale, Color c)
        => DrawTextureWithTransformations(
            Pictures[pictureName],
            centerX, centerY,
            clippingRectangle,
            flip,
            angle,
            scale,
            scale,
            c
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawPictureRotatedAndScaled(string pictureName, int x, int y, Rectangle? clippingRectangle, float angle, float scale, Color c)
        => DrawTextureRotatedAndScaled(Pictures[pictureName], x, y, clippingRectangle, angle, scale, c);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawPicture(string pictureName, int x, int y)
        => DrawTexture(Pictures[pictureName], x, y, Color.White);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawPicture(string pictureName, int x, int y, Color tint)
        => DrawTexture(Pictures[pictureName], x, y, tint);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawPictureStretched(string pictureName, int x, int y, int width, int height, Rectangle? clippingRectangle = null)
        => DrawTextureStretched(Pictures[pictureName], x, y, width, height, clippingRectangle, Color.White);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawPictureStretched(string pictureName, int x, int y, int width, int height, Rectangle? clippingRectangle, Color c) =>
        DrawTextureStretched(Pictures[pictureName], x, y, width, height, clippingRectangle, c);
}
