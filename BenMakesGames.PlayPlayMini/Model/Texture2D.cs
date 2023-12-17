using System.Drawing;
using Silk.NET.OpenGL;

namespace BenMakesGames.PlayPlayMini.Model;

public struct Texture2D
{
    public readonly uint BufferId;
    public readonly uint TextureId;
    public readonly int Width;
    public readonly int Height;

    public readonly byte[] Data;

    public Texture2D(GL gl, int width, int height, byte[] data)
    {
        BufferId = gl.GenBuffer();
        TextureId = gl.GenTexture();
        Width = width;
        Height = height;
        Data = data;
    }

    public Texture2D(GL gl, int width, int height, Color[] data)
    {
        BufferId = gl.GenBuffer();
        TextureId = gl.GenTexture();
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
    
    public static Texture2D FromFile(GL gl, string path)
    {
        // TODO
        return new Texture2D(gl, 0, 0, new byte[] { });
    }
}