using System;
using System.Text;
using BenMakesGames.PlayPlayMini.Model;

namespace BenMakesGames.PlayPlayMini.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Add newlines to a string so that it fits within a given width.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="font"></param>
    /// <param name="maxWidth"></param>
    /// <returns></returns>
    public static string WrapText(this string text, Font font, int maxWidth)
    {
        var characterWidth = font.CharacterWidth;
        
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var result = new StringBuilder();

        var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        foreach (var line in lines)
        {
            var words = line.Split(' ');

            var lineLength = 0;

            foreach (string word in words)
            {
                var wordWidth = (word.Length + 1) * characterWidth;
                if (lineLength + wordWidth > maxWidth)
                {
                    result.Append('\n');
                    lineLength = 0;
                }

                result.Append(word + " ");
                lineLength += wordWidth;
            }

            result.Append('\n');
        }

        return result.ToString().TrimEnd();
    }
}