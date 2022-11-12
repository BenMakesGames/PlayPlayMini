using BenMakesGames.PlayPlayMini.Attributes.DI;
using BenMakesGames.PlayPlayMini.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BenMakesGames.PlayPlayMini.Services;

[AutoRegister(Lifetime.Singleton)]
public sealed class SoundManager : IServiceLoadContent
{
    private ILogger<SoundManager> Logger { get; }

    private Game Game { get; set; } = null!;

    private ContentManager Content => Game.Content;

    public Dictionary<string, SoundEffect> SoundEffects = new();
    public Dictionary<string, Song> Songs = new();

    public float SoundVolume { get; private set; } = 1.0f;
    public float MusicVolume { get; private set; } = 1.0f;

    public bool FullyLoaded { get; private set; }

    public SoundManager(ILogger<SoundManager> logger)
    {
        Logger = logger;
    }
    
    public void SetGame(Game game)
    {
        if (Game != null)
            throw new ArgumentException("SetGame can only be called once!");

        Game = game;
    }

    public void SetSoundVolume(float volume)
    {
        SoundVolume = volume <= 0 ? 0 : volume;
    }

    public void SetMusicVolume(float volume)
    {
        MusicVolume = volume <= 0 ? 0 : volume;
        MediaPlayer.Volume = volume;
    }

    public void PlaySound(string name, float volume = 1.0f, float pitch = 0.0f, float pan = 0.0f)
    {
        if (!SoundEffects.ContainsKey(name))
        {
            Logger.LogWarning("Sound {Name} has not been loaded", name);
            return;
        }
            
        var v = volume * SoundVolume;

        if(v > 0)
            SoundEffects[name].Play(v, pitch, pan);
    }

    public void PlayMusic(string name, bool repeat = false)
    {
        if (!Songs.ContainsKey(name))
        {
            Logger.LogWarning("Song {Name} has not been loaded", name);
            return;
        }
            
        if (MediaPlayer.Queue.ActiveSong == Songs[name])
            return;

        MediaPlayer.Stop();
            
        while (MediaPlayer.State == MediaState.Playing)
            Thread.Yield();
            
        MediaPlayer.IsRepeating = repeat;
        MediaPlayer.Play(Songs[name]);
    }

    public void StopMusic()
    {
        MediaPlayer.Stop();
    }

    public void LoadContent(GameStateManager gsm)
    {
        // TODO: this is dumb and bad (not extensible), and must be fixed/replaced:
        var soundEffects = gsm.SoundEffects;
        var songs = gsm.Songs;

        SoundEffects = new();
        Songs = new();

        // load immediately
        foreach (var meta in soundEffects.Where(m => m.PreLoaded))
            LoadSoundEffect(meta);

        // deferred
        Task.Run(() => {
            LoadDeferredContent(soundEffects, songs);
        });
    }

    private void LoadDeferredContent(
        List<SoundEffectMeta> soundEffects,
        List<SongMeta> songs
    )
    {
        foreach (var meta in soundEffects.Where(m => !m.PreLoaded))
            LoadSoundEffect(meta);

        foreach (var meta in songs)
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
            Logger.LogWarning("Failed to load {Path}: {Message}", soundEffect.Path, e.Message);
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
            Logger.LogWarning("Failed to load {Path}: {Message}", song.Path, e.Message);
        }
    }
}