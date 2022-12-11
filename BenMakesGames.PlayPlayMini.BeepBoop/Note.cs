using System;

namespace BenMakesGames.PlayPlayMini.BeepBoop;

public enum Note
{
    C,
    Db,
    D,
    Eb,
    E,
    F,
    Gb,
    G,
    Ab,
    A,
    Bb,
    B    
}

public static class NoteExtensions
{
    public static float GetFrequency(this Note note, int octave)
    {
        // The frequency of the reference note A4 in hertz
        const float referenceFrequency = 440;

        // The pitch of the reference note A4 in the chromatic scale
        const int referencePitch = 9;

        // The pitch of the given note in the chromatic scale
        var pitch = (int)note % 12;

        // The frequency of the given note in hertz
        return referenceFrequency * (float)Math.Pow(2, (pitch - referencePitch) / 12f + octave - 4);
    }
}