using System;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace BenMakesGames.PlayPlayMini.NAudio.Services;

public interface INAudioPlaybackEngine
{
    public int SampleRate { get; }
    public int Channels { get; }

    void SetVolume(float volume);
    float GetVolume();

    void AddSample(ISampleProvider sample);
    void RemoveSample(ISampleProvider sample);
    void RemoveAll();
}

// modified from https://markheath.net/post/fire-and-forget-audio-playback-with
public sealed class NAudioPlaybackEngine<T>: INAudioPlaybackEngine, IDisposable
    where T: IWavePlayer, new()
{
    private IWavePlayer OutputDevice { get; }
    private VolumeSampleProvider VolumeControl { get; }
    private MixingSampleProvider Mixer { get; }

    public int SampleRate => Mixer.WaveFormat.SampleRate;
    public int Channels => Mixer.WaveFormat.Channels;

    public NAudioPlaybackEngine(int sampleRate = 44100, int channelCount = 2)
    {
        OutputDevice = new T();
        Mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
        Mixer.ReadFully = true;
        VolumeControl = new VolumeSampleProvider(Mixer);
        OutputDevice.Init(VolumeControl);
        OutputDevice.Play();
    }

    public void SetVolume(float volume)
    {
        VolumeControl.Volume = volume;
    }

    public float GetVolume() => VolumeControl.Volume;

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
