using BenMakesGames.PlayPlayMini.Attributes.DI;

namespace BenMakesGames.PlayPlayMini.Services;

[AutoRegister]
public sealed class KeyboardManager : IServiceInput
{
    // TODO: restore this
    /*
    private KeyboardState PreviousKeyboardState { get; set; }
    private KeyboardState KeyboardState { get; set; }
    */

    public KeyboardManager()
    {
        // TODO: restore this
        /*
        PreviousKeyboardState = Keyboard.GetState();
        KeyboardState = Keyboard.GetState();
        */
    }

    public void Input(GameTime gameTime)
    {
        // TODO: restore this
        /*
        PreviousKeyboardState = KeyboardState;
        KeyboardState = Keyboard.GetState();
        */
    }

    // TODO: restore this
    /*
    public bool AnyKeyDown() => KeyboardState.GetPressedKeyCount() > 0;
    public bool PressedAnyKey() => AnyKeyDown() && PreviousKeyboardState.GetPressedKeyCount() == 0;

    public bool PressedKey(Keys key) => KeyboardState.IsKeyDown(key) && PreviousKeyboardState.IsKeyUp(key);
    public bool PressedAnyKey(IList<Keys> keys) => keys.Any(PressedKey);
    public bool KeyDown(Keys key) => KeyboardState.IsKeyDown(key);
    public bool AnyKeyDown(IList<Keys> keys) => keys.Any(KeyDown);
    public bool KeyUp(Keys key) => KeyboardState.IsKeyUp(key);
    public bool AnyKeyUp(IList<Keys> keys) => keys.Any(KeyUp);
    */
}
