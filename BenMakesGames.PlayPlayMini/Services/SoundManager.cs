using BenMakesGames.PlayPlayMini.Attributes.DI;
using BenMakesGames.PlayPlayMini.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BenMakesGames.PlayPlayMini.Services
{
    [AutoRegister(Lifetime.Singleton)]
    public class SoundManager : IServiceLoadContent
    {
        private Game Game { get; set; } = null!;

        private ContentManager Content { get => Game.Content; }

        public Dictionary<string, SoundEffect> SoundEffects = new();
        public Dictionary<string, Song> Songs = new();

        public float SoundVolume { get; private set; } = 1.0f;
        public float MusicVolume { get; private set; } = 1.0f;

        public bool FullyLoaded { get; private set; } = false;

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
            float v = volume * SoundVolume;

            if(v > 0)
                SoundEffects[name].Play(v, pitch, pan);
        }

        public void PlayMusic(string name, bool repeat = false)
        {
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

            // load immediately
            SoundEffects = soundEffects
                .Where(m => m.PreLoaded)
                .ToDictionary(
                    meta => meta.Key,
                    meta => Content.Load<SoundEffect>(meta.Path)
                )
            ;

            Songs = new Dictionary<string, Song>();

            // deferred
            Task.Run(() => {
                try
                {
                    LoadDeferredContent(soundEffects, songs);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });
        }

        private void LoadDeferredContent(
            List<SoundEffectMeta> soundEffects,
            List<SongMeta> songs
        )
        {
            foreach (var meta in soundEffects.Where(m => !m.PreLoaded))
            {
                SoundEffects.Add(meta.Key, Content.Load<SoundEffect>(meta.Path));
            }

            foreach (var meta in songs)
            {
                Songs.Add(meta.Key, Content.Load<Song>(meta.Path));
            }

            FullyLoaded = true;
        }
    }
}
