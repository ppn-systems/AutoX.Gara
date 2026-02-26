// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Graphics.Abstractions;
using Nalix.Graphics.Engine;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Graphics.Internal.Effects;

/// <summary>
/// Base class for full-screen overlay effects used in scene transitions.
/// (VN) Lớp nền cho các hiệu ứng overlay màn hình chuyển cảnh.
/// </summary>
internal abstract class ScreenOverlayBase : ITransitionDrawable
{
    /// <summary>
    /// Gets the full-screen overlay size based on current engine screen size.
    /// </summary>
    protected static Vector2f OverlaySize => new(GraphicsEngine.ScreenSize.X, GraphicsEngine.ScreenSize.Y);

    /// <summary>
    /// Gets the effect base color (alpha may be controlled dynamically).
    /// </summary>
    protected Color BaseColor { get; }

    /// <summary>
    /// Gets the overlay size (captured at construction; use OverlaySize if need to dynamically resize).
    /// </summary>
    protected readonly Vector2f Size;

    /// <summary>
    /// Initializes the overlay effect with a given color.
    /// </summary>
    /// <param name="color">Overlay base color.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    protected ScreenOverlayBase(Color color)
    {
        this.Size = OverlaySize;
        this.BaseColor = color;
    }

    /// <summary>
    /// Updates the overlay effect logic for the current frame.
    /// </summary>
    /// <param name="t">Progress of the transition phase [0..1].</param>
    /// <param name="closing">True if in closing (cover), false if opening (reveal).</param>
    public abstract void Update(System.Single t, System.Boolean closing);

    /// <summary>
    /// Gets the object that renders the current overlay.
    /// </summary>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public abstract IDrawable GetDrawable();
}
