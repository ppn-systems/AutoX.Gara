// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace Nalix.Graphics.UI.DataGrid;

/// <summary>
/// Represents a column definition for the DataGrid.
/// </summary>
public sealed class DataGridColumn<T>(System.String header, System.Single width, System.Func<T, System.String> selector)
{
    /// <summary>
    /// Width of the column in pixels.
    /// </summary>
    public System.Single Width { get; set; } = width;

    /// <summary>
    /// Column header text.
    /// </summary>
    public System.String Header { get; } = header ?? "";

    /// <summary>
    /// Optional comparer for sorting by this column.
    /// If null, string comparison of CellSelector(T) will be used.
    /// </summary>
    public System.Collections.Generic.IComparer<T> Comparer { get; set; }

    /// <summary>
    /// Function which selects a string representation for a cell given an item.
    /// </summary>
    public System.Func<T, System.String> CellSelector { get; } = selector ?? throw new System.ArgumentNullException(nameof(selector));
}
