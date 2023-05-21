using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions.Extensions;

public static class ColorExtensions
{
    public static Color FindNearestColor(this Color color, IList<Color> palette, Func<Color, Color, double> distanceFunction)
    {
        if(palette.Count == 0)
            throw new ArgumentException("Palette must have at least one color.", nameof(palette));

        if (palette.Count == 1)
            return palette[0];

        var nearestIndex = 0;
        var nearestDistance = distanceFunction(color, palette[0]);

        for (var i = 1; i < palette.Count; i++)
        {
            var distance = distanceFunction(color, palette[i]);
            if (distance < nearestDistance)
            {
                nearestIndex = i;
                nearestDistance = distance;
            }
        }

        return palette[nearestIndex];
    }

    public static double EuclideanDistance(this Color a, Color b)
    {
        return Math.Sqrt(
            Math.Pow(b.R - a.R, 2) +
            Math.Pow(b.G - a.G, 2) +
            Math.Pow(b.B - a.B, 2)
        );
    }

    public static Color IncreaseBrightness(this Color color, double brightness)
    {
        var red = (byte)Math.Clamp(color.R + 255 * brightness, 0, 255);
        var green = (byte)Math.Clamp(color.G + 255 * brightness, 0, 255);
        var blue = (byte)Math.Clamp(color.B + 255 * brightness, 0, 255);

        return new Color(red, green, blue, color.A);
    }

    public static Color IncreaseContrast(this Color color, double contrast)
    {
        var factor = (259 * (contrast + 1)) / (255 * (1 - contrast));

        var red = (byte)Math.Clamp(factor * (color.R - 128) + 128, 0, 255);
        var green = (byte)Math.Clamp(factor * (color.G - 128) + 128, 0, 255);
        var blue = (byte)Math.Clamp(factor * (color.B - 128) + 128, 0, 255);

        return new Color(red, green, blue, color.A);
    }

    public static (Color Min, Color Max) GetMinMax(this IEnumerable<Color> colors)
    {
        var min = new Color(255, 255, 255);
        var max = new Color(0, 0, 0);

        foreach (var c in colors)
        {
            if (c.R < min.R) min.R = c.R;
            if (c.G < min.G) min.G = c.G;
            if (c.B < min.B) min.B = c.B;

            if (c.R > max.R) max.R = c.R;
            if (c.G > max.G) max.G = c.G;
            if (c.B > max.B) max.B = c.B;
        }

        return (min, max);
    }

    public static Color Normalize(this Color color, Color min, Color max) => new(
        (byte)Math.Clamp(255.0 * (color.R - min.R) / (max.R - min.R), 0, 255),
        (byte)Math.Clamp(255.0 * (color.G - min.G) / (max.G - min.G), 0, 255),
        (byte)Math.Clamp(255.0 * (color.B - min.B) / (max.B - min.B), 0, 255),
        color.A
    );
}