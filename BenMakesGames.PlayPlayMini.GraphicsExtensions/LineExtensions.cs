using System;
using System.Runtime.CompilerServices;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions;

/// <summary>
/// Extension methods for drawing lines using the GraphicsManager.
/// </summary>
public static class LineExtensions
{
    /// <summary>
    /// Draws a horizontal line from x1 to x2 at height y.
    /// </summary>
    /// <param name="graphics">The graphics manager instance.</param>
    /// <param name="x1">Starting x coordinate.</param>
    /// <param name="x2">Ending x coordinate.</param>
    /// <param name="y">Y coordinate of the line.</param>
    /// <param name="color">Color of the line.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DrawRow(this GraphicsManager graphics, int x1, int x2, int y, Color color)
        => graphics.DrawFilledRectangle(x1, y, x2 - x1 + 1, 1, color);

    /// <summary>
    /// Draws a vertical line from y1 to y2 at position x.
    /// </summary>
    /// <param name="graphics">The graphics manager instance.</param>
    /// <param name="x">X coordinate of the line.</param>
    /// <param name="y1">Starting y coordinate.</param>
    /// <param name="y2">Ending y coordinate.</param>
    /// <param name="color">Color of the line.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DrawColumn(this GraphicsManager graphics, int x, int y1, int y2, Color color)
        => graphics.DrawFilledRectangle(x, y1, 1, y2 - y1 + 1, color);

    /// <summary>
    /// Draws a line between two Vector2 points.
    /// </summary>
    /// <param name="graphics">The graphics manager instance.</param>
    /// <param name="start">Starting point of the line.</param>
    /// <param name="end">Ending point of the line.</param>
    /// <param name="color">Color of the line.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DrawLine(this GraphicsManager graphics, Vector2 start, Vector2 end, Color color)
        => graphics.DrawLine((int)start.X, (int)start.Y, (int)end.X, (int)end.Y, color);

    /// <summary>
    /// Draws a line between two points using a modified Bresenham's line algorithm.
    /// </summary>
    /// <remarks>
    /// This implementation minimizes MonoGame Draw calls by drawing line segments.
    /// </remarks>
    /// <param name="graphics">The graphics manager instance.</param>
    /// <param name="x1">Starting x coordinate.</param>
    /// <param name="y1">Starting y coordinate.</param>
    /// <param name="x2">Ending x coordinate.</param>
    /// <param name="y2">Ending y coordinate.</param>
    /// <param name="color">Color of the line.</param>
    public static void DrawLine(this GraphicsManager graphics, int x1, int y1, int x2, int y2, Color color)
    {
        var w = x2 - x1;

        if (w == 0)
        {
            graphics.DrawColumn(x1, y1, y2, color);
            return;
        }

        var h = y2 - y1;

        if (h == 0)
        {
            graphics.DrawRow(x1, x2, y1, color);
            return;
        }

        var dy2 = 0;
        var dx1 = w < 0 ? -1 : 1;
        var dy1 = h < 0 ? -1 : 1;
        var dx2 = w < 0 ? -1 : 1;
        var longest = Math.Abs(w);
        var shortest = Math.Abs(h);
        if (!(longest > shortest))
        {
            (shortest, longest) = (longest, shortest);
            dy2 = h < 0 ? -1 : 1;
            dx2 = 0;
        }

        var numerator = longest >> 1;
        var oldX = x1;
        var oldY = y1;

        for (var i = 0; i <= longest; i++)
        {
            numerator += shortest;
            if (!(numerator < longest))
            {
                numerator -= longest;
                x1 += dx1;
                y1 += dy1;

                if ((x1 != oldX && y1 != oldY) || i == longest)
                {
                    var rectX = Math.Min(oldX, x1 - dx1);
                    var rectY = Math.Min(oldY, y1 - dy1);
                    var rectW = Math.Abs(x1 - dx1 - oldX) + 1;
                    var rectH = Math.Abs(y1 - dy1 - oldY) + 1;
                    graphics.DrawFilledRectangle(rectX, rectY, rectW, rectH, color);
                    oldX = x1;
                    oldY = y1;
                }
            }
            else
            {
                x1 += dx2;
                y1 += dy2;

                if ((x1 != oldX && y1 != oldY) || i == longest)
                {
                    var rectX = Math.Min(oldX, x1 - dx2);
                    var rectY = Math.Min(oldY, y1 - dy2);
                    var rectW = Math.Abs(x1 - dx2 - oldX) + 1;
                    var rectH = Math.Abs(y1 - dy2 - oldY) + 1;
                    graphics.DrawFilledRectangle(rectX, rectY, rectW, rectH, color);
                    oldX = x1;
                    oldY = y1;
                }
            }
        }
    }
}
