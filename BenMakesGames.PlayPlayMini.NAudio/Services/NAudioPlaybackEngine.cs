using System;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace BenMakesGames.PlayPlayMini.NAudio.Services;

// modified from https://markheath.net/post/fire-and-forget-audio-playback-with
public sealed class NAudioPlaybackEngine: IDisposable
{
    private IWavePlayer OutputDevice { get; }
    private MixingSampleProvider Mixer { get; }

    public int SampleRate => Mixer.WaveFormat.SampleRate;
    public int Channels => Mixer.WaveFormat.Channels;

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
        if(sample.WaveFormat.SampleRate != Mixer.WaveFormat.SampleRate)
        {
            Console.WriteLine($"Warning: Sample's sample rate ({sample.WaveFormat.SampleRate}) does not match mixer sample rate ({Mixer.WaveFormat.SampleRate})");
            return;
        }

        if(sample.WaveFormat.Channels != Mixer.WaveFormat.Channels)
        {
            Console.WriteLine($"Warning: Sample's channel count ({sample.WaveFormat.Channels}) does not match mixer channel count ({Mixer.WaveFormat.Channels})");
            return;
        }

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
