// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Graphics.Abstractions;
using Nalix.Graphics.Assets;
using Nalix.Graphics.Entities;
using Nalix.Graphics.Extensions;
using Nalix.Graphics.Input;
using Nalix.Graphics.UI.Controls;
using Nalix.Graphics.UI.Theme;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Linq;

namespace Nalix.Graphics.UI.DataGrid;

/// <summary>
/// A virtualized, multi-column data grid with sorting, filtering and multi-selection support.
/// This control performs minimal allocations per-frame by reusing SFML drawables for visible rows.
/// </summary>
public sealed class DataGrid<T> : RenderObject, IUpdatable
{
    #region Constants

    // Layout defaults
    private const System.Single DefaultRowHeight = 28f;
    private const System.UInt32 DefaultFontSize = 14;
    private const System.Int32 BufferRows = 2; // extra rows to render above/below for smoothness

    private const System.Single DefaultScrollBarWidth = 12f;
    private const System.Single MinThumbHeight = 20f;

    #endregion Constants

    #region Fields

    // Data source / view
    private System.Collections.Generic.IList<T> _items = System.Array.Empty<T>();
    private System.Collections.Generic.List<System.Int32> _viewIndices = []; // indices into _items after filtering/sorting
    private System.Func<T, System.Boolean> _filterPredicate;

    // Columns
    private readonly System.Collections.Generic.List<DataGridColumn<T>> _columns = [];

    // Rendering pool (virtualization)
    private readonly System.Collections.Generic.List<Text[]> _cellTextPool = []; // each entry: array of Texts for columns
    private readonly System.Collections.Generic.List<RectangleShape> _rowShapes = [];

    // Appearance
    private readonly Font _font;
    private System.Single _rowHeight = DefaultRowHeight;
    private readonly System.UInt32 _fontSize = DefaultFontSize;
    private Vector2f _position = new(0f, 0f);
    private Vector2f _size = new(400f, 200f);

    // Scrolling / virtualization
    private System.Single _scrollOffset = 0f; // in pixels from top of list
    private System.Int32 _firstVisibleIndex = 0;

    // Interaction / selection
    private System.Boolean _isHovered;
    private readonly System.Collections.Generic.HashSet<System.Int32> _selectedViewIndices = [];
    private System.Int32 _lastSelectedViewIndex = -1;

    // Sorting state
    private System.Int32 _sortColumnIndex = -1;
    private System.Boolean _sortAscending = true;

    // Events
    public event System.Action<T[]> SelectionChanged;
    public event System.Action<System.Int32, System.Boolean> RowClicked; // viewIndex, doubleClickFlag (future)

    // Input state
    private System.Boolean _lastFrameMouseDown = false;

    // ScrollBar instance (external control)
    private readonly ScrollBar _scrollBar;
    private System.Single _scrollBarWidth = DefaultScrollBarWidth;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Position of the DataGrid in screen coordinates.
    /// </summary>
    public Vector2f Position
    {
        get => _position;
        set
        {
            if (_position != value)
            {
                _position = value;
                UPDATE_SCROLL_BAR_GEOMETRY();
            }
        }
    }

    /// <summary>
    /// Size (width, height) of the DataGrid.
    /// </summary>
    public Vector2f Size
    {
        get => _size;
        set
        {
            if (_size != value)
            {
                _size = value;
                ENSURE_POOL_CAPACITY();
                UPDATE_SCROLL_BAR_GEOMETRY();
            }
        }
    }

    /// <summary>
    /// Height of each row in pixels.
    /// </summary>
    public System.Single RowHeight
    {
        get => _rowHeight;
        set
        {
            _rowHeight = System.MathF.Max(8f, value);
            ENSURE_POOL_CAPACITY();
            UPDATE_SCROLL_BAR_GEOMETRY();
        }
    }

    /// <summary>
    /// Width of the scrollbar in pixels.
    /// </summary>
    public System.Single ScrollBarWidth
    {
        get => _scrollBarWidth;
        set
        {
            _scrollBarWidth = System.MathF.Max(8f, value);
            ENSURE_POOL_CAPACITY();
            UPDATE_SCROLL_BAR_GEOMETRY();
        }
    }

    /// <summary>
    /// Vertical scroll offset in pixels.
    /// </summary>
    public System.Single ScrollOffset
    {
        get => _scrollOffset;
        private set
        {
            System.Single contentHeight = System.Math.Max(0, _viewIndices.Count) * _rowHeight;
            System.Single maxOffset = System.MathF.Max(0f, contentHeight - (_size.Y - _rowHeight)); // subtract header height
            _scrollOffset = System.Math.Clamp(value, 0f, maxOffset);

            // Sync scrollbar normalized value without raising looped event
            if (_scrollBar != null)
            {
                System.Single v = maxOffset <= 0f ? 0f : _scrollOffset / maxOffset;
                _scrollBar.SetValue(v, raiseEvent: false);
            }
        }
    }

    /// <summary>
    /// Gets current selected items (snapshot).
    /// </summary>
    public T[] SelectedItems => [.. _selectedViewIndices.Select(i => _items[_viewIndices[i]])];

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Creates a new DataGrid with an optional font.
    /// </summary>
    public DataGrid(Font font = null)
    {
        _font = font ?? EmbeddedAssets.JetBrainsMono.ToFont();

        _scrollBar = new ScrollBar()
        {
            MinThumbHeight = MinThumbHeight,
            IsVisibleForRender = true
        };

        // when scrollbar value changes, update scroll offset
        _scrollBar.ValueChanged += _ =>
        {
            System.Single contentHeight = System.Math.Max(1, _viewIndices.Count) * _rowHeight;
            System.Single viewport = System.Math.Max(1f, _size.Y - _rowHeight);
            System.Single offset = _scrollBar.ValueToOffset(contentHeight, viewport);
            // directly set backing field to avoid repeating SetValue -> event loop
            _scrollOffset = offset;
        };

        ENSURE_POOL_CAPACITY();
        UPDATE_SCROLL_BAR_GEOMETRY();
    }

    #endregion Constructor

    #region Data / Columns API

    /// <summary>
    /// Set the data source for the grid. Items are not copied; indices are used for virtualization.
    /// </summary>
    public void SetItems(System.Collections.Generic.IList<T> items)
    {
        _items = items ?? System.Array.Empty<T>();
        REBUILD_VIEW();
        ScrollOffset = 0f;
        UPDATE_SCROLL_BAR_GEOMETRY();
    }

    /// <summary>
    /// Adds a column to the grid.
    /// </summary>
    public void AddColumn(DataGridColumn<T> column)
    {
        System.ArgumentNullException.ThrowIfNull(column);

        _columns.Add(column);
        ENSURE_POOL_CAPACITY();
    }

    /// <summary>
    /// Clears columns.
    /// </summary>
    public void ClearColumns()
    {
        _columns.Clear();
        ENSURE_POOL_CAPACITY();
    }

    /// <summary>
    /// Applies a filter predicate. Pass null to remove filter.
    /// </summary>
    public void ApplyFilter(System.Func<T, System.Boolean> predicate)
    {
        _filterPredicate = predicate;
        REBUILD_VIEW();
        UPDATE_SCROLL_BAR_GEOMETRY();
    }

    /// <summary>
    /// Sorts by column index. Use -1 to clear sorting.
    /// </summary>
    public void SortBy(System.Int32 columnIndex, System.Boolean ascending = true)
    {
        if (columnIndex < -1 || columnIndex >= _columns.Count)
        {
            throw new System.ArgumentOutOfRangeException(nameof(columnIndex));
        }

        _sortColumnIndex = columnIndex;
        _sortAscending = ascending;
        APPLY_SORT();
    }

    #endregion

    #region Pool & Virtualization

    private void APPLY_SORT()
    {
        if (_sortColumnIndex < 0 || _sortColumnIndex >= _columns.Count)
        {
            // no sorting; keep current order (which is insertion order from RebuildView)
            return;
        }

        var col = _columns[_sortColumnIndex];
        // If a comparer for T provided, use it; else compare strings from selector.
        if (col.Comparer != null)
        {
            _viewIndices.Sort((a, b) =>
            {
                System.Int32 r = col.Comparer.Compare(_items[a], _items[b]);
                return _sortAscending ? r : -r;
            });
        }
        else
        {
            _viewIndices.Sort((a, b) =>
            {
                System.String sa = col.CellSelector(_items[a]) ?? "";
                System.String sb = col.CellSelector(_items[b]) ?? "";
                System.Int32 r = System.String.CompareOrdinal(sa, sb);
                return _sortAscending ? r : -r;
            });
        }

        UPDATE_SCROLL_BAR_GEOMETRY();
    }

    private void REBUILD_VIEW()
    {
        _viewIndices = [.. Enumerable.Range(0, _items.Count).Where(i => _filterPredicate is null || _filterPredicate(_items[i]))];
        APPLY_SORT(); // sort after filter
        _selectedViewIndices.Clear();
        _lastSelectedViewIndex = -1;
    }

    private void ENSURE_POOL_CAPACITY()
    {
        // Reserve width for scrollbar
        System.Single contentWidth = System.Math.Max(10f, _size.X - _scrollBarWidth);

        System.Int32 visibleRows = (System.Int32)System.MathF.Ceiling((_size.Y - _rowHeight) / _rowHeight) + (BufferRows * 2); // exclude header height
        visibleRows = System.Math.Max(1, visibleRows);

        // Resize pool to visibleRows
        while (_cellTextPool.Count < visibleRows)
        {
            var texts = new Text[System.Math.Max(1, _columns.Count)];
            for (System.Int32 c = 0; c < texts.Length; c++)
            {
                texts[c] = new Text(_font, "", _fontSize)
                {
                    FillColor = Themes.PrimaryTextColor,
                    Position = new Vector2f(0, 0)
                };
            }
            _cellTextPool.Add(texts);
            _rowShapes.Add(new RectangleShape(new Vector2f(contentWidth, _rowHeight))); // width patched later
        }

        // If columns changed, ensure each pooled array has required length
        for (System.Int32 i = 0; i < _cellTextPool.Count; i++)
        {
            if (_cellTextPool[i].Length != System.Math.Max(1, _columns.Count))
            {
                var old = _cellTextPool[i];
                var newArr = new Text[System.Math.Max(1, _columns.Count)];
                for (System.Int32 c = 0; c < newArr.Length; c++)
                {
                    newArr[c] = c < old.Length && old[c] != null ? old[c] : new Text(_font, "", _fontSize) { FillColor = Themes.PrimaryTextColor };
                }
                _cellTextPool[i] = newArr;
            }
            _rowShapes[i].Size = new Vector2f(System.Math.Max(10f, _size.X - _scrollBarWidth), _rowHeight);
        }

        UPDATE_SCROLL_BAR_GEOMETRY();
    }

    #endregion

    #region Interaction helpers

    /// <summary>
    /// Scroll the grid by a pixel delta (positive = down).
    /// Call this from the RenderWindow.MouseWheelScrolled event or other scroll sources.
    /// </summary>
    public void OnMouseWheel(System.Single delta)
    {
        // Typical mouse wheel delta is +/- 1 per tick; scale to row height for convenience
        ScrollOffset -= delta * _rowHeight * 3f;

        // update scrollbar normalized value
        System.Single contentHeight = System.Math.Max(1, _viewIndices.Count) * _rowHeight;
        System.Single viewport = System.Math.Max(1f, _size.Y - _rowHeight);
        System.Single maxOffset = System.MathF.Max(0f, contentHeight - viewport);
        System.Single v = maxOffset <= 0f ? 0f : _scrollOffset / maxOffset;
        _scrollBar.SetValue(v, raiseEvent: false);
    }

    private System.Int32 HIT_TEST_ROW_INDEX(Vector2i mousePos)
    {
        // mousePos is global window coordinates
        FloatRect bounds = new(_position, _size);
        if (!bounds.Contains(mousePos))
        {
            return -1;
        }

        System.Single localY = mousePos.Y - _position.Y - _rowHeight + ScrollOffset; // subtract header height
        System.Int32 viewIndex = (System.Int32)(localY / _rowHeight);
        return viewIndex < 0 || viewIndex >= _viewIndices.Count ? -1 : viewIndex;
    }

    private void TOGGLE_SELECTION(System.Int32 viewIndex, System.Boolean ctrl, System.Boolean shift)
    {
        if (viewIndex < 0)
        {
            return;
        }

        if (shift && _lastSelectedViewIndex >= 0)
        {
            System.Int32 a = System.Math.Min(_lastSelectedViewIndex, viewIndex);
            System.Int32 b = System.Math.Max(_lastSelectedViewIndex, viewIndex);
            for (System.Int32 i = a; i <= b; i++)
            {
                _selectedViewIndices.Add(i);
            }
        }
        else if (ctrl)
        {
            if (!_selectedViewIndices.Remove(viewIndex))
            {
                _selectedViewIndices.Add(viewIndex);
            }

            _lastSelectedViewIndex = viewIndex;
        }
        else
        {
            _selectedViewIndices.Clear();
            _selectedViewIndices.Add(viewIndex);
            _lastSelectedViewIndex = viewIndex;
        }

        SelectionChanged?.Invoke(SelectedItems);
    }

    #endregion

    #region Scrollbar helpers

    private void UPDATE_SCROLL_BAR_GEOMETRY()
    {
        // position scrollbar track to right, below header
        var trackPos = new Vector2f(_position.X + _size.X - _scrollBarWidth, _position.Y + _rowHeight);
        var trackSize = new Vector2f(_scrollBarWidth, System.Math.Max(0f, _size.Y - _rowHeight));
        _scrollBar.Position = trackPos;
        _scrollBar.Size = trackSize;

        // compute viewport ratio and set
        System.Single contentHeight = System.Math.Max(1, _viewIndices.Count) * _rowHeight;
        System.Single viewportHeight = System.Math.Max(1f, _size.Y - _rowHeight);
        System.Single ratio = System.Math.Clamp(viewportHeight / contentHeight, 0f, 1f);
        _scrollBar.SetViewportRatio(ratio);

        // ensure normalized value matches current offset
        System.Single maxOffset = System.MathF.Max(0f, contentHeight - viewportHeight);
        System.Single normalized = maxOffset <= 0f ? 0f : _scrollOffset / maxOffset;
        _scrollBar.SetValue(normalized, raiseEvent: false);

        // hide scrollbar if content fits
        _scrollBar.IsVisibleForRender = contentHeight > viewportHeight;
    }

    #endregion Scrollbar helpers

    #region Update / Draw

    /// <summary>
    /// Update handles mouse hover, click selection, scrollbar update, and virtualized range recalculation.
    /// </summary>
    public override void Update(System.Single dt)
    {
        // update scrollbar first (handles drag & track clicks)
        _scrollBar.Update(dt);

        // compute visible first index (based on scroll offset)
        _firstVisibleIndex = (System.Int32)System.MathF.Floor(_scrollOffset / _rowHeight);
        _firstVisibleIndex = System.Math.Clamp(_firstVisibleIndex - BufferRows, 0, System.Math.Max(0, _viewIndices.Count - 1));

        // input: mouse hovering + click
        Vector2i mpos = MouseManager.Instance.GetMousePosition();
        System.Boolean isDown = Mouse.IsButtonPressed(Mouse.Button.Left);
        System.Boolean isOver = new FloatRect(_position, _size).Contains(mpos);

        if (_isHovered != isOver)
        {
            _isHovered = isOver;
        }

        // ensure we don't handle row clicks that target the scrollbar region
        FloatRect scrollbarArea = new(new Vector2f(_position.X + _size.X - _scrollBarWidth, _position.Y), new Vector2f(_scrollBarWidth, _size.Y));

        if (!_lastFrameMouseDown && isDown && isOver && !scrollbarArea.Contains(mpos))
        {
            System.Int32 clickedViewIndex = HIT_TEST_ROW_INDEX(mpos);
            if (clickedViewIndex >= 0)
            {
                System.Boolean ctrl = KeyboardManager.Instance.IsKeyPressed(Keyboard.Key.LControl) ||
                            KeyboardManager.Instance.IsKeyPressed(Keyboard.Key.RControl);
                System.Boolean shift = KeyboardManager.Instance.IsKeyPressed(Keyboard.Key.LShift) ||
                             KeyboardManager.Instance.IsKeyPressed(Keyboard.Key.RShift);
                TOGGLE_SELECTION(clickedViewIndex, ctrl, shift);
                RowClicked?.Invoke(clickedViewIndex, false);
            }
        }

        _lastFrameMouseDown = isDown;
    }

    /// <summary>
    /// Draws only the visible rows and headers using the SFML render target.
    /// Colors are taken from Themes static class for consistent theming.
    /// </summary>
    public override void Draw(IRenderTarget target)
    {
        if (!this.IsVisible)
        {
            return;
        }

        // Draw header background using DataGridHeaderBackgroundColor from theme
        var headerRect = new RectangleShape(new Vector2f(_size.X, _rowHeight))
        {
            FillColor = Themes.DataGridHeaderBackgroundColor,
            Position = _position
        };
        target.Draw(headerRect);

        // Draw column headers (header text color uses DataGridHeaderTextColor)
        System.Single x = _position.X;
        for (System.Int32 c = 0; c < _columns.Count; c++)
        {
            var col = _columns[c];
            var headerText = new Text(_font, col.Header, _fontSize)
            {
                FillColor = Themes.DataGridHeaderTextColor,
                Position = new Vector2f(x + 6f, _position.Y + 4f)
            };
            target.Draw(headerText);
            x += col.Width;
        }

        // Visible range
        System.Int32 visibleCount = (System.Int32)System.MathF.Ceiling((_size.Y - _rowHeight) / _rowHeight);
        visibleCount = System.Math.Max(0, visibleCount);
        System.Int32 start = System.Math.Clamp((System.Int32)System.MathF.Floor(_scrollOffset / _rowHeight), 0, System.Math.Max(0, _viewIndices.Count - 1));
        System.Int32 end = System.Math.Clamp(start + visibleCount + BufferRows, 0, _viewIndices.Count);

        // Reuse pooled drawables
        System.Int32 poolIndex = 0;
        for (System.Int32 vi = start; vi < end; vi++, poolIndex++)
        {
            System.Int32 itemIndex = _viewIndices[vi];
            T item = _items[itemIndex];

            System.Single rowY = _position.Y + _rowHeight + (vi * _rowHeight) - _scrollOffset;

            var rect = _rowShapes[poolIndex];
            rect.Position = new Vector2f(_position.X, rowY);
            rect.Size = new Vector2f(System.Math.Max(1f, _size.X - _scrollBarWidth), _rowHeight);

            // background color: selection / alternate using theme colors
            rect.FillColor = _selectedViewIndices.Contains(vi)
                ? Themes.DataGridRowSelectionColor
                : (vi % 2 == 0) ? Themes.DataGridRowBackgroundColor : Themes.DataGridRowAltBackgroundColor;
            target.Draw(rect);

            // draw each column text (use pool)
            System.Single cx = _position.X;
            var texts = _cellTextPool[poolIndex];
            for (System.Int32 c = 0; c < _columns.Count; c++)
            {
                System.String s = _columns[c].CellSelector(item) ?? "";
                var txt = texts[c];
                txt.DisplayedString = s;
                txt.CharacterSize = _fontSize;
                txt.FillColor = Themes.PrimaryTextColor;
                txt.Position = new Vector2f(cx + 6f, rowY + 4f);
                target.Draw(txt);
                cx += _columns[c].Width;
            }
        }

        // Draw scrollbar (ScrollBar draws itself)
        System.Single contentHeight = System.Math.Max(1, _viewIndices.Count) * _rowHeight;
        System.Single viewportHeight = System.Math.Max(1f, _size.Y - _rowHeight);
        if (contentHeight > viewportHeight)
        {
            _scrollBar.Draw(target);
        }
    }

    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override IDrawable GetDrawable() => throw new System.NotSupportedException("Use Draw(IRenderTarget) directly for DataGrid.");

    #endregion
}