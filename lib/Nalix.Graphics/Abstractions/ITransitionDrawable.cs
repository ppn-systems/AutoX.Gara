// Copyright (c) 2026 PPN Corporation. All rights reserved.

using SFML.Graphics;

namespace Nalix.Graphics.Abstractions;

/// <summary>
/// Represents an abstraction for drawing overlays for each transition effect.
/// </summary>
public interface ITransitionDrawable
{
    /// <summary>
    /// Gets the <see cref="IDrawable"/> instance to be rendered each frame.
    /// </summary>
    /// <returns>
    /// A <see cref="IDrawable"/> object representing the current overlay to render.
    /// </returns>
    IDrawable GetDrawable();

    /// <summary>
    /// Updates the shape based on the progress value (range [0..1]) and phase.
    /// </summary>
    /// <param name="progress01">A value between 0 and 1 indicating the transition progress.</param>
    /// <param name="closing">If true, the overlay is closing (covering); if false, it is opening (revealing).</param>
    void Update(System.Single progress01, System.Boolean closing);
}
