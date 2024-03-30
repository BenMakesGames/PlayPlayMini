using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BenMakesGames.PlayPlayMini.Attributes.DI;
using BenMakesGames.PlayPlayMini.NAudio.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace BenMakesGames.PlayPlayMini.NAudio.Services;

/// <summary>
/// Service for playing music using NAudio.
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
[AutoRegister]
public sealed class NAudioMusicPlayer: IServiceLoadContent, IServiceUpdate, IDisposable
{
    private NAudioPlaybackEngine PlaybackEngine { get; }
    private ILogger<NAudioMusicPlayer> Logger { get; }

    public Dictionary<string, WaveStream> Songs { get; } = new();

    private Dictionary<string, FadeInOutSampleProvider> PlayingSongs { get; } = new();
    private Dictionary<string, DateTimeOffset> FadingSongs { get; } = new();

    public NAudioMusicPlayer(ILogger<NAudioMusicPlayer> logger)
    {
        Logger = logger;

        PlaybackEngine = new NAudioPlaybackEngine();
    }

    public void LoadContent(GameStateManager gsm)
    {
        var allSongs = gsm.Assets.GetAll<NAudioSongMeta>().ToList();

        foreach(var song in allSongs.Where(s => s.PreLoaded))
            LoadSong(song.Key, song.Path);

        Task.Run(() =>
        {
            foreach(var song in allSongs.Where(s => !s.PreLoaded))
                LoadSong(song.Key, song.Path);

            FullyLoaded = true;
        });
    }

    public void UnloadContent()
    {
        // unloading is done in Dispose()
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
            var stream = CreateWaveStream(filePath);

            Songs.Add(name, stream);
        }
        catch(Exception e)
        {
            Logger.LogWarning(e, "Failed to load song {Name} from {Path}", name, filePath);
        }
    }

    private static WaveStream CreateWaveStream(string filePath) => Path.GetExtension(filePath).ToLower() switch
    {
        ".ogg" => TryLoad("NAudio.Vorbis.VorbisWaveReader", filePath),
        ".flac" => TryLoad("NAudio.Flac.FlacReader", filePath),
        _ => new AudioFileReader(filePath),
    };

    private static WaveStream TryLoad(string typeName, string filePath)
    {
        var type = Type.GetType(typeName);

        if(type == null)
            throw new InvalidOperationException($"Could not find class of type {typeName}");

        var instance = Activator.CreateInstance(type, filePath);

        if (instance is not WaveStream waveStream)
            throw new InvalidOperationException($"Failed to instantiate class of type {typeName}");

        return waveStream;
    }

    /// <summary>
    /// Sets the master volume, affecting all songs.
    ///
    /// This affects songs which are fading in or out as you would intuitively expect: their volume is adjusted
    /// proportional to their current fade, and they will continue to fade in or out at the same rate.
    /// </summary>
    /// <param name="volume"></param>
    /// <returns></returns>
    public NAudioMusicPlayer SetVolume(float volume)
    {
        PlaybackEngine.SetVolume(volume);
        return this;
    }

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
    public NAudioMusicPlayer PlaySong(string name, int fadeInMilliseconds = 0)
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

    /// <summary>
    /// Stops all songs. If the fadeOutMilliseconds is greater than 0, all songs will fade out over that duration.
    ///
    /// If a song is already fading out, its fade-out time will NOT be changed. However, a fadeOutMilliseconds of 0 will
    /// immediately stop songs, regardless of fade-out time.
    /// </summary>
    /// <param name="fadeOutMilliseconds"></param>
    /// <returns></returns>
    public NAudioMusicPlayer StopAllSongs(int fadeOutMilliseconds = 0)
    {
        if(fadeOutMilliseconds <= 0)
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
    public NAudioMusicPlayer StopAllSongsExcept(string[] songsToContinue, int fadeOutMilliseconds = 0)
    {
        foreach(var song in PlayingSongs)
        {
            if(songsToContinue.Contains(song.Key) || FadingSongs.ContainsKey(song.Key))
                continue;

            song.Value.BeginFadeOut(fadeOutMilliseconds);
            FadingSongs.Add(song.Key, DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(fadeOutMilliseconds));
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
    public NAudioMusicPlayer StopAllSongsExcept(string name, int fadeOutMilliseconds = 0)
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
    public NAudioMusicPlayer StopSong(string name, int fadeOutMilliseconds = 0)
    {
        if (!PlayingSongs.TryGetValue(name, out var sample))
            return this;

        if (fadeOutMilliseconds <= 0)
        {
            PlaybackEngine.RemoveSample(sample);
            PlayingSongs.Remove(name);
            FadingSongs.Remove(name);
            return this;
        }

        if (FadingSongs.ContainsKey(name))
            return this;

        sample.BeginFadeOut(fadeOutMilliseconds);
        FadingSongs.Add(name, DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(fadeOutMilliseconds));

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
