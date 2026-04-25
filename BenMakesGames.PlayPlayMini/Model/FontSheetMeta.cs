namespace BenMakesGames.PlayPlayMini.Model;

/// <param name="Path">Relative path to image, excluding file extension (ex: "Fonts/Consolas")</param>
/// <param name="Width">Width of an individual character</param>
/// <param name="Height">Height of an individual character</param>
public sealed record FontSheetMeta(string Path, int Width, int Height)
{
    public char FirstCharacter { get; init; } = ' ';
    public int HorizontalSpacing { get; init; } = 1;
    public int VerticalSpacing { get; init; } = 1;
}
