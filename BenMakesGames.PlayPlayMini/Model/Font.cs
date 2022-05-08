using Microsoft.Xna.Framework.Graphics;

namespace BenMakesGames.PlayPlayMini.Model;

public struct Font
{
    public Texture2D Texture { get; }
    public int CharacterWidth { get; }
    public int CharacterHeight { get; }
    public int Columns => Texture.Width / CharacterWidth;
    public int Rows => Texture.Height / CharacterHeight;

    public Font(Texture2D texture, int spriteWidth, int spriteHeight)
    {
        Texture = texture;
        CharacterWidth = spriteWidth;
        CharacterHeight = spriteHeight;
    }
}