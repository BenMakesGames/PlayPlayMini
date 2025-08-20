using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

namespace BenMakesGames.PlayPlayMini.Services;

public sealed partial class GraphicsManager
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawFilledRectangle((int x, int y) upperLeft, (int x, int y) bottomRight, Color c)
    {
        SpriteBatch.Draw(WhitePixel, new Rectangle(upperLeft.x, upperLeft.y, bottomRight.x - upperLeft.x + 1, bottomRight.y - upperLeft.y + 1), null, c);
        DrawCalls++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawFilledRectangle(int x, int y, int w, int h, Color c)
    {
        SpriteBatch.Draw(WhitePixel, new Rectangle(x, y, w, h), null, c);
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
        SpriteBatch.Draw(WhitePixel, new Rectangle(x, y, w, 1), null, c);
        SpriteBatch.Draw(WhitePixel, new Rectangle(x, y + h - 1, w, 1), null, c);

        // left & right line
        SpriteBatch.Draw(WhitePixel, new Rectangle(x, y + 1, 1, h - 2), null, c);
        SpriteBatch.Draw(WhitePixel, new Rectangle(x + w - 1, y + 1, 1, h - 2), null, c);

        DrawCalls += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawPoint(int x, int y, Color c)
    {
        SpriteBatch.Draw(WhitePixel, new Rectangle(x, y, 1, 1), c);
        DrawCalls++;
    }
}
