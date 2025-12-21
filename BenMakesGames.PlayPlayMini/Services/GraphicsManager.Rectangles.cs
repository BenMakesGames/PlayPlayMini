using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;

namespace BenMakesGames.PlayPlayMini.Services;

public sealed partial class GraphicsManager
{
    /// <summary>
    /// Draws a filled rectangle.
    /// </summary>
    /// <param name="upperLeft"></param>
    /// <param name="bottomRight"></param>
    /// <param name="fillColor"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawFilledRectangle((int x, int y) upperLeft, (int x, int y) bottomRight, Color fillColor)
    {
        SpriteBatch.Draw(WhitePixel, new Rectangle(upperLeft.x, upperLeft.y, bottomRight.x - upperLeft.x + 1, bottomRight.y - upperLeft.y + 1), null, fillColor);
        DrawCalls++;
    }

    /// <summary>
    /// Draws a filled rectangle.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="w"></param>
    /// <param name="h"></param>
    /// <param name="fillColor"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawFilledRectangle(int x, int y, int w, int h, Color fillColor)
    {
        SpriteBatch.Draw(WhitePixel, new Rectangle(x, y, w, h), null, fillColor);
        DrawCalls++;
    }

    /// <summary>
    /// Draws a filled rectangle.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="w"></param>
    /// <param name="h"></param>
    /// <param name="fillColor"></param>
    /// <param name="outlineColor"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawFilledRectangle(int x, int y, int w, int h, Color fillColor, Color outlineColor)
    {
        DrawFilledRectangle(x + 1, y + 1, w - 2, h - 2, fillColor);
        DrawRectangle(x, y, w, h, outlineColor);
    }

    /// <summary>
    /// Draws the outline of a rectangle.
    /// </summary>
    /// <param name="rectangle"></param>
    /// <param name="outlineColor"></param>
    public void DrawRectangle(Rectangle rectangle, Color outlineColor)
        => DrawRectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, outlineColor);

    /// <summary>
    /// Draws a filled rectangle.
    /// </summary>
    /// <param name="rectangle"></param>
    /// <param name="fillColor"></param>
    public void DrawFilledRectangle(Rectangle rectangle, Color fillColor)
    {
        SpriteBatch.Draw(WhitePixel, rectangle, null, fillColor);
        DrawCalls++;
    }

    /// <summary>
    /// Draws a filled rectangle.
    /// </summary>
    /// <param name="rectangle"></param>
    /// <param name="fillColor"></param>
    /// <param name="outlineColor"></param>
    public void DrawFilledRectangle(Rectangle rectangle, Color fillColor, Color outlineColor)
    {
        DrawFilledRectangle(rectangle, fillColor);
        DrawRectangle(rectangle, outlineColor);
    }

    /// <summary>
    /// Draws the outline of a rectangle.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="w"></param>
    /// <param name="h"></param>
    /// <param name="outlineColor"></param>
    public void DrawRectangle(int x, int y, int w, int h, Color outlineColor)
    {
        // top & bottom line
        SpriteBatch.Draw(WhitePixel, new Rectangle(x, y, w, 1), null, outlineColor);
        SpriteBatch.Draw(WhitePixel, new Rectangle(x, y + h - 1, w, 1), null, outlineColor);

        // left & right line
        SpriteBatch.Draw(WhitePixel, new Rectangle(x, y + 1, 1, h - 2), null, outlineColor);
        SpriteBatch.Draw(WhitePixel, new Rectangle(x + w - 1, y + 1, 1, h - 2), null, outlineColor);

        DrawCalls += 4;
    }

    /// <summary>
    /// Draws a single point.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="color"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawPoint(int x, int y, Color color)
    {
        SpriteBatch.Draw(WhitePixel, new Rectangle(x, y, 1, 1), color);
        DrawCalls++;
    }
}