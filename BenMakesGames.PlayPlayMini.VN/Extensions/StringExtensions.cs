using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace BenMakesGames.PlayPlayMini.VN.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Uppercases the first letter of the given string, using the given culture.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="cultureInfo"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull("str")]
    public static string? ToUppercaseFirst(this string? str, CultureInfo cultureInfo)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        return char.ToUpper(str[0], cultureInfo) + str[1..];
    }

    /// <summary>
    /// Uppercases the first letter of the given string.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull("str")]
    public static string? ToUppercaseFirst(this string? str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        return char.ToUpper(str[0]) + str[1..];
    }
}
