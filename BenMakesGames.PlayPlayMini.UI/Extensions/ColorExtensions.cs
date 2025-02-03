using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.UI.Extensions;

public static class ColorExtensions
{
    /// <summary>
    /// Calculate the luminosity (perceived brightness) of a color.
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public static float Luminosity(this Color c) => c.R * 0.2126f + c.G * 0.7152f + c.B * 0.0722f;

    /// <summary>
    ///
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public static Color GetContrastingBlackOrWhite(this Color c) => c.Luminosity() < 96 ? Color.White : Color.Black;
}
