using System;
using NAudio.Wave;

namespace BenMakesGames.PlayPlayMini.NAudio.Services;

public sealed class NAudioFileLoader
{
    private Func<string, WaveStream> Loader { get; }
    public string Extension { get; }

    public NAudioFileLoader(string extension, Func<string, WaveStream> loader)
    {
        Loader = loader;
        Extension = extension.ToLower();
    }

    public WaveStream Load(string path) => Loader(path);
}
