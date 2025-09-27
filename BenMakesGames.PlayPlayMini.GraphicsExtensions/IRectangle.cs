using System.Numerics;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions;

/// <summary>
/// Interface for a generic rectangle with numeric coordinates and dimensions.
/// </summary>
/// <typeparam name="T">The numeric type used for coordinates and dimensions.</typeparam>
public interface IRectangle<out T> where T: INumber<T>
{
    T X { get; }
    T Y { get; }
    T Width { get; }
    T Height { get; }
}

/// <summary>
/// Extension methods for rectangle-related operations.
/// </summary>
public static class RectangleExtensions
{
    /// <summary>
    /// Checks if a rectangle contains the current mouse position.
    /// </summary>
    /// <param name="rectangle">The rectangle to check.</param>
    /// <param name="mouse">The <see cref="MouseManager">MouseManager</see> instance.</param>
    /// <returns>True if the rectangle contains the mouse position, false otherwise.</returns>
    public static bool Contains(this IRectangle<int> rectangle, MouseManager mouse) =>
        rectangle.Contains(mouse.X, mouse.Y);

    /// <summary>
    /// Checks if a rectangle contains the current mouse position.
    /// </summary>
    /// <param name="rectangle">The rectangle to check.</param>
    /// <param name="mouse">The <see cref="MouseManager">MouseManager</see> instance.</param>
    /// <returns>True if the rectangle contains the mouse position, false otherwise.</returns>
    public static bool Contains(this IRectangle<float> rectangle, MouseManager mouse) =>
        rectangle.Contains(mouse.X, mouse.Y);

    /// <summary>
    /// Checks if a rectangle contains the current mouse position.
    /// </summary>
    /// <param name="rectangle">The rectangle to check.</param>
    /// <param name="mouse">The <see cref="MouseManager">MouseManager</see> instance.</param>
    /// <returns>True if the rectangle contains the mouse position, false otherwise.</returns>
    public static bool Contains(this IRectangle<double> rectangle, MouseManager mouse) =>
        rectangle.Contains(mouse.X, mouse.Y);

    /// <summary>
    /// Checks if the mouse is within the graphics viewport.
    /// </summary>
    /// <param name="graphics">The graphics manager instance.</param>
    /// <param name="mouse">The <see cref="MouseManager">MouseManager</see> instance.</param>
    /// <returns>True if the mouse is within the viewport, false otherwise.</returns>
    public static bool Contains(this GraphicsManager graphics, MouseManager mouse) =>
        mouse.X >= 0 && mouse.X < graphics.Width &&
        mouse.Y >= 0 && mouse.Y < graphics.Height;

    /// <summary>
    /// Checks if a rectangle contains a point specified by coordinates.
    /// </summary>
    /// <typeparam name="T">The numeric type used for coordinates.</typeparam>
    /// <param name="rectangle">The rectangle to check.</param>
    /// <param name="x">X coordinate of the point.</param>
    /// <param name="y">Y coordinate of the point.</param>
    /// <returns>True if the rectangle contains the point, false otherwise.</returns>
    public static bool Contains<T>(this IRectangle<T> rectangle, T x, T y) where T: INumber<T> =>
        x >= rectangle.X && x < rectangle.X + rectangle.Width &&
        y >= rectangle.Y && y < rectangle.Y + rectangle.Height;

    /// <summary>
    /// Checks if a rectangle contains a point specified by coordinates.
    /// </summary>
    /// <param name="rectangle">The rectangle to check.</param>
    /// <param name="x">X coordinate of the point.</param>
    /// <param name="y">Y coordinate of the point.</param>
    /// <returns>True if the rectangle contains the point, false otherwise.</returns>
    public static bool Contains(this Rectangle rectangle, int x, int y) =>
        x >= rectangle.X && x < rectangle.X + rectangle.Width &&
        y >= rectangle.Y && y < rectangle.Y + rectangle.Height;

    /// <summary>
    /// Checks if the mouse is within the graphics viewport.
    /// </summary>
    /// <param name="rectangle">The rectangle to check.</param>
    /// <param name="mouse">The <see cref="MouseManager">MouseManager</see> instance.</param>
    /// <returns>True if the rectangle contains the point, false otherwise.</returns>
    public static bool Contains(this Rectangle rectangle, MouseManager mouse) =>
        mouse.X >= rectangle.X && mouse.X < rectangle.X + rectangle.Width &&
        mouse.Y >= rectangle.Y && mouse.Y < rectangle.Y + rectangle.Height;

    /// <summary>
    /// Draws the outline of a rectangle.
    /// </summary>
    /// <param name="graphics"></param>
    /// <param name="rectangle"></param>
    /// <param name="outlineColor"></param>
    public static void DrawRectangle(this GraphicsManager graphics, IRectangle<int> rectangle, Color outlineColor)
        => graphics.DrawRectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, outlineColor);

    /// <summary>
    /// Draws a filled rectangle.
    /// </summary>
    /// <param name="graphics"></param>
    /// <param name="rectangle"></param>
    /// <param name="fillColor"></param>
    public static void DrawFilledRectangle(this GraphicsManager graphics, IRectangle<int> rectangle, Color fillColor)
        => graphics.DrawFilledRectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, fillColor);

    /// <summary>
    /// Draws a filled rectangle.
    /// </summary>
    /// <param name="graphics"></param>
    /// <param name="rectangle"></param>
    /// <param name="fillColor"></param>
    /// <param name="outlineColor"></param>
    public static void DrawFilledRectangle(this GraphicsManager graphics, IRectangle<int> rectangle, Color fillColor, Color outlineColor)
        => graphics.DrawFilledRectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, fillColor, outlineColor);
}
