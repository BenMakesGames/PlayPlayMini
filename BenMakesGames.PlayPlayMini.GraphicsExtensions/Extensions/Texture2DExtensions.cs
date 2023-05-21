using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BenMakesGames.PlayPlayMini.GraphicsExtensions.Extensions;

public static class Texture2DExtensions
{
    public static Texture2D Clone(this Texture2D originalTexture)
    {
        // Get the pixel data from the original texture
        var originalPixels = new Color[originalTexture.Width * originalTexture.Height];
        originalTexture.GetData(originalPixels);

        // Create a new texture and set its data to the original's pixel data
        var clonedTexture = new Texture2D(originalTexture.GraphicsDevice, originalTexture.Width, originalTexture.Height);
        clonedTexture.SetData(originalPixels);

        return clonedTexture;
    }

    public static Texture2D CloneWithFilter(this Texture2D originalTexture, Func<Color, Color> filter)
    {
        var originalPixels = new Color[originalTexture.Width * originalTexture.Height];
        originalTexture.GetData(originalPixels);

        for (var i = 0; i < originalPixels.Length; i++)
            originalPixels[i] = filter(originalPixels[i]);

        var clonedTexture = new Texture2D(originalTexture.GraphicsDevice, originalTexture.Width, originalTexture.Height);
        clonedTexture.SetData(originalPixels);

        return clonedTexture;
    }
}