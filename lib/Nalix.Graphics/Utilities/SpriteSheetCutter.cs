// Copyright (c) 2026 PPN Corporation. All rights reserved.

using SFML.Graphics;
using SFML.System;

namespace Nalix.Graphics.Utilities;

/// <summary>
/// Provides static utility methods for cutting sprite sheets into individual frames.
/// </summary>
/// <remarks>
/// Supports uniform grid-based sprite sheets with configurable cell size, spacing, and margin.
/// All methods are stateless and thread-safe.
/// </remarks>
public static class SpriteSheetCutter
{
    /// <summary>
    /// Cuts a uniform grid sprite sheet into a list of texture rectangles.
    /// </summary>
    /// <param name="cellWidth">The width of each sprite cell in pixels.</param>
    /// <param name="cellHeight">The height of each sprite cell in pixels.</param>
    /// <param name="columns">The number of columns in the sprite sheet grid.</param>
    /// <param name="rows">The number of rows in the sprite sheet grid.</param>
    /// <param name="spacing">The spacing between cells in pixels (default: 0).</param>
    /// <param name="margin">The margin around the entire sprite sheet in pixels (default: 0).</param>
    /// <returns>A list of <see cref="IntRect"/> representing each frame's texture coordinates.</returns>
    /// <remarks>
    /// <para>
    /// Frames are returned in row-major order (left-to-right, top-to-bottom).
    /// </para>
    /// <para>
    /// Example: A 4x4 grid with 16x16 cells returns 16 rectangles.
    /// </para>
    /// </remarks>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// Thrown if <paramref name="cellWidth"/>, <paramref name="cellHeight"/>, 
    /// <paramref name="columns"/>, or <paramref name="rows"/> is less than or equal to zero.
    /// </exception>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static System.Collections.Generic.List<IntRect> CutGrid(
        System.Int32 cellWidth,
        System.Int32 cellHeight,
        System.Int32 columns,
        System.Int32 rows,
        System.Int32 spacing = 0,
        System.Int32 margin = 0)
    {
        if (cellWidth <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(cellWidth), "Cell width must be greater than zero.");
        }

        if (cellHeight <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(cellHeight), "Cell height must be greater than zero.");
        }

        if (columns <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(columns), "Columns must be greater than zero.");
        }

        if (rows <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(rows), "Rows must be greater than zero.");
        }

        System.Collections.Generic.List<IntRect> frames = new(columns * rows);

        for (System.Int32 row = 0; row < rows; row++)
        {
            for (System.Int32 col = 0; col < columns; col++)
            {
                System.Int32 x = margin + (col * (cellWidth + spacing));
                System.Int32 y = margin + (row * (cellHeight + spacing));

                frames.Add(new IntRect(new Vector2i(x, y), new Vector2i(cellWidth, cellHeight)));
            }
        }

        return frames;
    }

    /// <summary>
    /// Cuts a single row from a sprite sheet into a list of texture rectangles.
    /// </summary>
    /// <param name="rowIndex">The zero-based row index to extract.</param>
    /// <param name="cellWidth">The width of each sprite cell in pixels.</param>
    /// <param name="cellHeight">The height of each sprite cell in pixels.</param>
    /// <param name="columns">The number of columns in the sprite sheet grid.</param>
    /// <param name="spacing">The spacing between cells in pixels (default: 0).</param>
    /// <param name="margin">The margin around the entire sprite sheet in pixels (default: 0).</param>
    /// <returns>A list of <see cref="IntRect"/> representing the frames in the specified row.</returns>
    /// <remarks>
    /// Useful for extracting animation frames for a single direction or action.
    /// </remarks>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// Thrown if <paramref name="rowIndex"/> is negative, or if cell dimensions or columns are invalid.
    /// </exception>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static System.Collections.Generic.List<IntRect> CutRow(
        System.Int32 rowIndex,
        System.Int32 cellWidth,
        System.Int32 cellHeight,
        System.Int32 columns,
        System.Int32 spacing = 0,
        System.Int32 margin = 0)
    {
        if (rowIndex < 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(rowIndex), "Row index cannot be negative.");
        }

        if (cellWidth <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(cellWidth), "Cell width must be greater than zero.");
        }

        if (cellHeight <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(cellHeight), "Cell height must be greater than zero.");
        }

        if (columns <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(columns), "Columns must be greater than zero.");
        }

        System.Collections.Generic.List<IntRect> frames = new(columns);
        System.Int32 y = margin + (rowIndex * (cellHeight + spacing));

        for (System.Int32 col = 0; col < columns; col++)
        {
            System.Int32 x = margin + (col * (cellWidth + spacing));
            frames.Add(new IntRect(new Vector2i(x, y), new Vector2i(cellWidth, cellHeight)));
        }

        return frames;
    }

    /// <summary>
    /// Cuts a single column from a sprite sheet into a list of texture rectangles.
    /// </summary>
    /// <param name="columnIndex">The zero-based column index to extract.</param>
    /// <param name="cellWidth">The width of each sprite cell in pixels.</param>
    /// <param name="cellHeight">The height of each sprite cell in pixels.</param>
    /// <param name="rows">The number of rows in the sprite sheet grid.</param>
    /// <param name="spacing">The spacing between cells in pixels (default: 0).</param>
    /// <param name="margin">The margin around the entire sprite sheet in pixels (default: 0).</param>
    /// <returns>A list of <see cref="IntRect"/> representing the frames in the specified column.</returns>
    /// <remarks>
    /// Useful for extracting vertical animation sequences.
    /// </remarks>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// Thrown if <paramref name="columnIndex"/> is negative, or if cell dimensions or rows are invalid.
    /// </exception>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static System.Collections.Generic.List<IntRect> CutColumn(
        System.Int32 columnIndex,
        System.Int32 cellWidth,
        System.Int32 cellHeight,
        System.Int32 rows,
        System.Int32 spacing = 0,
        System.Int32 margin = 0)
    {
        if (columnIndex < 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(columnIndex), "Column index cannot be negative.");
        }

        if (cellWidth <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(cellWidth), "Cell width must be greater than zero.");
        }

        if (cellHeight <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(cellHeight), "Cell height must be greater than zero.");
        }

        if (rows <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(rows), "Rows must be greater than zero.");
        }

        System.Collections.Generic.List<IntRect> frames = new(rows);
        System.Int32 x = margin + (columnIndex * (cellWidth + spacing));

        for (System.Int32 row = 0; row < rows; row++)
        {
            System.Int32 y = margin + (row * (cellHeight + spacing));
            frames.Add(new IntRect(new Vector2i(x, y), new Vector2i(cellWidth, cellHeight)));
        }

        return frames;
    }

    /// <summary>
    /// Cuts a rectangular region from a sprite sheet into a list of texture rectangles.
    /// </summary>
    /// <param name="startColumn">The starting column index (inclusive).</param>
    /// <param name="startRow">The starting row index (inclusive).</param>
    /// <param name="columnCount">The number of columns to extract.</param>
    /// <param name="rowCount">The number of rows to extract.</param>
    /// <param name="cellWidth">The width of each sprite cell in pixels.</param>
    /// <param name="cellHeight">The height of each sprite cell in pixels.</param>
    /// <param name="spacing">The spacing between cells in pixels (default: 0).</param>
    /// <param name="margin">The margin around the entire sprite sheet in pixels (default: 0).</param>
    /// <returns>A list of <see cref="IntRect"/> representing the frames in the specified region.</returns>
    /// <remarks>
    /// Frames are returned in row-major order within the specified region.
    /// Useful for extracting a subset of a larger sprite sheet.
    /// </remarks>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// Thrown if any index is negative or if dimensions are invalid.
    /// </exception>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static System.Collections.Generic.List<IntRect> CutRegion(
        System.Int32 startColumn,
        System.Int32 startRow,
        System.Int32 columnCount,
        System.Int32 rowCount,
        System.Int32 cellWidth,
        System.Int32 cellHeight,
        System.Int32 spacing = 0,
        System.Int32 margin = 0)
    {
        if (startColumn < 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(startColumn), "Start column cannot be negative.");
        }

        if (startRow < 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(startRow), "Start row cannot be negative.");
        }

        if (columnCount <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(columnCount), "Column count must be greater than zero.");
        }

        if (rowCount <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(rowCount), "Row count must be greater than zero.");
        }

        if (cellWidth <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(cellWidth), "Cell width must be greater than zero.");
        }

        if (cellHeight <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(cellHeight), "Cell height must be greater than zero.");
        }

        System.Collections.Generic.List<IntRect> frames = new(columnCount * rowCount);

        for (System.Int32 row = 0; row < rowCount; row++)
        {
            for (System.Int32 col = 0; col < columnCount; col++)
            {
                System.Int32 x = margin + ((startColumn + col) * (cellWidth + spacing));
                System.Int32 y = margin + ((startRow + row) * (cellHeight + spacing));

                frames.Add(new IntRect(new Vector2i(x, y), new Vector2i(cellWidth, cellHeight)));
            }
        }

        return frames;
    }

    /// <summary>
    /// Cuts specific frames from a sprite sheet by their grid indices.
    /// </summary>
    /// <param name="indices">List of tuples (column, row) specifying which frames to extract.</param>
    /// <param name="cellWidth">The width of each sprite cell in pixels.</param>
    /// <param name="cellHeight">The height of each sprite cell in pixels.</param>
    /// <param name="spacing">The spacing between cells in pixels (default: 0).</param>
    /// <param name="margin">The margin around the entire sprite sheet in pixels (default: 0).</param>
    /// <returns>A list of <see cref="IntRect"/> representing the specified frames in order.</returns>
    /// <remarks>
    /// Allows extraction of non-contiguous frames in custom order.
    /// Useful for custom animation sequences or scattered frames.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown if <paramref name="indices"/> is null.
    /// </exception>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// Thrown if cell dimensions are invalid or if any index is negative.
    /// </exception>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static System.Collections.Generic.List<IntRect> CutByIndices(
        System.Collections.Generic.IReadOnlyList<(System.Int32 Column, System.Int32 Row)> indices,
        System.Int32 cellWidth,
        System.Int32 cellHeight,
        System.Int32 spacing = 0,
        System.Int32 margin = 0)
    {
        System.ArgumentNullException.ThrowIfNull(indices);

        if (cellWidth <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(cellWidth), "Cell width must be greater than zero.");
        }

        if (cellHeight <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(cellHeight), "Cell height must be greater than zero.");
        }

        System.Collections.Generic.List<IntRect> frames = new(indices.Count);

        foreach ((System.Int32 col, System.Int32 row) in indices)
        {
            if (col < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(indices), $"Column index {col} cannot be negative.");
            }

            if (row < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(indices), $"Row index {row} cannot be negative.");
            }

            System.Int32 x = margin + (col * (cellWidth + spacing));
            System.Int32 y = margin + (row * (cellHeight + spacing));

            frames.Add(new IntRect(new Vector2i(x, y), new Vector2i(cellWidth, cellHeight)));
        }

        return frames;
    }
}
