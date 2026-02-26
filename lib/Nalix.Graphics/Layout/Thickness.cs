// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace Nalix.Graphics.Layout;

/// <summary>
/// Represents thickness values for each side of a rectangle.
/// Commonly used for 9-slice scaling, margins, or padding.
/// </summary>
[System.Diagnostics.DebuggerDisplay("Thickness (L={Left}, T={Top}, R={Right}, B={Bottom})")]
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public readonly struct Thickness(System.Int32 left, System.Int32 top, System.Int32 right, System.Int32 bottom) : System.IEquatable<Thickness>
{
    #region Properties

    /// <summary>
    /// Gets the thickness of the left side.
    /// </summary>
    public System.Int32 Left { get; } = left;

    /// <summary>
    /// Gets the thickness of the top side.
    /// </summary>
    public System.Int32 Top { get; } = top;

    /// <summary>
    /// Gets the thickness of the right side.
    /// </summary>
    public System.Int32 Right { get; } = right;

    /// <summary>
    /// Gets the thickness of the bottom side.
    /// </summary>
    public System.Int32 Bottom { get; } = bottom;

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new <see cref="Thickness"/> with the same value for all sides.
    /// </summary>
    /// <param name="uniform">
    /// The thickness applied uniformly to left, top, right, and bottom.
    /// </param>
    public Thickness(System.Int32 uniform)
        : this(System.Math.Max(0, uniform), System.Math.Max(0, uniform),
               System.Math.Max(0, uniform), System.Math.Max(0, uniform))
    {
    }

    #endregion Constructor

    #region IEquatable Implementation

    /// <inheritdoc/>
    public System.Boolean Equals(Thickness other)
        => Left == other.Left
        && Top == other.Top
        && Right == other.Right
        && Bottom == other.Bottom;

    /// <inheritdoc/>
    public override System.Boolean Equals(System.Object obj) => obj is Thickness other && Equals(other);

    /// <inheritdoc/>
    public override System.Int32 GetHashCode() => System.HashCode.Combine(Left, Top, Right, Bottom);

    /// <inheritdoc/>
    public static System.Boolean operator ==(Thickness left, Thickness right) => left.Equals(right);

    /// <inheritdoc/>
    public static System.Boolean operator !=(Thickness left, Thickness right) => !left.Equals(right);

    #endregion IEquatable Implementation
}
