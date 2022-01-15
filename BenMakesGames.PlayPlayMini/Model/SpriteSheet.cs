using Microsoft.Xna.Framework.Graphics;

namespace BenMakesGames.PlayPlayMini.Model
{
    public struct SpriteSheet
    {
        public Texture2D Texture { get; }
        public int SpriteWidth { get; }
        public int SpriteHeight { get; }
        public int Columns => Texture.Width / SpriteWidth;
        public int Rows => Texture.Height / SpriteHeight;

        public SpriteSheet(Texture2D texture, int spriteWidth, int spriteHeight)
        {
            Texture = texture;
            SpriteWidth = spriteWidth;
            SpriteHeight = spriteHeight;
        }
    }
}
