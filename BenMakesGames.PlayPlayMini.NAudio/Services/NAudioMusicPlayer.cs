using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using BenMakesGames.PlayPlayMini.NAudio.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace BenMakesGames.PlayPlayMini.NAudio.Services;

public interface INAudioMusicPlayer: IServiceLoadContent, IServiceUpdate
{
    INAudioMusicPlayer SetVolume(float volume);
    float GetVolume();
    bool IsPlaying(string name);
    string[] GetPlayingSongs();
    INAudioMusicPlayer PlaySong(string name, int fadeInMilliseconds = 0);
    INAudioMusicPlayer StopAllSongs(int fadeOutMilliseconds = 0);
    INAudioMusicPlayer StopAllSongsExcept(string[] songsToContinue, int fadeOutMilliseconds = 0);
    INAudioMusicPlayer StopAllSongsExcept(string name, int fadeOutMilliseconds = 0);
    INAudioMusicPlayer StopSong(string name, int fadeOutMilliseconds = 0);
}

/// <summary>
/// Service for playing music using NAudio.
///
/// To support cross-platform, you must register the NAudioMusicPlayer manually.
/// (This may be improved in a future version of PlayPlayMini.NAudio.)
///
/// Register it manually as follows:
///
///     .AddServices((s, c, serviceWatcher) => {
///
///        ...
///
///        s.RegisterType&lt;NAudioMusicPlayer&lt;WaveOutEvent>>().As&lt;INAudioMusicPlayer>()
///            .SingleInstance()
///            .OnActivating(audio => serviceWatcher.RegisterService(audio.Instance));
///     })
///
/// The above will register NAudioMusicPlayer using WaveOutEvent, which only
/// works on Windows. For different OSes, you will need to use a different class.
///
/// Here are some community-made players for other OSes:
/// * MacOS, iOS, and Android: https://github.com/naudio/NAudio/issues/585
/// *
///
/// Inject it into your game state or service, for example:
///
///     public sealed class Battle: GameState
///     {
///         private NAudioMusicPlayer MusicPlayer { get; }
///
///         public MyGameState(NAudioMusicPlayer musicPlayer)
///         {
///             MusicPlayer = musicPlayer;
///         }
///
///         override void Enter()
///         {
///             MusicPlayer
///                 .StopAllSongs(1000)
///                 .PlaySong("BattleTheme");
///         }
///     }
/// </summary>
public sealed class NAudioMusicPlayer<T>: INAudioMusicPlayer, IDisposable
    where T: IWavePlayer, new()
{
    private INAudioPlaybackEngine? PlaybackEngine { get; set; }
    private ILogger<NAudioMusicPlayer<T>> Logger { get; }
    private ILifetimeScope IocContainer { get; }

    public Dictionary<string, (WaveStream Stream, float Gain)> Songs { get; private set; } = new();

    private Dictionary<string, FadeInOutSampleProvider> PlayingSongs { get; } = new();
    private List<(string Name, DateTimeOffset EndTime)> FadingSongs { get; } = new();

    public NAudioMusicPlayer(ILogger<NAudioMusicPlayer<T>> logger, ILifetimeScope iocContainer)
    {
        Logger = logger;
        IocContainer = iocContainer;
    }

    public void LoadContent(GameStateManager gsm)
    {
        Logger.LogInformation("LoadContent() started");

        var allSongs = gsm.Assets.GetAll<NAudioSongMeta>().ToList();

        Songs = allSongs.ToDictionary(meta => meta.Key, _ => ((WaveStream)null!, 0f));

        foreach(var song in allSongs.Where(s => s.PreLoaded))
            LoadSong(song.Key, song.Path, song.Gain);

        Task.Run(() =>
        {
            foreach(var song in allSongs.Where(s => !s.PreLoaded))
                LoadSong(song.Key, song.Path, song.Gain);

            Logger.LogInformation("Fully loaded!");

            FullyLoaded = true;
        });
    }

    public void UnloadContent()
    {
        // unloading is done in Dispose()
    }

    private void LoadSong(string name, string filePath, float gain)
    {
        try
        {
            var stream = CreateWaveStream(filePath);

            if(PlaybackEngine is null)
                PlaybackEngine = new NAudioPlaybackEngine<T>(stream.WaveFormat.SampleRate, stream.WaveFormat.Channels);
            else if(stream.WaveFormat.SampleRate != PlaybackEngine.SampleRate || stream.WaveFormat.Channels != PlaybackEngine.Channels)
            {
                Logger.LogError(
                    "All songs must have the same sample rate and channel count. Song {Name} has a sample rate of {StreamSampleRate} and channel count of {StreamChannelCount}, but other songs have a sample rate of {PlayerSampleRate} and channel count of {PlayerChannelCount}.",
                    name, stream.WaveFormat.SampleRate, stream.WaveFormat.Channels, PlaybackEngine.SampleRate, PlaybackEngine.Channels
                );

                return;
            }

            Songs[name] = (stream, gain);
        }
        catch(Exception e)
        {
            Logger.LogWarning(e, "Failed to load song {Name} from {Path}", name, filePath);
        }
    }

    private WaveStream CreateWaveStream(string filePath)
    {
        var extension = Path.GetExtension(filePath)[1..].ToLower();

        var loader = IocContainer.Resolve<IEnumerable<NAudioFileLoader>>()
            .FirstOrDefault(l => l.Extension == extension);

        if(loader is not null)
            return loader.Load(filePath);

        return new AudioFileReader(filePath);
    }

    /// <summary>
    /// Sets the master volume, affecting all songs.
    ///
    /// This affects songs which are fading in or out as you would intuitively expect: their volume is adjusted
    /// proportional to their current fade, and they will continue to fade in or out at the same rate.
    /// </summary>
    /// <param name="volume"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Throws an InvalidOperationException if called before any songs have been loaded.</exception>
    public INAudioMusicPlayer SetVolume(float volume)
    {
        if (PlaybackEngine is null)
            throw new InvalidOperationException("NAudioMusicPlayer has not been initialized, yet.");

        PlaybackEngine.SetVolume(volume);

        return this;
    }

    /// <summary>
    /// Returns the current master volume level.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Throws an InvalidOperationException if called before any songs have been loaded.</exception>
    public float GetVolume() => PlaybackEngine?.GetVolume() ?? throw new InvalidOperationException("NAudioMusicPlayer has not been initialized, yet.");

    /// <summary>
    /// Returns true if the specified song is currently playing, including if it is fading in or out.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool IsPlaying(string name) => PlayingSongs.ContainsKey(name);

    /// <summary>
    /// Returns a list of all currently-playing songs, including any songs that are fading in or out.
    /// </summary>
    /// <returns></returns>
    public string[] GetPlayingSongs() => PlayingSongs.Keys.ToArray();

    /// <summary>
    /// Play the specified song. If the fadeInMilliseconds is greater than 0, the song will fade in over that duration.
    ///
    /// Currently-playing songs will not be stopped - the new song will play alongside them.
    ///
    /// If the specified song is already playing, another copy of the song will NOT be started. If that song is already
    /// fading in, its fade-in time will NOT be changed.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="fadeInMilliseconds"></param>
    /// <returns></returns>
    public INAudioMusicPlayer PlaySong(string name, int fadeInMilliseconds = 0)
    {
        if(!Songs.TryGetValue(name, out var song))
        {
            Logger.LogWarning("There is no song named {Name}", name);
            return this;
        }

        if(PlayingSongs.ContainsKey(name))
            return this;

        var initiallySilent = fadeInMilliseconds > 0;

        var gainAdjusted = Math.Abs(song.Gain - 1) < 0.001
            ? new LoopStream(song.Stream).ToSampleProvider()
            : new VolumeSampleProvider(new LoopStream(song.Stream).ToSampleProvider()) { Volume = song.Gain };

        var sample = new FadeInOutSampleProvider(gainAdjusted, initiallySilent);

        if(fadeInMilliseconds > 0)
            sample.BeginFadeIn(fadeInMilliseconds);

        PlaybackEngine?.AddSample(sample);
        PlayingSongs.Add(name, sample);

        return this;
    }

    /// <summary>
    /// Stops all songs. If the fadeOutMilliseconds is greater than 0, all songs will fade out over that duration.
    ///
    /// If a song is already fading out, its fade-out time will NOT be changed. However, a fadeOutMilliseconds of 0 will
    /// immediately stop songs, regardless of fade-out time.
    /// </summary>
    /// <param name="fadeOutMilliseconds"></param>
    /// <returns></returns>
    public INAudioMusicPlayer StopAllSongs(int fadeOutMilliseconds = 0)
    {
        if(fadeOutMilliseconds <= 0)
        {
            PlaybackEngine?.RemoveAll();
            PlayingSongs.Clear();
            FadingSongs.Clear();
            return this;
        }

        foreach(var song in PlayingSongs)
        {
            if(FadingSongs.Any(s => s.Name == song.Key))
                continue;

            song.Value.BeginFadeOut(fadeOutMilliseconds);
            FadingSongs.Add((song.Key, DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(fadeOutMilliseconds)));
        }

        return this;
    }

    /// <summary>
    /// Stops all songs, except the ones given in songsToContinue. If the fadeOutMilliseconds is greater than 0, the
    /// songs will fade out over that duration.
    ///
    /// If a song is already fading out, its fade-out time will NOT be changed. However, a fadeOutMilliseconds of 0 will
    /// immediately stop songs, regardless of fade-out time.
    /// </summary>
    /// <param name="songsToContinue"></param>
    /// <param name="fadeOutMilliseconds"></param>
    /// <returns></returns>
    public INAudioMusicPlayer StopAllSongsExcept(string[] songsToContinue, int fadeOutMilliseconds = 0)
    {
        foreach(var song in PlayingSongs)
        {
            if(songsToContinue.Contains(song.Key) || FadingSongs.Any(s => s.Name == song.Key))
                continue;

            song.Value.BeginFadeOut(fadeOutMilliseconds);
            FadingSongs.Add((song.Key, DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(fadeOutMilliseconds)));
        }

        return this;
    }

    /// <summary>
    /// Stops all songs, except the named song. If the fadeOutMilliseconds is greater than 0, the songs will fade out
    /// over that duration.
    ///
    /// If a song is already fading out, its fade-out time will NOT be changed. However, a fadeOutMilliseconds of 0 will
    /// immediately stop songs, regardless of fade-out time.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="fadeOutMilliseconds"></param>
    /// <returns></returns>
    public INAudioMusicPlayer StopAllSongsExcept(string name, int fadeOutMilliseconds = 0)
        => StopAllSongsExcept([ name ], fadeOutMilliseconds);

    /// <summary>
    /// Stops the named song. If the fadeOutMilliseconds is greater than 0, the song will fade out over that duration.
    ///
    /// If the song is already fading out, its fade-out time will NOT be changed. However, a fadeOutMilliseconds of 0
    /// will immediately stop the song, regardless of fade-out time.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="fadeOutMilliseconds"></param>
    /// <returns></returns>
    public INAudioMusicPlayer StopSong(string name, int fadeOutMilliseconds = 0)
    {
        if (!PlayingSongs.TryGetValue(name, out var sample))
            return this;

        var fadingSongIndex = FadingSongs.FindIndex(s => s.Name == name);

        if (fadeOutMilliseconds <= 0)
        {
            PlaybackEngine?.RemoveSample(sample);
            PlayingSongs.Remove(name);

            if(fadingSongIndex >= 0)
                FadingSongs.RemoveAt(fadingSongIndex);

            return this;
        }

        if (fadingSongIndex >= 0)
            return this;

        sample.BeginFadeOut(fadeOutMilliseconds);
        FadingSongs.Add((name, DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(fadeOutMilliseconds)));

        return this;
    }

    public bool FullyLoaded { get; private set; }

    public void Dispose()
    {
        foreach (var song in Songs.Values)
            song.Stream.Dispose();

        Songs.Clear();

        if(PlaybackEngine is IDisposable disposablePlaybackEngine)
            disposablePlaybackEngine.Dispose();
    }

    public void Update(GameTime gameTime)
    {
        var now = DateTimeOffset.UtcNow;

        for (var i = FadingSongs.Count - 1; i >= 0; i--)
        {
            if (now <= FadingSongs[i].EndTime)
                continue;

            var sample = PlayingSongs[FadingSongs[i].Name];

            PlaybackEngine?.RemoveSample(sample);

            PlayingSongs.Remove(FadingSongs[i].Name);
            FadingSongs.RemoveAt(i);
        }
    }
}
