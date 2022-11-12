using Microsoft.Xna.Framework.Graphics;

namespace BenMakesGames.PlayPlayMini.Model;

public sealed record SpriteSheet(Texture2D Texture, int SpriteWidth, int SpriteHeight)
{
    public int Columns => Texture.Width / SpriteWidth;
    public int Rows => Texture.Height / SpriteHeight;
}