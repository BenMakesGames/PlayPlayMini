using BenMakesGames.PlayPlayMini.Attributes.DI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace BenMakesGames.PlayPlayMini.Services;

/// <summary>
/// Service for getting keyboard input.
/// </summary>
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

    /// <summary>
    /// Returns true if any key is currently down.
    /// </summary>
    /// <returns></returns>
    public bool AnyKeyDown() => KeyboardState.GetPressedKeyCount() > 0;

    /// <summary>
    /// Returns true if any key was pressed this frame.
    /// </summary>
    /// <returns></returns>
    public bool PressedAnyKey() => AnyKeyDown() && PreviousKeyboardState.GetPressedKeyCount() == 0;

    /// <summary>
    /// Returns true if the specified key was pressed this frame.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool PressedKey(Keys key) => KeyboardState.IsKeyDown(key) && PreviousKeyboardState.IsKeyUp(key);

    /// <summary>
    /// Returns true if any of the specified keys were pressed this frame.
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    public bool PressedAnyKey(params ReadOnlySpan<Keys> keys)
    {
        foreach (var key in keys)
        {
            if (PressedKey(key)) return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true if any of the specified keys were pressed this frame.
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    public bool PressedAnyKey(params IList<Keys> keys)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        // ReSharper disable once ForCanBeConvertedToForeach
        for(var i = 0; i < keys.Count; i++)
        {
            if (PressedKey(keys[i])) return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true if the specified key is currently down.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool KeyDown(Keys key) => KeyboardState.IsKeyDown(key);

    /// <summary>
    /// Returns true if any of the specified keys are currently down.
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    public bool AnyKeyDown(params ReadOnlySpan<Keys> keys)
    {
        foreach (var key in keys)
        {
            if (KeyDown(key)) return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true if any of the specified keys are currently down.
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    public bool AnyKeyDown(params IList<Keys> keys)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < keys.Count; i++)
        {
            if (KeyDown(keys[i])) return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true if the specified key is currently up.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool KeyUp(Keys key) => KeyboardState.IsKeyUp(key);

    /// <summary>
    /// Returns true if any of the specified keys are currently up.
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    public bool AnyKeyUp(params ReadOnlySpan<Keys> keys)
    {
        foreach (var key in keys)
        {
            if (KeyUp(key)) return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true if any of the specified keys are currently up.
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    public bool AnyKeyUp(params IList<Keys> keys)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        // ReSharper disable once ForCanBeConvertedToForeach
        for(var i = 0; i < keys.Count; i++)
        {
            if (KeyUp(keys[i])) return true;
        }
        return false;
    }
}
