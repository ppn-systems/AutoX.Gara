// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace Nalix.Graphics.Enums;

/// <summary>
/// Represents the state of an Animator.
/// </summary>
public enum AnimationState : System.Byte
{
    /// <summary>
    /// The animation is idle and not playing.
    /// </summary>
    Idle = 0,

    /// <summary>
    /// The animation is currently playing.
    /// </summary>
    Playing = 1,

    /// <summary>
    /// The animation is paused.
    /// </summary>
    Paused = 2,

    /// <summary>
    /// The animation is stopped.
    /// </summary>
    Stopped = 3
}
