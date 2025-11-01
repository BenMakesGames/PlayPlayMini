using System;
using System.Collections.Generic;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions;

public static class PointExtensions
{
    /// <summary>
    /// Draw a series of points in the given color.
    /// </summary>
    /// <remarks>
    /// Use with a generator may have significant performance impacts.
    /// </remarks>
    /// <param name="graphics"></param>
    /// <param name="points"></param>
    /// <param name="color"></param>
    public static void DrawPoints(this GraphicsManager graphics, IEnumerable<(int x, int y)> points, Color color)
    {
        foreach (var p in points)
            graphics.DrawPoint(p.x, p.y, color);
    }

    /// <summary>
    /// Draw a series of points in the given color.
    /// </summary>
    /// <param name="graphics"></param>
    /// <param name="points"></param>
    /// <param name="color"></param>
    public static void DrawPoints(this GraphicsManager graphics, Span<(int x, int y)> points, Color color)
    {
        foreach (var p in points)
            graphics.DrawPoint(p.x, p.y, color);
    }

    /// <summary>
    /// Draw a series of points in the given color.
    /// </summary>
    /// <param name="graphics"></param>
    /// <param name="points"></param>
    /// <param name="color"></param>
    public static void DrawPoints(this GraphicsManager graphics, ReadOnlySpan<(int x, int y)> points, Color color)
    {
        foreach (var p in points)
            graphics.DrawPoint(p.x, p.y, color);
    }

    /// <summary>
    /// Draw a series of points in the given color.
    /// </summary>
    /// <remarks>
    /// Use with a generator may have significant performance impacts.
    /// </remarks>
    /// <param name="graphics"></param>
    /// <param name="points"></param>
    /// <param name="color"></param>
    public static void DrawPoints(this GraphicsManager graphics, IEnumerable<Point> points, Color color)
    {
        foreach (var p in points)
            graphics.DrawPoint(p.X, p.Y, color);
    }

    /// <summary>
    /// Draw a series of points in the given color.
    /// </summary>
    /// <param name="graphics"></param>
    /// <param name="points"></param>
    /// <param name="color"></param>
    public static void DrawPoints(this GraphicsManager graphics, Span<Point> points, Color color)
    {
        foreach (var p in points)
            graphics.DrawPoint(p.X, p.Y, color);
    }

    /// <summary>
    /// Draw a series of points in the given color.
    /// </summary>
    /// <param name="graphics"></param>
    /// <param name="points"></param>
    /// <param name="color"></param>
    public static void DrawPoints(this GraphicsManager graphics, ReadOnlySpan<Point> points, Color color)
    {
        foreach (var p in points)
            graphics.DrawPoint(p.X, p.Y, color);
    }
}
