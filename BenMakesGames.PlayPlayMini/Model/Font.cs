namespace BenMakesGames.PlayPlayMini.Model;

public sealed class Font
{
    public required Texture2D Texture { get; init; }
    public required int CharacterWidth { get; init; }
    public required int CharacterHeight { get; init; }
    public required int HorizontalSpacing { get; init; }
    public required int VerticalSpacing { get; init; }
    public required char FirstCharacter { get; init; }
    
    public int Columns => Texture.Width / CharacterWidth;
    public int Rows => Texture.Height / CharacterHeight;
}
