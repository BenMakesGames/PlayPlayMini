using BenMakesGames.PlayPlayMini.Attributes.DI;
using BenMakesGames.PlayPlayMini.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BenMakesGames.PlayPlayMini.Services;

[AutoRegister]
public sealed class SoundManager : IServiceLoadContent
{
    private ILogger<SoundManager> Logger { get; }

    private Game Game { get; set; } = null!;

    private ContentManager Content => Game.Content;

    public Dictionary<string, SoundEffect> SoundEffects { get; } = new();
    public Dictionary<string, Song> Songs { get; } = new();

    public float SoundVolume { get; private set; } = 1.0f;
    public float MusicVolume { get; private set; } = 1.0f;

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
    /// 
    /// </summary>
    /// <remarks>The volume level of sounds that are currently playing WILL NOT be adjusted.</remarks>
    /// <param name="volume"></param>
    public void SetSoundVolume(float volume)
    {
        SoundVolume = volume <= 0 ? 0 : volume;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>The volume level of songs that are currently playing will be adjusted.</remarks>
    /// <param name="volume"></param>
    public void SetMusicVolume(float volume)
    {
        MusicVolume = volume <= 0 ? 0 : volume;
        MediaPlayer.Volume = volume;
    }

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

    public void StopMusic()
    {
        MediaPlayer.Stop();
    }

    public void LoadContent(GameStateManager gsm)
    {
        SoundEffects.Clear();
        Songs.Clear();

        // load immediately
        foreach (var meta in gsm.Assets.GetAll<SoundEffectMeta>().Where(m => m.PreLoaded))
            LoadSoundEffect(meta);

        foreach (var meta in gsm.Assets.GetAll<SongMeta>().Where(s => s.PreLoaded))
            LoadSong(meta);

        // deferred
        Task.Run(() => LoadDeferredContent(gsm.Assets));
    }

    public void UnloadContent()
    {
    }

    private void LoadDeferredContent(AssetCollection assets)
    {
        foreach (var meta in assets.GetAll<SoundEffectMeta>().Where(m => !m.PreLoaded))
            LoadSoundEffect(meta);

        foreach (var meta in assets.GetAll<SongMeta>().Where(s => !s.PreLoaded))
            LoadSong(meta);

        FullyLoaded = true;
    }

    private void LoadSoundEffect(SoundEffectMeta soundEffect)
    {
        try
        {
            SoundEffects.Add(soundEffect.Key, Content.Load<SoundEffect>(soundEffect.Path));
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
            Songs.Add(song.Key, Content.Load<Song>(song.Path));
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to load {Path}: {Message}", song.Path, e.Message);
        }
    }
}
