using BenMakesGames.PlayPlayMini.Attributes.DI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace BenMakesGames.PlayPlayMini.Services;

[AutoRegister]
public sealed class KeyboardManager : IServiceInput
{
    private KeyboardState PreviousKeyboardState { get; set; }
    private KeyboardState KeyboardState { get; set; }

    public KeyboardManager()
    {
        PreviousKeyboardState = Keyboard.GetState();
        KeyboardState = Keyboard.GetState();
    }

    /// <inheritdoc />
    public void Input(GameTime gameTime)
    {
        PreviousKeyboardState = KeyboardState;
        KeyboardState = Keyboard.GetState();
    }

    public bool AnyKeyDown() => KeyboardState.GetPressedKeyCount() > 0;
    public bool PressedAnyKey() => AnyKeyDown() && PreviousKeyboardState.GetPressedKeyCount() == 0;

    public bool PressedKey(Keys key) => KeyboardState.IsKeyDown(key) && PreviousKeyboardState.IsKeyUp(key);
    public bool PressedAnyKey(IList<Keys> keys) => keys.Any(PressedKey);
    public bool KeyDown(Keys key) => KeyboardState.IsKeyDown(key);
    public bool AnyKeyDown(IList<Keys> keys) => keys.Any(KeyDown);
    public bool KeyUp(Keys key) => KeyboardState.IsKeyUp(key);
    public bool AnyKeyUp(IList<Keys> keys) => keys.Any(KeyUp);
}
