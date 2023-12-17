using System.Drawing;
using System.IO;
using Silk.NET.OpenGL;
using StbImageSharp;
using PixelFormat = Silk.NET.OpenGL.PixelFormat;
using PixelType = Silk.NET.OpenGL.PixelType;

namespace BenMakesGames.PlayPlayMini.Model;

// adapted from https://github.com/dotnet/Silk.NET/blob/main/examples/CSharp/OpenGL%20Tutorials/Tutorial%201.4%20-%20Abstractions/Texture.cs
public struct Texture2D
{
    private uint TextureId { get; }
    private GL GL { get; }

    public readonly int Width;
    public readonly int Height;

    public readonly byte[] Data;

    public Texture2D(GL gl, int width, int height, byte[] data)
    {
        GL = gl;
        TextureId = GL.GenTexture();

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, TextureId);
        
        Width = width;
        Height = height;
        Data = data;

        unsafe
        {
            fixed (byte* ptr = Data)
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)Width, (uint)Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
            }
        }
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) GLEnum.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) GLEnum.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) GLEnum.LinearMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) GLEnum.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
            
        GL.GenerateMipmap(TextureTarget.Texture2D);
    }

    public static Texture2D FromFile(GL gl, string path)
    {
        var image = ImageResult.FromMemory(File.ReadAllBytes(path), ColorComponents.RedGreenBlueAlpha);
        
        return new Texture2D(gl, image.Width, image.Height, image.Data);
    }

    public static Texture2D FromColorArray(GL gl, int width, int height, Color[] colors)
    {
        var bytes = new byte[width * height * 4];
        
        for (var i = 0; i < colors.Length; i++)
        {
            var color = colors[i];
            bytes[i * 4 + 0] = color.R;
            bytes[i * 4 + 1] = color.G;
            bytes[i * 4 + 2] = color.B;
            bytes[i * 4 + 3] = color.A;
        }

        return new Texture2D(gl, width, height, bytes);
    }
}