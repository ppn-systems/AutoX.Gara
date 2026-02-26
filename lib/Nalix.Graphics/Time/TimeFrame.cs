// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace Nalix.Graphics.Time;

/// <summary>
/// Represents an immutable snapshot of timing data for the current frame.
/// </summary>
/// <remarks>
/// This type is typically provided by the engine during an update cycle
/// and should be treated as read-only by consuming systems.
/// </remarks>
[System.Diagnostics.DebuggerDisplay("Δ={DeltaTime}, t={TotalTime}, fixedΔ={FixedDeltaTime}")]
public sealed class TimeFrame
{
    /// <summary>
    /// Gets the elapsed time, in seconds, since the previous frame update.
    /// </summary>
    /// <remarks>
    /// Commonly used for frame-rate–independent movement and animations.
    /// </remarks>
    public System.Single DeltaTime { get; internal set; }

    /// <summary>
    /// Gets the total elapsed time, in seconds, since the engine started.
    /// </summary>
    /// <remarks>
    /// This value increases monotonically and is not affected by pausing
    /// or time scaling unless explicitly handled by the engine.
    /// </remarks>
    public System.Single TotalTime { get; internal set; }

    /// <summary>
    /// Gets the fixed time step, in seconds, used for deterministic updates.
    /// </summary>
    /// <remarks>
    /// Typically applied in fixed-update loops such as physics simulation
    /// to ensure consistent and reproducible behavior.
    /// </remarks>
    public System.Single FixedDeltaTime { get; internal set; }
}
