using System.Drawing;
using Silk.NET.SDL;
using Color = System.Drawing.Color;

namespace BenMakesGames.PlayPlayMini.Model;

public sealed class SpriteBatch
{
    public void Draw(Texture2D picture, Rectangle screenPosition, Rectangle? clippingRectangle, Color? tint)
    {
    }
    
    public void Draw(Texture2D picture, Rectangle screenPosition, SpriteOptions options)
    {
    }
}

public sealed class SpriteOptions
{
    public Rectangle? ClippingRectangle { get; init; }
    public Color? Tint { get; init; }
    public float Rotation { get; init; }
    public float Scale { get; init; } = 1f;
    public RendererFlip Flip { get; init; }
}