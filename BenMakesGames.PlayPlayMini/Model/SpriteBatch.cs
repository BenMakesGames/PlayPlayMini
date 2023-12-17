using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using Color = System.Drawing.Color;

namespace BenMakesGames.PlayPlayMini.Model;

public sealed class SpriteBatch
{
    private GL GL { get; }
    private bool IsDrawing { get; set; }

    public SpriteBatch(GL gl)
    {
        GL = gl;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Draw(Texture2D picture, Rectangle screenPosition, Rectangle? clippingRectangle, Color? tint)
        => Draw(picture, screenPosition, new SpriteOptions
        {
            ClippingRectangle = clippingRectangle,
            Tint = tint
        });

    public void Begin()
    {
        if (IsDrawing)
            throw new InvalidOperationException("SpriteBatch.Begin() called while already drawing.");

        IsDrawing = true;
        
        
    }
    
    public void Draw(Texture2D picture, Rectangle screenPosition, SpriteOptions options)
    {
    }

    public void End()
    {
        if (!IsDrawing)
            throw new InvalidOperationException("SpriteBatch.End() called while not drawing.");

        IsDrawing = false;
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