// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Framework.Injection.DI;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Nalix.Graphics.Input;

/// <summary>
/// Manages mouse state and input.
/// </summary>
[System.Diagnostics.DebuggerDisplay(
    "MouseManager | Pos=({_mousePosition.X},{_mousePosition.Y}), ButtonsDown={GetPressedButtonCount()}")]
public class MouseManager : SingletonBase<MouseManager>
{
    #region Fields

    private Vector2i _mousePosition;
    private readonly Mouse.Button[] AllButtons;
    private readonly System.Boolean[] MouseButtonState;
    private readonly System.Boolean[] PreviousMouseButtonState;

    #endregion Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="MouseManager"/> class,
    /// configuring button state and default input enabled state.
    /// </summary>
    public MouseManager()
    {
        AllButtons = System.Enum.GetValues<Mouse.Button>();
        MouseButtonState = new System.Boolean[(System.Int32)Mouse.ButtonCount];
        PreviousMouseButtonState = new System.Boolean[(System.Int32)Mouse.ButtonCount];
    }

    #endregion Constructor

    #region Input Control

    /// <summary>
    /// Updates mouse button state and position based on the current frame.
    /// </summary>
    /// <param name="window">The render window reference (for relative mouse position).</param>
    public void Update(RenderWindow window)
    {
        for (System.Int32 i = 0; i < AllButtons.Length; i++)
        {
            System.Int32 idx = (System.Int32)AllButtons[i];

            if (AllButtons[i] == (Mouse.Button)Mouse.ButtonCount)
            {
                continue;
            }

            PreviousMouseButtonState[idx] = MouseButtonState[idx];
            MouseButtonState[idx] = Mouse.IsButtonPressed(AllButtons[i]);
        }

        _mousePosition = Mouse.GetPosition(window);
    }

    #endregion Input Control

    #region Getter Methods

    /// <summary>
    /// Gets the current position of the mouse, in window coordinates.
    /// </summary>
    /// <returns>A <see cref="Vector2i"/> containing the X and Y position of the mouse.</returns>
    public Vector2i GetMousePosition() => _mousePosition;

    /// <summary>
    /// Gets the current position of the mouse as a floating point tuple.
    /// </summary>
    /// <returns>A tuple containing the X and Y position of the mouse as floats.</returns>
    public (System.Single X, System.Single Y) GetMousePositionF() => (_mousePosition.X, _mousePosition.Y);

    #endregion Getter Methods

    #region Mouse Button State Methods

    /// <summary>
    /// Determines whether the specified mouse button is currently being held down.
    /// </summary>
    /// <param name="button">The mouse button to check (e.g., Left, Right).</param>
    /// <returns>True if the mouse button is currently down; otherwise, false.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean IsMouseButtonDown(Mouse.Button button) => MouseButtonState[(System.Int32)button];

    /// <summary>
    /// Determines whether the specified mouse button was just pressed during this frame.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>
    /// True if the button is down now but was not down in the previous frame; otherwise, false.
    /// </returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean IsMouseButtonPressed(Mouse.Button button) => MouseButtonState[(System.Int32)button] && !PreviousMouseButtonState[(System.Int32)button];

    /// <summary>
    /// Determines whether the specified mouse button was just released during this frame.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>
    /// True if the button was down in the previous frame but is not down now; otherwise, false.
    /// </returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Boolean IsMouseButtonReleased(Mouse.Button button) => !MouseButtonState[(System.Int32)button] && PreviousMouseButtonState[(System.Int32)button];

    #endregion Mouse Button State Methods
}
