// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Framework.Injection.DI;
using SFML.Window;

namespace Nalix.Graphics.Input;

/// <summary>
/// Manages keyboard state and input.
/// </summary>
[System.Diagnostics.DebuggerDisplay(
    "KeyboardManager | PressedKeys={GetPressedKeyCount()}")]
public class KeyboardManager : SingletonBase<KeyboardManager>
{
    #region Fields

    private readonly Keyboard.Key[] AllKeys;
    private readonly System.Boolean[] KeyState;
    private readonly System.Boolean[] PreviousKeyState;

    #endregion Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyboardManager"/> class,
    /// configuring all internal key state arrays.
    /// </summary>
    public KeyboardManager()
    {
        AllKeys = System.Enum.GetValues<Keyboard.Key>();
        KeyState = new System.Boolean[(System.Int32)Keyboard.KeyCount];
        PreviousKeyState = new System.Boolean[(System.Int32)Keyboard.KeyCount];
    }

    #endregion Constructor

    #region Input Control

    /// <summary>
    /// Updates the internal keyboard state for all keys.
    /// </summary>
    public void Update()
    {
        for (System.Int32 i = 0; i < AllKeys.Length; i++)
        {
            System.Int32 idx = (System.Int32)AllKeys[i];

            if (idx < 0 || idx >= KeyState.Length)
            {
                continue;
            }

            PreviousKeyState[idx] = KeyState[idx];
            KeyState[idx] = Keyboard.IsKeyPressed(AllKeys[i]);
        }
    }

    #endregion Input Control

    #region Keyboard

    /// <summary>
    /// Checks if a key is currently being pressed.
    /// </summary>
    /// <param name="key">The keyboard key to check.</param>
    /// <returns>True if the key is currently down; otherwise, false.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean IsKeyDown(Keyboard.Key key) => KeyState[(System.Int32)key];

    /// <summary>
    /// Checks if a key is currently not being pressed.
    /// </summary>
    /// <param name="key">The keyboard key to check.</param>
    /// <returns>True if the key is currently up; otherwise, false.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean IsKeyUp(Keyboard.Key key) => !KeyState[(System.Int32)key];

    /// <summary>
    /// Checks if a key was pressed for the first time in the current frame.
    /// </summary>
    /// <param name="key">The keyboard key to check.</param>
    /// <returns>True if the key was pressed this frame; otherwise, false.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean IsKeyPressed(Keyboard.Key key) => KeyState[(System.Int32)key] && !PreviousKeyState[(System.Int32)key];

    /// <summary>
    /// Checks if a key was released for the first time in the current frame.
    /// </summary>
    /// <param name="key">The keyboard key to check.</param>
    /// <returns>True if the key was released this frame; otherwise, false.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean IsKeyReleased(Keyboard.Key key) => !KeyState[(System.Int32)key] && PreviousKeyState[(System.Int32)key];

    #endregion Keyboard
}
