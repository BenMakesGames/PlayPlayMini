using Microsoft.Xna.Framework.Graphics;

namespace BenMakesGames.PlayPlayMini.Model;

public sealed record Font(Texture2D Texture, int CharacterWidth, int CharacterHeight, int HorizontalSpacing, int VerticalSpacing, char FirstCharacter)
{
    public int Columns => Texture.Width / CharacterWidth;
    public int Rows => Texture.Height / CharacterHeight;
}
