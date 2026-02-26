// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace Nalix.Graphics.Enums;

/// <summary>
/// Defines visual effects used when transitioning between scenes.
/// </summary>
public enum SceneTransitionEffect : System.Byte
{
    /// <summary>
    /// Fade in or fade out effect.
    /// </summary>
    Fade = 0,

    /// <summary>
    /// Horizontal wipe effect, covering and then revealing from left to right.
    /// </summary>
    WipeHorizontal = 1,

    /// <summary>
    /// Vertical wipe effect, covering and then revealing from top to bottom.
    /// </summary>
    WipeVertical = 2,

    /// <summary>
    /// Slide-in cover effect from the left, then slides out.
    /// </summary>
    SlideCoverLeft = 3,

    /// <summary>
    /// Slide-in cover effect from the right, then slides out.
    /// </summary>
    SlideCoverRight = 4,

    /// <summary>
    /// Zoom in effect from a smaller frame to fullscreen, then zooms out.
    /// </summary>
    ZoomIn = 5,

    /// <summary>
    /// Zoom out effect from a larger frame to zero, then zooms in again.
    /// </summary>
    ZoomOut = 6
}
