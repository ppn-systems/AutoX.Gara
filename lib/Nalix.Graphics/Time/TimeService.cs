// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Framework.Injection;
using SFML.System;

namespace Nalix.Graphics.Time;

/// <summary>
/// Provides centralized time measurement and accumulation for the rendering engine.
/// </summary>
/// <remarks>
/// This service is responsible for tracking frame-to-frame timing,
/// total elapsed time, and fixed-step timing used by deterministic systems
/// such as physics or simulations.
/// </remarks>
[System.Diagnostics.DebuggerDisplay("TotalTime={_totalTime}, FixedΔ={FixedDeltaTime}")]
public sealed class TimeService
{
    #region Constants

    /// <summary>
    /// Default target frame rate for fixed timestep updates.
    /// </summary>
    private const System.Single DEFAULT_TARGET_FPS = 60f;

    /// <summary>
    /// Maximum allowed delta time to prevent spiral of death.
    /// </summary>
    private const System.Single MAX_DELTA_TIME = 0.25f;

    #endregion Constants

    #region Fields

    private System.Single _totalTime;

    /// <summary>
    /// Internal clock used to measure elapsed real time between frames.
    /// </summary>
    private readonly Clock _clock = InstanceManager.Instance.GetOrCreateInstance<Clock>();

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the timing snapshot for the current frame.
    /// </summary>
    /// <remarks>
    /// The returned instance is updated once per frame during <see cref="Update"/>.
    /// Consumers should treat this data as read-only.
    /// </remarks>
    public TimeFrame Current { get; } = new();

    /// <summary>
    /// Gets the fixed time step, in seconds, used for deterministic update loops.
    /// </summary>
    /// <remarks>
    /// The default value corresponds to a 60 Hz update rate.
    /// </remarks>
    public System.Single FixedDeltaTime { get; } = 1f / DEFAULT_TARGET_FPS;

    #endregion Properties

    #region APIs

    /// <summary>
    /// Updates the timing data for the current frame.
    /// </summary>
    /// <remarks>
    /// This method should be called exactly once per frame.
    /// It clamps excessively large delta times to avoid instability
    /// caused by long frame stalls.
    /// </remarks>
    public void Update()
    {
        System.Single delta = _clock.Restart().AsSeconds();

        // Clamp delta time to avoid extreme spikes (e.g. breakpoint, window drag)
        if (delta > MAX_DELTA_TIME)
        {
            delta = MAX_DELTA_TIME;
        }

        _totalTime += delta;

        this.Current.DeltaTime = delta;
        this.Current.TotalTime = _totalTime;
        this.Current.FixedDeltaTime = FixedDeltaTime;
    }

    #endregion APIs
}
