using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ATL;
using Autofac;
using BenMakesGames.PlayPlayMini.NAudio.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace BenMakesGames.PlayPlayMini.NAudio.Services;

public interface INAudioMusicPlayer: IServiceLoadContent, IServiceUpdate
{
    void SetVolume(float volume);
    float GetVolume();
    bool IsPlaying(string name);
    IEnumerable<string> GetPlayingSongs();
    bool PlaySong(string name, int fadeInMilliseconds = 0, long? startPosition = null, bool loop = true);
    void StopAllSongs(int fadeOutMilliseconds = 0);
    void StopAllSongsExcept(string[] songsToContinue, int fadeOutMilliseconds = 0);
    void StopAllSongsExcept(string name, int fadeOutMilliseconds = 0);
    void StopSong(string name, int fadeOutMilliseconds = 0);
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
///             MusicPlayer.StopAllSongs(1000);
///             MusicPlayer.PlaySong("BattleTheme");
///         }
///     }
/// </summary>
public sealed class NAudioMusicPlayer<T>: INAudioMusicPlayer, IDisposable
    where T: IWavePlayer, new()
{
    private INAudioPlaybackEngine? PlaybackEngine { get; set; }
    private ILogger<NAudioMusicPlayer<T>> Logger { get; }
    private ILifetimeScope IocContainer { get; }

    public Dictionary<string, NAudioSong> Songs { get; } = new();

    private Dictionary<string, PlayingSong> PlayingSongs { get; } = new();

    private List<(string Name, ISampleProvider SampleProvider, DateTimeOffset EndTime)> FadingSongs { get; } = new();

    public NAudioMusicPlayer(ILogger<NAudioMusicPlayer<T>> logger, ILifetimeScope iocContainer)
    {
        Logger = logger;
        IocContainer = iocContainer;
    }

    public void LoadContent(GameStateManager gsm)
    {
        Logger.LogInformation("LoadContent() started");

        var allSongs = gsm.Assets.GetAll<NAudioSongMeta>().ToList();

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

            Songs[name] = new NAudioSong()
            {
                WaveStream = stream,
                Gain = gain,
                Tags = new Track(filePath)
            };
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
    public void SetVolume(float volume)
    {
        if (PlaybackEngine is null)
            throw new InvalidOperationException("NAudioMusicPlayer has not been initialized, yet.");

        PlaybackEngine.SetVolume(volume);
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
    public bool IsPlaying(string name) => PlayingSongs.TryGetValue(name, out var song) && song.IsPlaying;

    /// <summary>
    /// Returns a list of all currently-playing songs, including any songs that are fading in or out.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetPlayingSongs() => PlayingSongs.Keys.AsEnumerable();

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
    /// <param name="startPosition">Position to start song from. If null, song resumes from where it left off.</param>
    /// <param name="loop"></param>
    /// <returns></returns>
    public bool PlaySong(string name, int fadeInMilliseconds = 0, long? startPosition = null, bool loop = true)
    {
        if(!Songs.TryGetValue(name, out var song))
        {
            Logger.LogWarning("There is no song named {Name}", name);
            return false;
        }

        if(PlayingSongs.ContainsKey(name))
            return false;

        if(startPosition.HasValue)
            song.WaveStream.Position = startPosition.Value;

        var playingSong = new PlayingSong(name, song, fadeInMilliseconds);

        playingSong.Loop(loop);

        PlaybackEngine?.AddSample(playingSong.SampleProvider);
        PlayingSongs.Add(name, playingSong);

        return true;
    }

    /// <summary>
    /// Stops all songs. If the fadeOutMilliseconds is greater than 0, all songs will fade out over that duration.
    ///
    /// If a song is already fading out, its fade-out time will NOT be changed. However, a fadeOutMilliseconds of 0 will
    /// immediately stop songs, regardless of fade-out time.
    /// </summary>
    /// <param name="fadeOutMilliseconds"></param>
    /// <returns></returns>
    public void StopAllSongs(int fadeOutMilliseconds = 0)
    {
        if(fadeOutMilliseconds <= 0)
        {
            PlaybackEngine?.RemoveAll();
            PlayingSongs.Clear();
            FadingSongs.Clear();
            return;
        }

        foreach(var song in PlayingSongs)
        {
            if(FadingSongs.Any(s => s.Name == song.Key))
                continue;

            song.Value.BeginFadeOut(fadeOutMilliseconds);
            FadingSongs.Add((song.Key, song.Value.SampleProvider, DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(fadeOutMilliseconds)));
        }
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
    public void StopAllSongsExcept(string[] songsToContinue, int fadeOutMilliseconds = 0)
    {
        foreach(var song in PlayingSongs)
        {
            if(songsToContinue.Contains(song.Key) || FadingSongs.Any(s => s.Name == song.Key))
                continue;

            song.Value.BeginFadeOut(fadeOutMilliseconds);
            FadingSongs.Add((song.Key, song.Value.SampleProvider, DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(fadeOutMilliseconds)));
        }
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
    public void StopAllSongsExcept(string name, int fadeOutMilliseconds = 0)
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
    public void StopSong(string name, int fadeOutMilliseconds = 0)
    {
        if (!PlayingSongs.TryGetValue(name, out var song))
        {
            Logger.LogInformation("Song {Name} is not playing", name);
            return;
        }

        var fadingSongIndex = FadingSongs.FindIndex(s => s.Name == name);

        if (fadeOutMilliseconds <= 0)
        {
            PlaybackEngine?.RemoveSample(song.SampleProvider);
            PlayingSongs.Remove(name);

            if(fadingSongIndex >= 0)
                FadingSongs.RemoveAt(fadingSongIndex);
        }

        if (fadingSongIndex >= 0)
            return;

        song.BeginFadeOut(fadeOutMilliseconds);
        FadingSongs.Add((name, song.SampleProvider, DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(fadeOutMilliseconds)));
    }

    /// <inheritdoc />
    public bool FullyLoaded { get; private set; }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var song in Songs.Values)
            song.WaveStream.Dispose();

        Songs.Clear();

        if(PlaybackEngine is IDisposable disposablePlaybackEngine)
            disposablePlaybackEngine.Dispose();
    }

    /// <inheritdoc />
    public void Update(GameTime gameTime)
    {
        var now = DateTimeOffset.UtcNow;

        for (var i = FadingSongs.Count - 1; i >= 0; i--)
        {
            if (now <= FadingSongs[i].EndTime)
                continue;

            PlayingSongs.Remove(FadingSongs[i].Name);
            PlaybackEngine?.RemoveSample(FadingSongs[i].SampleProvider);

            FadingSongs.RemoveAt(i);
        }
    }
}

internal sealed class PlayingSong
{
    public string Name { get; }
    public NAudioSong Song { get; }
    public WaveStream WaveStream => Song.WaveStream;
    public ISampleProvider SampleProvider { get; }

    public bool IsPlaying => LoopStream.EnableLooping || WaveStream.Position < WaveStream.Length;

    private LoopStream LoopStream;
    private FadeInOutSampleProvider FadeInOutSampleProvider;

    public PlayingSong(string name, NAudioSong song, double fadeInMilliseconds = 0)
    {
        Name = name;
        Song = song;

        var initiallySilent = fadeInMilliseconds > 0;

        LoopStream = new LoopStream(WaveStream);

        var gainAdjusted = Math.Abs(Song.Gain - 1) < 0.001
            ? LoopStream.ToSampleProvider()
            : new VolumeSampleProvider(LoopStream.ToSampleProvider()) { Volume = Song.Gain };

        FadeInOutSampleProvider = new FadeInOutSampleProvider(gainAdjusted, initiallySilent);

        if(fadeInMilliseconds > 0)
            FadeInOutSampleProvider.BeginFadeIn(fadeInMilliseconds);

        SampleProvider = FadeInOutSampleProvider;
    }

    /// <summary>
    /// Sets whether the song should loop or not.
    /// </summary>
    /// <param name="loop"></param>
    public void Loop(bool loop)
    {
        LoopStream.EnableLooping = loop;
    }

    internal void BeginFadeOut(double durationMs)
    {
        FadeInOutSampleProvider.BeginFadeOut(durationMs);
    }
}
