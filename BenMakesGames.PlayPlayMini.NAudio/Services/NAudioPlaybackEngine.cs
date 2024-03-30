using System;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace BenMakesGames.PlayPlayMini.NAudio.Services;

// modified from https://markheath.net/post/fire-and-forget-audio-playback-with
public sealed class NAudioPlaybackEngine: IDisposable
{
    private IWavePlayer OutputDevice { get; }
    private MixingSampleProvider Mixer { get; }

    public NAudioPlaybackEngine(int sampleRate = 44100, int channelCount = 2)
    {
        OutputDevice = new WaveOutEvent();
        Mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
        Mixer.ReadFully = true;
        OutputDevice.Init(Mixer);
        OutputDevice.Play();
    }

    public void SetVolume(float volume)
    {
        OutputDevice.Volume = volume;
    }

    public void AddSample(ISampleProvider sample)
    {
        Mixer.AddMixerInput(sample);
    }

    public void RemoveSample(ISampleProvider sample)
    {
        Mixer.RemoveMixerInput(sample);
    }

    public void RemoveAll()
    {
        Mixer.RemoveAllMixerInputs();
    }

    public void Dispose()
    {
        OutputDevice.Dispose();
    }
}
