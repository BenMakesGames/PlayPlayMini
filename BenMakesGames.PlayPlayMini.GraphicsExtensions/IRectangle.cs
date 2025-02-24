using System.Numerics;
using BenMakesGames.PlayPlayMini.Services;

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
    /// <param name="mouse">The mouse manager instance.</param>
    /// <returns>True if the rectangle contains the mouse position, false otherwise.</returns>
    public static bool Contains(this IRectangle<int> rectangle, MouseManager mouse) =>
        rectangle.Contains(mouse.X, mouse.Y);

    /// <summary>
    /// Checks if a rectangle contains the current mouse position.
    /// </summary>
    /// <param name="rectangle">The rectangle to check.</param>
    /// <param name="mouse">The mouse manager instance.</param>
    /// <returns>True if the rectangle contains the mouse position, false otherwise.</returns>
    public static bool Contains(this IRectangle<float> rectangle, MouseManager mouse) =>
        rectangle.Contains(mouse.X, mouse.Y);

    /// <summary>
    /// Checks if a rectangle contains the current mouse position.
    /// </summary>
    /// <param name="rectangle">The rectangle to check.</param>
    /// <param name="mouse">The mouse manager instance.</param>
    /// <returns>True if the rectangle contains the mouse position, false otherwise.</returns>
    public static bool Contains(this IRectangle<double> rectangle, MouseManager mouse) =>
        rectangle.Contains(mouse.X, mouse.Y);

    /// <summary>
    /// Checks if the mouse is within the graphics viewport.
    /// </summary>
    /// <param name="graphics">The graphics manager instance.</param>
    /// <param name="mouse">The mouse manager instance.</param>
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
}
