using BenMakesGames.PlayPlayMini.Attributes.DI;
using BenMakesGames.PlayPlayMini.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BenMakesGames.PlayPlayMini.Services;

/// <summary>
/// Service for playing music and sound effects.
/// </summary>
[AutoRegister]
public sealed class SoundManager : IServiceLoadContent
{
    private ILogger<SoundManager> Logger { get; }

    private Game Game { get; set; } = null!;

    private ContentManager Content => Game.Content;

    public IDictionary<string, SoundEffect> SoundEffects { get; private set; } = new Dictionary<string, SoundEffect>();
    public IDictionary<string, Song> Songs { get; private set; } = new Dictionary<string, Song>();

    /// <summary>
    /// The volume level of sounds.
    /// </summary>
    public float SoundVolume { get; private set; } = 1.0f;

    /// <summary>
    /// The volume level of music.
    /// </summary>
    public float MusicVolume { get; private set; } = 1.0f;

    /// <inheritdoc />
    public bool FullyLoaded { get; private set; }

    public SoundManager(ILogger<SoundManager> logger)
    {
        Logger = logger;
    }

    internal void SetGame(Game game)
    {
        if (Game is not null)
            throw new ArgumentException("SetGame can only be called once!");

        Game = game;
    }

    /// <summary>
    /// Sets the volume level of sounds.
    /// </summary>
    /// <remarks>The volume level of sounds that are currently playing WILL NOT be adjusted.</remarks>
    /// <param name="volume"></param>
    public void SetSoundVolume(float volume)
    {
        SoundVolume = volume <= 0 ? 0 : volume;
    }

    /// <summary>
    /// Sets the volume level of music.
    /// </summary>
    /// <remarks>The volume level of songs that are currently playing will be adjusted.</remarks>
    /// <param name="volume"></param>
    public void SetMusicVolume(float volume)
    {
        MusicVolume = volume <= 0 ? 0 : volume;
        MediaPlayer.Volume = volume;
    }

    /// <summary>
    /// Plays a sound effect.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="volume"></param>
    /// <param name="pitch"></param>
    /// <param name="pan"></param>
    public void PlaySound(string name, float volume = 1.0f, float pitch = 0.0f, float pan = 0.0f)
    {
        if (!SoundEffects.TryGetValue(name, out var soundEffect))
        {
            Logger.LogWarning("Sound {Name} has not been loaded", name);
            return;
        }

        var v = volume * SoundVolume;

        if(v > 0)
            soundEffect.Play(v, pitch, pan);
    }

    public string? CurrentMusic()
    {
        if(MediaPlayer.State != MediaState.Playing || MediaPlayer.Queue.ActiveSong is not {} activeSong)
            return null;

        return Songs.Keys.FirstOrDefault(k => Songs[k] == activeSong);
    }

    public void PlayMusic(string name, bool repeat = false)
    {
        if (!Songs.TryGetValue(name, out var song))
        {
            Logger.LogWarning("Song {Name} has not been loaded", name);
            return;
        }

        if (MediaPlayer.Queue.ActiveSong == song && MediaPlayer.State == MediaState.Playing)
            return;

        MediaPlayer.Stop();

        while (MediaPlayer.State == MediaState.Playing)
            Thread.Yield();

        MediaPlayer.IsRepeating = repeat;
        MediaPlayer.Play(song);
    }

    /// <summary>
    /// Stops the currently playing song, if any.
    /// </summary>
    public void StopMusic()
    {
        MediaPlayer.Stop();
    }

    /// <inheritdoc />
    public void LoadContent(GameStateManager gsm)
    {
        SoundEffects = gsm.Assets.GetAll<SoundEffectMeta>().ToDictionary(m => m.Key, _ => (SoundEffect)null!);
        Songs = gsm.Assets.GetAll<SongMeta>().ToDictionary(m => m.Key, _ => (Song)null!);

        // load immediately
        foreach (var meta in gsm.Assets.GetAll<SoundEffectMeta>().Where(m => m.PreLoaded))
            LoadSoundEffect(meta);

        foreach (var meta in gsm.Assets.GetAll<SongMeta>().Where(s => s.PreLoaded))
            LoadSong(meta);

        // deferred
        Task.Run(() => LoadDeferredContent(gsm.Assets));
    }

    /// <inheritdoc />
    public void UnloadContent()
    {
    }

    private void LoadDeferredContent(AssetCollection assets)
    {
        foreach (var meta in assets.GetAll<SoundEffectMeta>().Where(m => !m.PreLoaded))
            LoadSoundEffect(meta);

        SoundEffects = SoundEffects.ToFrozenDictionary();

        foreach (var meta in assets.GetAll<SongMeta>().Where(s => !s.PreLoaded))
            LoadSong(meta);

        Songs = Songs.ToFrozenDictionary();

        FullyLoaded = true;
    }

    private void LoadSoundEffect(SoundEffectMeta soundEffect)
    {
        try
        {
            SoundEffects[soundEffect.Key] = Content.Load<SoundEffect>(soundEffect.Path);
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to load {Path}: {Message}", soundEffect.Path, e.Message);
        }
    }

    private void LoadSong(SongMeta song)
    {
        try
        {
            Songs[song.Key] = Content.Load<Song>(song.Path);
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to load {Path}: {Message}", song.Path, e.Message);
        }
    }
}