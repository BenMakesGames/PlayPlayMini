using BenMakesGames.PlayPlayMini.Attributes.DI;
using BenMakesGames.PlayPlayMini.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace BenMakesGames.PlayPlayMini.Services;

[AutoRegister]
public sealed class SoundManager : IServiceLoadContent, IDisposable, IServiceUpdate
{
    private NAudioPlaybackEngine PlaybackEngine { get; }
    private ILogger<SoundManager> Logger { get; }

    public Dictionary<string, WaveStream> Songs { get; } = new();
    public Dictionary<string, WaveStream> SoundEffects { get; } = new();
    
    private Dictionary<string, FadeInOutSampleProvider> PlayingSongs { get; } = new();
    private Dictionary<string, DateTimeOffset> FadingSongs { get; } = new();

    public SoundManager(ILogger<SoundManager> logger)
    {
        Logger = logger;

        PlaybackEngine = new NAudioPlaybackEngine();
    }
    
    public void LoadContent(AssetCollection assetCollection)
    {
        foreach(var song in assetCollection.GetAll<SongMeta>().Where(s => s.PreLoaded))
            LoadSong(song.Key, song.Path);

        foreach(var soundEffect in assetCollection.GetAll<SoundEffectMeta>().Where(s => s.PreLoaded))
            LoadSoundEffect(soundEffect.Key, soundEffect.Path);

        Task.Run(() =>
        {
            foreach(var song in assetCollection.GetAll<SongMeta>().Where(s => !s.PreLoaded))
                LoadSong(song.Key, song.Path);
            
            foreach(var soundEffect in assetCollection.GetAll<SoundEffectMeta>().Where(s => !s.PreLoaded))
                LoadSoundEffect(soundEffect.Key, soundEffect.Path);
            
            FullyLoaded = true;
        });
    }

    private void LoadSong(string name, string filePath)
    {
        if(Songs.ContainsKey(name))
        {
            Logger.LogWarning("A song named {Name} has already been loaded", name);
            return;
        }

        try
        {
            var stream = CreateWaveStream($"Content/{filePath}");

            Songs.Add(name, stream);
        }
        catch(Exception e)
        {
            Logger.LogWarning(e, "Failed to load song {Name} from {Path}", name, filePath);
        }
    }

    private void LoadSoundEffect(string name, string filePath)
    {
        if(Songs.ContainsKey(name))
        {
            Logger.LogWarning("A sound effect named {Name} has already been loaded", name);
            return;
        }

        try
        {
            var stream = CreateWaveStream($"Content/{filePath}");

            SoundEffects.Add(name, stream);
        }
        catch(Exception e)
        {
            Logger.LogWarning(e, "Failed to load sound effect {Name} from {Path}", name, filePath);
        }
    }

    private static WaveStream CreateWaveStream(string filePath) => Path.GetExtension(filePath).ToLower() switch
    {
        ".ogg" => new VorbisWaveReader(filePath),
        _ => new AudioFileReader(filePath),
    };

    public void SetVolume(float volume) => PlaybackEngine.SetVolume(volume);

    public bool IsPlaying(string name) => PlayingSongs.ContainsKey(name);

    public SoundManager PlaySound(string name)
    {
        if(!SoundEffects.ContainsKey(name))
        {
            Logger.LogWarning("There is no sound named {Name}", name);
            return this;
        }

        PlaybackEngine.AddSample(SoundEffects[name].ToSampleProvider());

        return this;
    }
    
    public SoundManager PlaySong(string name, int fadeInMilliseconds = 0)
    {
        if(!Songs.ContainsKey(name))
        {
            Logger.LogWarning("There is no song named {Name}", name);
            return this;
        }

        if(PlayingSongs.ContainsKey(name))
            return this;

        var initiallySilent = fadeInMilliseconds > 0;
        
        var sample = new FadeInOutSampleProvider(new LoopStream(Songs[name]).ToSampleProvider(), initiallySilent);
        
        if(fadeInMilliseconds > 0)
            sample.BeginFadeIn(fadeInMilliseconds);
        
        PlaybackEngine.AddSample(sample);
        PlayingSongs.Add(name, sample);

        return this;
    }
    
    public SoundManager StopSongs(int fadeOutMilliseconds = 0)
    {
        if(fadeOutMilliseconds == 0)
        {
            PlaybackEngine.RemoveAll();
            PlayingSongs.Clear();
            FadingSongs.Clear();
            return this;
        }
        
        foreach(var song in PlayingSongs)
        {
            if(FadingSongs.ContainsKey(song.Key))
                continue;
            
            song.Value.BeginFadeOut(fadeOutMilliseconds);
            FadingSongs.Add(song.Key, DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(fadeOutMilliseconds));
        }

        return this;
    }

    public bool FullyLoaded { get; private set; }

    public void Dispose()
    {
        foreach (var song in Songs.Values)
            song.Dispose();
        
        Songs.Clear();
        
        PlaybackEngine.Dispose();
    }

    public void Update(GameTime gameTime)
    {
        var now = DateTimeOffset.UtcNow;
        var toRemove = FadingSongs.Where(kvp => now >= kvp.Value).Select(kvp => kvp.Key);

        foreach(var name in toRemove)
        {
            var sample = PlayingSongs[name];
            
            PlaybackEngine.RemoveSample(sample);
            
            PlayingSongs.Remove(name);
            FadingSongs.Remove(name);
        }
    }
}
