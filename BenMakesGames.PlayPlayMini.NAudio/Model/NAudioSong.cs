using ATL;
using NAudio.Wave;

namespace BenMakesGames.PlayPlayMini.NAudio.Model;

public sealed class NAudioSong
{
    public required WaveStream WaveStream { get; init; }
    public required float Gain { get; init; }
    public required Track Tags { get; init; }
}
