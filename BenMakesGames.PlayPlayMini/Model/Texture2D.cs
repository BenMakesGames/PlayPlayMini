using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using Color = System.Drawing.Color;

namespace BenMakesGames.PlayPlayMini.Model;

public struct Texture2D
{
    public readonly int Width;
    public readonly int Height;

    public readonly byte[] Data;

    public Texture2D(int width, int height, byte[] data)
    {
        Width = width;
        Height = height;
        Data = data;
    }

    public Texture2D(int width, int height, Span<Color> data)
    {
        Width = width;
        Height = height;
        Data = new byte[width * height * 4];
        
        for (var i = 0; i < data.Length; i++)
        {
            var color = data[i];
            
            Data[i * 4 + 0] = color.R;
            Data[i * 4 + 1] = color.G;
            Data[i * 4 + 2] = color.B;
            Data[i * 4 + 3] = color.A;
        }
    }
    
    public static Texture2D FromFile(string path)
    {
        using var image = Image.Load<Rgba32>(path);

        var pixels = new byte[image.Width * image.Height * 4];
        image.CopyPixelDataTo(pixels);
        
        return new Texture2D(image.Width, image.Height, pixels);
    }
}