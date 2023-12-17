namespace BenMakesGames.PlayPlayMini.Model;

public sealed class SpriteSheet
{
    public required Texture2D Texture { get; init; }
    public required int SpriteWidth { get; init; }
    public required int SpriteHeight { get; init; }
    
    public int Columns => Texture.Width / SpriteWidth;
    public int Rows => Texture.Height / SpriteHeight;
}