using System.Runtime.CompilerServices;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions;

/// <summary>
/// Extension methods for drawing ellipses and circles using the GraphicsManager.
/// </summary>
public static class EllipseExtensions
{
    /// <param name="graphics">The graphics manager instance.</param>
    extension(GraphicsManager graphics)
    {
        /// <summary>
        /// Draws a filled circle centered at the specified Vector2 position.
        /// </summary>
        /// <param name="center">Center point of the circle.</param>
        /// <param name="radius">Radius of the circle.</param>
        /// <param name="fillColor">Fill color of the circle.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawFilledCircle(Vector2 center, int radius, Color fillColor)
            => graphics.DrawFilledEllipse((int)center.X - radius, (int)center.Y - radius, radius * 2 + 1, radius * 2 + 1, fillColor);

        /// <summary>
        /// Draws a circle outline centered at the specified Vector2 position.
        /// </summary>
        /// <param name="center">Center point of the circle.</param>
        /// <param name="radius">Radius of the circle.</param>
        /// <param name="outlineColor">Color of the circle outline.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawCircle(Vector2 center, int radius, Color outlineColor)
            => graphics.DrawFilledEllipse((int)center.X - radius, (int)center.Y - radius, radius * 2 + 1, radius * 2 + 1, outlineColor);

        /// <summary>
        /// Draws a filled circle centered at the specified coordinates.
        /// </summary>
        /// <param name="centerX">X coordinate of the circle center.</param>
        /// <param name="centerY">Y coordinate of the circle center.</param>
        /// <param name="radius">Radius of the circle.</param>
        /// <param name="fillColor">Fill color of the circle.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawFilledCircle(int centerX, int centerY, int radius, Color fillColor)
            => graphics.DrawFilledEllipse(centerX - radius, centerY - radius, radius * 2 + 1, radius * 2 + 1, fillColor);

        /// <summary>
        /// Draws a circle outline centered at the specified coordinates.
        /// </summary>
        /// <param name="centerX">X coordinate of the circle center.</param>
        /// <param name="centerY">Y coordinate of the circle center.</param>
        /// <param name="radius">Radius of the circle.</param>
        /// <param name="outlineColor">Color of the circle outline.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawCircle(int centerX, int centerY, int radius, Color outlineColor)
            => graphics.DrawEllipse(centerX - radius, centerY - radius, radius * 2 + 1, radius * 2 + 1, outlineColor);

        /// <summary>
        /// Draws a filled ellipse that fits within the specified rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle that defines the ellipse bounds.</param>
        /// <param name="fillColor">Fill color of the ellipse.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawFilledEllipse(Rectangle rectangle, Color fillColor)
            => graphics.DrawFilledEllipse(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, fillColor);

        /// <summary>
        /// Draws a filled ellipse that occupies the rectangle given by (x, y, width, height).
        /// </summary>
        /// <param name="x">X coordinate of the upper-left corner.</param>
        /// <param name="y">Y coordinate of the upper-left corner.</param>
        /// <param name="width">Width of the bounding rectangle.</param>
        /// <param name="height">Height of the bounding rectangle.</param>
        /// <param name="fillColor">Fill color of the ellipse.</param>
        public void DrawFilledEllipse(int x, int y, int width, int height, Color fillColor)
        {
            if (width < 1 || height < 1) return;

            if (width == 1)
            {
                graphics.DrawFilledRectangle(x, y, 1, height, fillColor);
                return;
            }

            if (height == 1)
            {
                graphics.DrawFilledRectangle(x, y, width, 1, fillColor);
                return;
            }

            graphics.DrawFilledEllipse_Sheldon(x, y, x + width - 1, y + height - 1, fillColor);
        }

        /// <summary>
        /// Draws an ellipse outline that fits within the specified rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle that defines the ellipse bounds.</param>
        /// <param name="outlineColor">Color of the ellipse outline.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawEllipse(Rectangle rectangle, Color outlineColor)
            => graphics.DrawEllipse(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, outlineColor);

        /// <summary>
        /// Draws an ellipse outline that occupies the rectangle given by (x, y, width, height).
        /// </summary>
        /// <param name="x">X coordinate of the upper-left corner.</param>
        /// <param name="y">Y coordinate of the upper-left corner.</param>
        /// <param name="width">Width of the bounding rectangle.</param>
        /// <param name="height">Height of the bounding rectangle.</param>
        /// <param name="outlineColor">Color of the ellipse outline.</param>
        public void DrawEllipse(int x, int y, int width, int height, Color outlineColor)
        {
            if (width < 1 || height < 1) return;

            if (width == 1)
            {
                graphics.DrawRectangle(x, y, 1, height, outlineColor);
                return;
            }

            if (height == 1)
            {
                graphics.DrawRectangle(x, y, width, 1, outlineColor);
                return;
            }

            graphics.DrawEllipse_Sheldon(x, y, x + width - 1, y + height - 1, outlineColor);
        }

        /// <remarks>
        /// from https://stackoverflow.com/questions/2914807/plot-ellipse-from-rectangle
        /// </remarks>
        private void DrawFilledEllipse_Sheldon(int x0, int y0, int x1, int y1, Color color)
        {
            int xc, yc;

            // Calculate height
            var yb = yc = (y0 + y1) / 2;
            var qb = (y0 < y1) ? (y1 - y0) : (y0 - y1);
            var qy = qb;
            var dy = qb / 2;
            if (qb % 2 != 0)
                // Bounding box has even pixel height
                yc++;

            // Calculate width
            var xb = xc = (x0 + x1) / 2;
            var qa = (x0 < x1) ? (x1 - x0) : (x0 - x1);
            var qx = qa % 2;
            var dx = 0;
            var qt = (long)qa * qa + (long)qb * qb - 2L * qa * qa * qb;
            if (qx != 0)
            {
                // Bounding box has even pixel width
                xc++;
                qt += 3L * qb * qb;
            }

            // Start at (dx, dy) = (0, b) and iterate until (a, 0) is reached
            while (qy >= 0 && qx <= qa)
            {
                // Draw the new points

                // If a (+1, 0) step stays inside the ellipse, do it
                if (qt + 2L * qb * qb * qx + 3L * qb * qb <= 0L ||
                    qt + 2L * qa * qa * qy - (long)qa * qa <= 0L)
                {
                    qt += 8L * qb * qb + 4L * qb * qb * qx;
                    dx++;
                    qx += 2;
                    // If a (0, -1) step stays outside the ellipse, do it
                }
                else if (qt - 2L * qa * qa * qy + 3L * qa * qa > 0L)
                {
                    graphics.DrawRow(xb - dx, xc + dx, yc + dy, color);

                    if (dy != 0 || yb != yc)
                        graphics.DrawRow(xb - dx, xc + dx, yb - dy, color);

                    qt += 8L * qa * qa - 4L * qa * qa * qy;
                    dy--;
                    qy -= 2;
                }
                else
                {
                    graphics.DrawRow(xb - dx, xc + dx, yc + dy, color);

                    if (dy != 0 || yb != yc)
                        graphics.DrawRow(xb - dx, xc + dx, yb - dy, color);

                    qt += 8L * qb * qb + 4L * qb * qb * qx + 8L * qa * qa - 4L * qa * qa * qy;
                    dx++;
                    qx += 2;
                    dy--;
                    qy -= 2;
                }
            }
        }

        /// <remarks>
        /// from https://stackoverflow.com/questions/2914807/plot-ellipse-from-rectangle
        /// </remarks>
        private void DrawEllipse_Sheldon(int x0, int y0, int x1, int y1, Color color)
        {
            int xc, yc;

            // Calculate height
            var yb = yc = (y0 + y1) / 2;
            var qb = (y0 < y1) ? (y1 - y0) : (y0 - y1);
            var qy = qb;
            var dy = qb / 2;
            if (qb % 2 != 0)
                // Bounding box has even pixel height
                yc++;

            // Calculate width
            var xb = xc = (x0 + x1) / 2;
            var qa = (x0 < x1) ? (x1 - x0) : (x0 - x1);
            var qx = qa % 2;
            var dx = 0;
            var qt = (long)qa * qa + (long)qb * qb - 2L * qa * qa * qb;
            if (qx != 0)
            {
                // Bounding box has even pixel width
                xc++;
                qt += 3L * qb * qb;
            }

            // Start at (dx, dy) = (0, b) and iterate until (a, 0) is reached
            while (qy >= 0 && qx <= qa)
            {
                // Draw the new points
                // TODO: can this method be optimized by drawing rectangles when many points are in a line?
                graphics.DrawPoint(xb - dx, yb - dy, color);

                if (dx != 0 || xb != xc)
                {
                    graphics.DrawPoint(xc + dx, yb - dy, color);

                    if (dy != 0 || yb != yc)
                        graphics.DrawPoint(xc + dx, yc + dy, color);
                }

                if (dy != 0 || yb != yc)
                    graphics.DrawPoint(xb - dx, yc + dy, color);

                // If a (+1, 0) step stays inside the ellipse, do it
                if (qt + 2L * qb * qb * qx + 3L * qb * qb <= 0L ||
                    qt + 2L * qa * qa * qy - (long)qa * qa <= 0L)
                {
                    qt += 8L * qb * qb + 4L * qb * qb * qx;
                    dx++;
                    qx += 2;
                    // If a (0, -1) step stays outside the ellipse, do it
                }
                else if (qt - 2L * qa * qa * qy + 3L * qa * qa > 0L)
                {
                    qt += 8L * qa * qa - 4L * qa * qa * qy;
                    dy--;
                    qy -= 2;
                }
                else
                {
                    qt += 8L * qb * qb + 4L * qb * qb * qx + 8L * qa * qa - 4L * qa * qa * qy;
                    dx++;
                    qx += 2;
                    dy--;
                    qy -= 2;
                }
            }
        }
    }
}
