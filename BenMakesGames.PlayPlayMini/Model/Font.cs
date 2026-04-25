using System.Collections.Generic;

namespace BenMakesGames.PlayPlayMini.Model;

public sealed record Font(List<FontSheet> Sheets)
{
    public Font(FontSheet fontSheet): this([ fontSheet ])
    {
    }

    /// <summary>
    /// Returns the width of the string, in pixels. Newlines and carriage returns ARE accounted for.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public int ComputeWidth(string text)
    {
        var maxWidth = 0;
        var lineWidth = 0;
        var lastSpacing = 0;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (c == '\r')
                continue;

            if (c == '\n')
            {
                if (lineWidth - lastSpacing > maxWidth)
                    maxWidth = lineWidth - lastSpacing;

                lineWidth = 0;
                lastSpacing = 0;
                continue;
            }

            foreach (var sheet in Sheets)
            {
                if (c >= sheet.FirstCharacter && c < sheet.LastCharacter)
                {
                    lineWidth += sheet.CharacterWidth + sheet.HorizontalSpacing;
                    lastSpacing = sheet.HorizontalSpacing;
                    break;
                }
            }
        }

        if (lineWidth - lastSpacing > maxWidth)
            maxWidth = lineWidth - lastSpacing;

        return maxWidth;
    }
}
