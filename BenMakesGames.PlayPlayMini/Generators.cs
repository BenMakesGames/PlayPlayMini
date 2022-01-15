using System;
using System.Collections.Generic;
using System.Text;

namespace BenMakesGames.PlayPlayMini
{
    public class Generators
    {
        public static IEnumerable<(int x, int y)> Circle(int centerX, int centerY, int radius)
        {
            var radiusSquared = radius * radius;

            for (var x = -radius; x < radius; x++)
            {
                var hh = (int)Math.Sqrt(radiusSquared - x * x);
                var rx = centerX + x;
                var ph = centerY + hh;

                for (var y = centerY - hh; y < ph; y++)
                    yield return (rx, y);
            }
        }

        public static IEnumerable<(int x, int y)> Circle(double centerX, double centerY, double radius)
        {
            var radiusSquared = radius * radius;

            for (var x = -radius; x < radius; x++)
            {
                var hh = (int)Math.Sqrt(radiusSquared - x * x);
                var rx = centerX + x;
                var ph = centerY + hh;

                for (var y = centerY - hh; y < ph; y++)
                    yield return ((int)rx, (int)y);
            }
        }

        public static IEnumerable<(int x, int y)> Line(int x, int y, int x2, int y2)
        {
            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                yield return (x, y);
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
        }
    }
}
