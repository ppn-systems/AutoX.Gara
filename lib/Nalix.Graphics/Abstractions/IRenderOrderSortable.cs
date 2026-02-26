// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace Nalix.Graphics.Abstractions;

/// <summary>
/// Provides rendering order hints for 2D drawables.
/// Implementations supply a coarse Z-index (layer) and a per-frame foot Y
/// used for dynamic ordering inside the same Z-index.
/// </summary>
public interface IRenderOrderSortable
{
    /// <summary>
    /// Gets the Z-index used for coarse layer ordering.
    /// Lower values are drawn earlier (appear behind).
    /// </summary>
    System.Int32 ZIndex { get; }

    /// <summary>
    /// Gets the "foot Y" used to order objects within the same Z-index.
    /// Lower values are drawn earlier (appear behind).
    /// </summary>
    System.Single GetFootY();
}
