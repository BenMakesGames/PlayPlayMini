# What Is It?

An extension for [PlayPlayMini](https://github.com/BenMakesGames/PlayPlayMini) which adds methods for generating wave forms on the fly, with support for attack and decay.

This library is in the early stages of development, and has some known issues (for example, it refuses to play notes after a while, for reasons which are still unclear to me). It is not suitable for use in a final product.

[![Buy Me a Coffee at ko-fi.com](https://raw.githubusercontent.com/BenMakesGames/AssetsForNuGet/main/buymeacoffee.png)](https://ko-fi.com/A0A12KQ16)

# How to Use

Here is an example PlayPlayMini game state which lets users play notes by pressing keys on their keyboard:

```
using System.Diagnostics;
using BenMakesGames.PlayPlayMini;
using BenMakesGames.PlayPlayMini.BeepBoop;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BeepBoopTest.GameStates;

public sealed class Playing: GameState
{
    private KeyboardManager Keyboard { get; }
    private SoundManager Sounds { get; }
    private GraphicsManager Graphics { get; }

    private WaveType WaveType { get; set; } = WaveType.Square;

    private List<char> NoteHistory { get; } = new();

    public Playing(KeyboardManager keyboard, SoundManager sounds, GraphicsManager graphics)
    {
        Keyboard = keyboard;
        Sounds = sounds;
        Graphics = graphics;
    }

    public override void AlwaysDraw(GameTime gameTime)
    {
        Graphics.Clear(Color.Black);

        // assumes you have loaded a font called "Font"; if not, comment these lines out
        Graphics.DrawText("Font", 2, 2, "Press A, S, D, F, G, H, J, K, L to play notes.", Color.White);
        Graphics.DrawText("Font", 2, 12, "Press 1, 2, 3, 4 to select wave shape.", Color.White);
        Graphics.DrawText("Font", 2, 22, $"Current wave shape: {WaveType}.", Color.White);
        
        for(int i = 0; i < NoteHistory.Count; i++)
            Graphics.DrawText("Font", 2 + (i * 6), 42, NoteHistory[i], i == NoteHistory.Count - 1 ? Color.White : Color.SkyBlue);
    }

    public override void ActiveInput(GameTime gameTime)
    {
        if(Keyboard.PressedKey(Keys.D1))
            WaveType = WaveType.Square;
        else if(Keyboard.PressedKey(Keys.D2))
            WaveType = WaveType.Sawtooth;
        else if(Keyboard.PressedKey(Keys.D3))
            WaveType = WaveType.Triangle;
        else if(Keyboard.PressedKey(Keys.D4))
            WaveType = WaveType.Sine;
        
        if(Keyboard.PressedKey(Keys.A)) PlaySound(Note.C);
        if(Keyboard.PressedKey(Keys.S)) PlaySound(Note.D);
        if(Keyboard.PressedKey(Keys.D)) PlaySound(Note.E);
        if(Keyboard.PressedKey(Keys.F)) PlaySound(Note.F);
        if(Keyboard.PressedKey(Keys.G)) PlaySound(Note.G);
        if(Keyboard.PressedKey(Keys.H)) PlaySound(Note.A);
        if(Keyboard.PressedKey(Keys.J)) PlaySound(Note.B);
        if(Keyboard.PressedKey(Keys.K)) PlaySound(Note.C, 5);
    }
    
    private void PlaySound(Note note, int octave = 4)
    {
        NoteHistory.Add(note.ToString()[0]);

        if(NoteHistory.Count > 40)
            NoteHistory.RemoveAt(0);
        
        var options = new WaveOptions()
        {
            Duration = 0.5f,
            AttackDuration = 0.1f,
            DecayDuration = 0.1f,
        };
        
        var sound = WaveType switch
        {
            WaveType.Square => WaveGenerator.Square(note, octave, options),
            WaveType.Sawtooth => WaveGenerator.Sawtooth(note, octave, options),
            WaveType.Triangle => WaveGenerator.Triangle(note, octave, options),
            WaveType.Sine => WaveGenerator.Sine(note, octave, options),
            _ => throw new UnreachableException()
        };
        
        Sounds.PlayWave(sound);
    }
}

enum WaveType
{
    Square,
    Sawtooth,
    Triangle,
    Sine
}
```
