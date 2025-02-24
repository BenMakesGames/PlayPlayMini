using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions;

/// <summary>
/// Extension methods for color-related operations.
/// </summary>
public static class ColorExtensions
{
    /// <summary>
    /// Calculates the relative luminosity of a color using the standard coefficients for RGB to grayscale conversion.
    /// </summary>
    /// <param name="c">The color to calculate luminosity for.</param>
    /// <returns>The relative luminosity value between 0 and 255.</returns>
    /// <remarks>
    /// Uses the standard coefficients for RGB to grayscale conversion:
    /// Red: 0.2126, Green: 0.7152, Blue: 0.0722
    /// </remarks>
    public static float Luminosity(this Color c) => c.R * 0.2126f + c.G * 0.7152f + c.B * 0.0722f;

    /// <summary>
    /// Returns either black or white color that provides the best contrast with the given color.
    /// </summary>
    /// <param name="c">The color to find a contrasting color for.</param>
    /// <returns>White if the input color's luminosity is less than 96, Black otherwise.</returns>
    public static Color GetContrastingBlackOrWhite(this Color c) => c.Luminosity() < 96 ? Color.White : Color.Black;
}
