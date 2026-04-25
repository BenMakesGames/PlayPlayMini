using Microsoft.Xna.Framework.Graphics;

namespace BenMakesGames.PlayPlayMini.Model;

public sealed record FontSheet(Texture2D Texture, int CharacterWidth, int CharacterHeight, int HorizontalSpacing, int VerticalSpacing, char FirstCharacter)
{
    public int Columns => Texture.Width / CharacterWidth;
    public int Rows => Texture.Height / CharacterHeight;

    public char LastCharacter => (char)(FirstCharacter + Columns * Rows);
}
