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
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var result = new StringBuilder();

        var lines = text.Split([ "\r\n", "\r", "\n" ], StringSplitOptions.None);

        for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            var words = lines[lineIndex].Split(' ');

            var lineLength = 0;

            if(lineIndex > 0)
                result.Append('\n');

            for (int wordIndex = 0; wordIndex < words.Length; wordIndex++)
            {
                var wordWidth = words[wordIndex].Length * font.CharacterWidth + (words[wordIndex].Length - 1) * font.HorizontalSpacing;

                // we might be prepending a space:
                if(lineLength > 0)
                    wordWidth += font.CharacterWidth + font.HorizontalSpacing * 2;
                
                if (lineLength + wordWidth > maxWidth)
                {
                    result.Append('\n');
                    
                    if(lineLength > 0)
                        wordWidth -= font.CharacterWidth + font.HorizontalSpacing * 2; // if we prepended a space, take it off again...
                    
                    lineLength = 0;
                }
                else if (lineLength > 0)
                {
                    result.Append(' ');
                }

                result.Append(words[wordIndex]);
                
                lineLength += wordWidth;
            }
        }

        return result.ToString();
    }
}
