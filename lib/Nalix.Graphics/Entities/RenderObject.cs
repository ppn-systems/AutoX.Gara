// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Graphics.Abstractions;
using SFML.Graphics;

namespace Nalix.Graphics.Entities;

/// <summary>
/// Represents an abstract base class for objects that can be rendered on a target.
/// Manages visibility, Z-Index ordering, and provides a method for rendering.
/// </summary>
[System.Diagnostics.DebuggerDisplay(
    "RenderObject | Z={_zIndex}, Visible={IsVisible}, Enabled={IsEnabled}")]
public abstract class RenderObject : SceneObject, IRenderable, System.IComparable<RenderObject>
{
    #region Fields

    private System.Int32 _zIndex;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Indicates the Z-Index of the object for rendering order.
    /// </summary>
    public System.Int32 ZIndex => _zIndex;

    /// <summary>
    /// Gets or sets whether the object is visible.
    /// </summary>
    public System.Boolean IsVisible { get; private set; } = true;

    #endregion Properties

    #region Public Methods

    /// <summary>
    /// Gets the drawable object to be rendered.
    /// Derived classes must implement this method to provide their specific drawable.
    /// </summary>
    /// <returns>ScreenSize <see cref="IDrawable"/> object to be rendered.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected abstract IDrawable GetDrawable();

    /// <summary>
    /// Renders the object on the specified render target if it is visible.
    /// </summary>
    /// <param name="target">The render target where the object will be drawn.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public virtual void Draw(IRenderTarget target)
    {
        if (this.IsVisible)
        {
            target.Draw(this.GetDrawable());
        }
    }

    /// <summary>
    /// Makes the object visible.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void Show() => this.IsVisible = true;

    /// <summary>
    /// Hides the object, making it not visible.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void Hide() => this.IsVisible = false;

    /// <summary>
    /// Sets the Z-Index of the object for rendering order.
    /// Lower values are rendered first.
    /// </summary>
    /// <param name="index">The Z-Index value.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void SetZIndex(System.Int32 index) => _zIndex = index;

    /// <inheritdoc/>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Int32 CompareTo(RenderObject other) => ReferenceEquals(this, other) ? 0 : other is null ? 1 : _zIndex.CompareTo(other._zIndex);

    /// <summary>
    /// Compares two <see cref="RenderObject"/> instances based on their Z-Index.
    /// </summary>
    /// <param name="r1">The first render object.</param>
    /// <param name="r2">The second render object.</param>
    /// <returns>
    /// An integer that indicates the relative order of the objects:
    /// - Negative if r1 is less than r2,
    /// - Zero if r1 equals r2,
    /// - Positive if r1 is greater than r2.
    /// </returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public static System.Int32 CompareZIndex(RenderObject r1, RenderObject r2) => ReferenceEquals(r1, r2) ? 0 : r1 is null ? -1 : r2 is null ? 1 : r1.CompareTo(r2);

    #endregion Public Methods
}
