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
            return string.Empty;

        var result = new StringBuilder();

        var lines = new LineSplitEnumerator(text);

        foreach(var (line, _) in lines)
        {
            var words = new SpaceSplitEnumerator(line);

            var lineLength = 0;

            if(result.Length > 0)
                result.Append('\n');

            foreach(var (word, _) in words)
            {
                var wordWidth = word.Length * font.CharacterWidth + (word.Length - 1) * font.HorizontalSpacing;

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

                result.Append(word);

                lineLength += wordWidth;
            }
        }

        return result.ToString();
    }
}

/// <summary>
/// Supports zero-allocation splitting of a string into lines.
/// From https://www.meziantou.net/split-a-string-into-lines-without-allocation.htm
/// </summary>
/// <example><code>
/// var lines = new LineSplitEnumerator("Text\nto\r\nsplit");
///&nbsp;
/// foreach(var (line, _) in lines)
/// {
///     // do something with "line"
/// }
/// </code></example>
public ref struct LineSplitEnumerator
{
    private ReadOnlySpan<char> Str;

    public LineSplitEnumerator(ReadOnlySpan<char> str)
    {
        Str = str;
        Current = default;
    }

    // Needed to be compatible with the foreach operator
    public LineSplitEnumerator GetEnumerator() => this;

    public bool MoveNext()
    {
        var span = Str;
        if (span.Length == 0) // Reach the end of the string
            return false;

        var index = span.IndexOfAny('\r', '\n');
        if (index == -1) // The string is composed of only one line
        {
            Str = ReadOnlySpan<char>.Empty; // The remaining string is an empty string
            Current = new LineSplitEntry(span, ReadOnlySpan<char>.Empty);
            return true;
        }

        if (index < span.Length - 1 && span[index] == '\r')
        {
            // Try to consume the '\n' associated to the '\r'
            var next = span[index + 1];
            if (next == '\n')
            {
                Current = new LineSplitEntry(span.Slice(0, index), span.Slice(index, 2));
                Str = span.Slice(index + 2);
                return true;
            }
        }

        Current = new LineSplitEntry(span.Slice(0, index), span.Slice(index, 1));
        Str = span.Slice(index + 1);
        return true;
    }

    public LineSplitEntry Current { get; private set; }
}

/// <summary>
/// Supports zero-allocation splitting of a string into words.
/// From https://www.meziantou.net/split-a-string-into-lines-without-allocation.htm
/// </summary>
public ref struct SpaceSplitEnumerator
{
    private ReadOnlySpan<char> Str;

    public SpaceSplitEnumerator(ReadOnlySpan<char> str)
    {
        Str = str;
        Current = default;
    }

    // Needed to be compatible with the foreach operator
    public SpaceSplitEnumerator GetEnumerator() => this;

    public bool MoveNext()
    {
        var span = Str;
        if (span.Length == 0) // Reach the end of the string
            return false;

        var index = span.IndexOf(' ');
        if (index == -1) // The string is composed of only one line
        {
            Str = ReadOnlySpan<char>.Empty; // The remaining string is an empty string
            Current = new LineSplitEntry(span, ReadOnlySpan<char>.Empty);
            return true;
        }

        Current = new LineSplitEntry(span.Slice(0, index), span.Slice(index, 1));
        Str = span.Slice(index + 1);
        return true;
    }

    public LineSplitEntry Current { get; private set; }
}

/// <summary>
/// Supports zero-allocation splitting of a string into lines.
/// From https://www.meziantou.net/split-a-string-into-lines-without-allocation.htm
/// </summary>
public readonly ref struct LineSplitEntry
{
    public LineSplitEntry(ReadOnlySpan<char> line, ReadOnlySpan<char> separator)
    {
        Line = line;
        Separator = separator;
    }

    public ReadOnlySpan<char> Line { get; }
    public ReadOnlySpan<char> Separator { get; }

    // This method allow to deconstruct the type, so you can write any of the following code
    // foreach (var entry in str.SplitLines()) { _ = entry.Line; }
    // foreach (var (line, endOfLine) in str.SplitLines()) { _ = line; }
    // https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/deconstruct?WT.mc_id=DT-MVP-5003978#deconstructing-user-defined-types
    public void Deconstruct(out ReadOnlySpan<char> line, out ReadOnlySpan<char> separator)
    {
        line = Line;
        separator = Separator;
    }

    // This method allow to implicitly cast the type into a ReadOnlySpan<char>, so you can write the following code
    // foreach (ReadOnlySpan<char> entry in str.SplitLines())
    public static implicit operator ReadOnlySpan<char>(LineSplitEntry entry) => entry.Line;
}
