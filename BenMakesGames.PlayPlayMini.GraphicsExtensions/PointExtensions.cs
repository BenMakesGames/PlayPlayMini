using System;
using System.Collections.Generic;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions;

public static class PointExtensions
{
    /// <param name="graphics"></param>
    extension(GraphicsManager graphics)
    {
        /// <summary>
        /// Draw a series of points in the given color.
        /// </summary>
        /// <remarks>
        /// Use with a generator may have significant performance impacts.
        /// </remarks>
        /// <param name="points"></param>
        /// <param name="color"></param>
        public void DrawPoints(IEnumerable<(int x, int y)> points, Color color)
        {
            foreach (var p in points)
                graphics.DrawPoint(p.x, p.y, color);
        }

        /// <summary>
        /// Draw a series of points in the given color.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="color"></param>
        public void DrawPoints(Span<(int x, int y)> points, Color color)
        {
            foreach (var p in points)
                graphics.DrawPoint(p.x, p.y, color);
        }

        /// <summary>
        /// Draw a series of points in the given color.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="color"></param>
        public void DrawPoints(ReadOnlySpan<(int x, int y)> points, Color color)
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
        /// <param name="points"></param>
        /// <param name="color"></param>
        public void DrawPoints(IEnumerable<Point> points, Color color)
        {
            foreach (var p in points)
                graphics.DrawPoint(p.X, p.Y, color);
        }

        /// <summary>
        /// Draw a series of points in the given color.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="color"></param>
        public void DrawPoints(Span<Point> points, Color color)
        {
            foreach (var p in points)
                graphics.DrawPoint(p.X, p.Y, color);
        }

        /// <summary>
        /// Draw a series of points in the given color.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="color"></param>
        public void DrawPoints(ReadOnlySpan<Point> points, Color color)
        {
            foreach (var p in points)
                graphics.DrawPoint(p.X, p.Y, color);
        }
    }
}
