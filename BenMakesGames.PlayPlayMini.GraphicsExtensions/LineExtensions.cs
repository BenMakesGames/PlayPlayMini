using System;
using System.Runtime.CompilerServices;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions;

public static class LineExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DrawRow(this GraphicsManager graphics, int x1, int x2, int y, Color color)
        => graphics.DrawFilledRectangle(x1, y, x2 - x1 + 1, 1, color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DrawColumn(this GraphicsManager graphics, int x, int y1, int y2, Color color)
        => graphics.DrawFilledRectangle(x, y1, 1, y2 - y1 + 1, color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DrawLine(this GraphicsManager graphics, Vector2 start, Vector2 end, Color color)
        => graphics.DrawLine((int)start.X, (int)start.Y, (int)end.X, (int)end.Y, color);

    // a modification of Bresenham's line algorithm that draws a sequence of line segments, to minimize
    // MonoGame Draw calls:
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
                    graphics.DrawFilledRectangle(oldX, oldY, x1 - dx1 - oldX + 1, y1 - dy1 - oldY + 1, color);
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
                    graphics.DrawFilledRectangle(oldX, oldY, x1 - dx2 - oldX + 1, y1 - dy2 - oldY + 1, color);
                    oldX = x1;
                    oldY = y1;
                }
            }
        }
    }
}
