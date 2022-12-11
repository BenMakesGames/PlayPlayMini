using System;

namespace BenMakesGames.PlayPlayMini.BeepBoop;

public static class WaveGenerator
{
    /// <summary>
    /// Generates a square wave of the given frequency.
    /// </summary>
    /// <param name="frequency"></param>
    /// <param name="options"></param>
    /// <returns>A byte array representing the sound.</returns>
    public static byte[] Square(float frequency, WaveOptions options = default)
    {
        var samplesPerPeriod = (int)(options.SampleRate / frequency);
        var numSamples = (int)(options.SampleRate * options.Duration);
        var attackSamples = (int)(options.SampleRate * options.AttackDuration);
        var decaySamples = (int)(options.SampleRate * options.DecayDuration);

        var wave = new byte[numSamples];

        // Generate the square wave data
        for (var i = 0; i < numSamples; i++)
        {
            var value = (i / samplesPerPeriod) % 2 == 0 ? 0 : options.MaxAmplitude;

            if (i < attackSamples)
            {
                // Attack phase - gradually increase the amplitude from 0 to maxAmplitude
                value *= i / (float)attackSamples;
            }
            else if (i > numSamples - decaySamples)
            {
                // Decay phase - gradually decrease the amplitude from maxAmplitude to 0
                value *= (numSamples - i) / (float)decaySamples;
            }

            wave[i] = (byte)(value * 255);
        }

        return wave;
    }

    /// <summary>
    /// Generates a square wave for the given note, and octave. 
    /// </summary>
    /// <param name="note"></param>
    /// <param name="octave"></param>
    /// <param name="options"></param>
    /// <returns>A byte array representing the sound.</returns>
    public static byte[] Square(Note note, int octave = 4, WaveOptions options = default)
        => Square(note.GetFrequency(octave), options);

    /// <summary>
    /// Generates a sawtooth wave of the given frequency.
    /// </summary>
    /// <param name="frequency"></param>
    /// <param name="options"></param>
    /// <returns>A byte array representing the sound.</returns>
    public static byte[] Sawtooth(float frequency, WaveOptions options = default)
    {
        var samplesPerPeriod = (int)(options.SampleRate / frequency);
        var numSamples = (int)(options.SampleRate * options.Duration);
        var attackSamples = (int)(options.SampleRate * options.AttackDuration);
        var decaySamples = (int)(options.SampleRate * options.DecayDuration);

        var wave = new byte[numSamples];

        // Generate the sawtooth wave data
        for (var i = 0; i < numSamples; i++)
        {
            var value = (i % samplesPerPeriod) / (float)samplesPerPeriod * options.MaxAmplitude;

            if (i < attackSamples)
            {
                // Attack phase - gradually increase the amplitude from 0 to maxAmplitude
                value *= i / (float)attackSamples;
            }
            else if (i > numSamples - decaySamples)
            {
                // Decay phase - gradually decrease the amplitude from maxAmplitude to 0
                value *= (numSamples - i) / (float)decaySamples;
            }

            wave[i] = (byte)(value * 255);
        }

        return wave;
    }

    /// <summary>
    /// Generates a sawtooth wave for the given note, and octave. 
    /// </summary>
    /// <param name="note"></param>
    /// <param name="octave"></param>
    /// <param name="options"></param>
    /// <returns>A byte array representing the sound.</returns>
    public static byte[] Sawtooth(Note note, int octave = 4, WaveOptions options = default)
        => Sawtooth(note.GetFrequency(octave), options);

    /// <summary>
    /// Generates a sine wave of the given frequency.
    /// </summary>
    /// <param name="frequency"></param>
    /// <param name="options"></param>
    /// <returns>A byte array representing the sound.</returns>
    public static byte[] Sine(float frequency, WaveOptions options = default)
    {
        var samplesPerPeriod = (int)(options.SampleRate / frequency);
        var numSamples = (int)(options.SampleRate * options.Duration);
        var attackSamples = (int)(options.SampleRate * options.AttackDuration);
        var decaySamples = (int)(options.SampleRate * options.DecayDuration);

        var wave = new byte[numSamples];

        // Generate the sine wave data
        for (var i = 0; i < numSamples; i++)
        {
            var value = (float)Math.Sin((i % samplesPerPeriod) / (double)samplesPerPeriod * Math.PI * 2) * options.MaxAmplitude;

            if (i < attackSamples)
            {
                // Attack phase - gradually increase the amplitude from 0 to maxAmplitude
                value *= i / (float)attackSamples;
            }
            else if (i > numSamples - decaySamples)
            {
                // Decay phase - gradually decrease the amplitude from maxAmplitude to 0
                value *= (numSamples - i) / (float)decaySamples;
            }

            wave[i] = (byte)(value * 255);
        }

        return wave;
    }

    /// <summary>
    /// Generates a sine wave for the given note, and octave. 
    /// </summary>
    /// <param name="note"></param>
    /// <param name="octave"></param>
    /// <param name="options"></param>
    /// <returns>A byte array representing the sound.</returns>
    public static byte[] Sine(Note note, int octave = 4, WaveOptions options = default)
        => Sine(note.GetFrequency(octave), options);

    /// <summary>
    /// Generates a triangle wave of the given frequency.
    /// </summary>
    /// <param name="frequency"></param>
    /// <param name="options"></param>
    /// <returns>A byte array representing the sound.</returns>
    public static byte[] Triangle(float frequency, WaveOptions options = default)
    {
        var samplesPerPeriod = (int)(options.SampleRate / frequency);
        var numSamples = (int)(options.SampleRate * options.Duration);
        var attackSamples = (int)(options.SampleRate * options.AttackDuration);
        var decaySamples = (int)(options.SampleRate * options.DecayDuration);
        
        var wave = new byte[numSamples];

        // Generate the triangle wave data
        for (var i = 0; i < numSamples; i++)
        {
            var value = (i % samplesPerPeriod) / (float)samplesPerPeriod * 2 * options.MaxAmplitude;

            if (value > options.MaxAmplitude)
                value = 2 * options.MaxAmplitude - value;
            
            if (i < attackSamples)
            {
                // Attack phase - gradually increase the amplitude from 0 to maxAmplitude
                value *= i / (float)attackSamples;
            }
            else if (i > numSamples - decaySamples)
            {
                // Decay phase - gradually decrease the amplitude from maxAmplitude to 0
                value *= (numSamples - i) / (float)decaySamples;
            }
            
            wave[i] = (byte)(value * 255);
        }

        return wave;
    }

    /// <summary>
    /// Generates a triangle wave for the given note, and octave. 
    /// </summary>
    /// <param name="note"></param>
    /// <param name="octave"></param>
    /// <param name="options"></param>
    /// <returns>A byte array representing the sound.</returns>
    public static byte[] Triangle(Note note, int octave = 4, WaveOptions options = default)
        => Triangle(note.GetFrequency(octave), options);
}