using System.Threading.Tasks;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework.Audio;

namespace BenMakesGames.PlayPlayMini.BeepBoop;

public static class SoundManagerExtensions
{
    /// <summary>
    /// Plays the given sound wave at the given sample rate.
    ///
    /// You should generally play a sound wave using the same sample rate as it was generated with.
    /// </summary>
    /// <param name="soundManager"></param>
    /// <param name="waveData">The sound wave to play.</param>
    /// <param name="sampleRate"></param>
    /// <param name="volume">From 0 to 1.</param>
    public static async Task PlayWave(this SoundManager soundManager, byte[] waveData, int sampleRate = 44100, float volume = 1.0f)
    {
        var v = volume * soundManager.SoundVolume;
        
        if(v <= 0) return;
        
        using var buffer = new DynamicSoundEffectInstance(sampleRate, AudioChannels.Mono);
        buffer.Volume = v;
        buffer.SubmitBuffer(waveData);
        buffer.Play();

        await Task.Delay(sampleRate / 1000);

        while(buffer.State == SoundState.Playing)
            await Task.Delay(50);
    }
}