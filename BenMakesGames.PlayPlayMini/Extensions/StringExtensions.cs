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
    public static string WrapText(this string text, FontSheet font, int maxWidth)
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

    /// <summary>Returns this instance as an enumerator.</summary>
    public LineSplitEnumerator GetEnumerator() => this;

    /// <summary>Advances the enumerator to the next line of the span.</summary>
    /// <returns><see langword="true"/> if the enumerator successfully advanced to the next line; <see langword="false"/> if the enumerator has passed the end of the span.</returns>
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

    /// <summary>Gets the line at the current position of the enumerator.</summary>
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

    /// <summary>Returns this instance as an enumerator.</summary>
    public SpaceSplitEnumerator GetEnumerator() => this;

    /// <summary>Advances the enumerator to the next word of the span.</summary>
    /// <returns><see langword="true"/> if the enumerator successfully advanced to the next word; <see langword="false"/> if the enumerator has passed the end of the span.</returns>
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

    /// <summary>Gets the word at the current position of the enumerator.</summary>
    public LineSplitEntry Current { get; private set; }
}

/// <summary>Represents a single line produced by <see cref="LineSplitEnumerator"/>, along with the line-ending separator that followed it.</summary>
/// <remarks>From https://www.meziantou.net/split-a-string-into-lines-without-allocation.htm</remarks>
public readonly ref struct LineSplitEntry
{
    /// <summary>Initializes a new <see cref="LineSplitEntry"/> with the specified line and separator.</summary>
    /// <param name="line">The line content, excluding any trailing line-ending characters.</param>
    /// <param name="separator">The line-ending sequence that followed <paramref name="line"/> in the source, or empty if the line was the last in the source.</param>
    public LineSplitEntry(ReadOnlySpan<char> line, ReadOnlySpan<char> separator)
    {
        Line = line;
        Separator = separator;
    }

    /// <summary>Gets the line content, excluding any trailing line-ending characters.</summary>
    public ReadOnlySpan<char> Line { get; }

    /// <summary>Gets the line-ending sequence that followed <see cref="Line"/> in the source, or empty if the line was the last in the source.</summary>
    public ReadOnlySpan<char> Separator { get; }

    /// <summary>Deconstructs the current <see cref="LineSplitEntry"/> into its line and separator components.</summary>
    /// <param name="line">The line content.</param>
    /// <param name="separator">The line-ending separator.</param>
    /// <remarks>
    /// This enables tuple-style destructuring in a <c>foreach</c> loop, so you can pull out just the pieces you care about:
    /// <code>
    /// foreach (var (line, endOfLine) in new LineSplitEnumerator(text))
    /// {
    ///     // use "line" directly, without having to write "entry.Line"
    /// }
    /// </code>
    /// See <see href="https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/deconstruct#deconstructing-user-defined-types"/> for more on user-defined deconstruction.
    /// </remarks>
    public void Deconstruct(out ReadOnlySpan<char> line, out ReadOnlySpan<char> separator)
    {
        line = Line;
        separator = Separator;
    }

    /// <summary>Implicitly converts a <see cref="LineSplitEntry"/> to a <see cref="ReadOnlySpan{T}"/> of its <see cref="Line"/>.</summary>
    /// <remarks>
    /// This lets you iterate lines directly as <see cref="ReadOnlySpan{T}"/> when you don't need the separator:
    /// <code>
    /// foreach (ReadOnlySpan&lt;char&gt; line in new LineSplitEnumerator(text))
    /// {
    ///     // "line" is the line content, with no line-ending characters
    /// }
    /// </code>
    /// </remarks>
    public static implicit operator ReadOnlySpan<char>(LineSplitEntry entry) => entry.Line;
}
