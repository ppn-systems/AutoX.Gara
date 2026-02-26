// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Graphics.Abstractions;
using Nalix.Graphics.Assets;
using Nalix.Graphics.Entities;
using Nalix.Graphics.Extensions;
using Nalix.Graphics.Input;
using Nalix.Graphics.UI.Theme;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Collections.Generic;

namespace Nalix.Graphics.UI.Tab;

/// <summary>
/// Vertical navigation pane for application sections.
/// Provides items (Jobs, Customers, Inventory, Reports, Settings) style navigation,
/// keyboard navigation, and selection event.
/// </summary>
public sealed class NavigationPane(Font font = null) : RenderObject, IUpdatable
{
    #region Fields

    private readonly List<(System.String Title, Texture Icon, System.Object Tag)> _items = [];
    private System.Int32 _selectedIndex = -1;
    private System.Int32 _hoverIndex = -1;
    private readonly Font _font = font ?? EmbeddedAssets.JetBrainsMono.ToFont();
    private Vector2f _position = new(0f, 0f);
    private Vector2f _size = new(200f, 400f);
    private readonly System.Single _itemHeight = 44f;

    // pooling drawables
    private readonly List<Text> _textPool = [];
    private readonly List<RectangleShape> _bgPool = [];

    // keyboard state
    private (System.Boolean LastUp, System.Boolean LastDown, System.Boolean LastEnter) _keyState;
    private System.Boolean _lastMouseDown = false;

    #endregion Fields

    #region Events

    /// <summary>
    /// Raised when the selected item changes: (index, title, tag).
    /// </summary>
    public event System.Action<System.Int32, System.String, System.Object> SelectionChanged;

    #endregion Events

    #region Properties

    /// <summary>
    /// Position of navigation pane.
    /// </summary>
    public Vector2f Position
    {
        get => _position;
        set
        {
            if (_position != value)
            {
                _position = value;
            }
        }
    }

    /// <summary>
    /// Size of navigation pane.
    /// </summary>
    public Vector2f Size
    {
        get => _size;
        set
        {
            _size = value;
            // ensure pool shapes width updated
            for (System.Int32 i = 0; i < _bgPool.Count; i++)
            {
                _bgPool[i].Size = new Vector2f(_size.X, _itemHeight);
            }
        }
    }

    #endregion Properties

    #region APIs

    /// <summary>
    /// Add a navigation item with optional icon and tag payload.
    /// </summary>
    public System.Int32 AddItem(System.String title, Texture icon = null, System.Object tag = null)
    {
        _items.Add((title, icon, tag));
        _textPool.Add(new Text(_font, title, 14) { FillColor = Themes.PrimaryTextColor });
        _bgPool.Add(new RectangleShape(new Vector2f(_size.X, _itemHeight)) { FillColor = Color.Transparent });
        if (_selectedIndex == -1)
        {
            Select(0);
        }

        return _items.Count - 1;
    }

    /// <summary>
    /// Select item by index.
    /// </summary>
    public void Select(System.Int32 index)
    {
        if (index < 0 || index >= _items.Count)
        {
            return;
        }

        if (_selectedIndex == index)
        {
            return;
        }

        _selectedIndex = index;
        SelectionChanged?.Invoke(index, _items[index].Title, _items[index].Tag);
    }

    /// <summary>
    /// Update handles mouse hover/click and keyboard navigation (Up/Down/Enter).
    /// </summary>
    public override void Update(System.Single dt)
    {
        Vector2i mpos = MouseManager.Instance.GetMousePosition();
        System.Boolean isDown = Mouse.IsButtonPressed(Mouse.Button.Left);

        // hit test items
        System.Int32 newHover = -1;
        var bounds = new FloatRect(_position, _size);
        if (bounds.Contains(mpos))
        {
            System.Single localY = mpos.Y - _position.Y;
            System.Int32 idx = (System.Int32)(localY / _itemHeight);
            if (idx >= 0 && idx < _items.Count)
            {
                newHover = idx;
            }
        }
        _hoverIndex = newHover;

        // click
        if (!_lastMouseDown && isDown && _hoverIndex >= 0)
        {
            Select(_hoverIndex);
        }

        // keyboard nav
        System.Boolean up = KeyboardManager.Instance.IsKeyPressed(Keyboard.Key.Up);
        System.Boolean down = KeyboardManager.Instance.IsKeyPressed(Keyboard.Key.Down);
        System.Boolean enter = KeyboardManager.Instance.IsKeyPressed(Keyboard.Key.Enter);

        if (!_keyState.LastUp && up)
        {
            if (_items.Count == 0) { }
            else
            {
                System.Int32 next = _selectedIndex <= 0 ? _items.Count - 1 : _selectedIndex - 1;
                Select(next);
            }
        }
        if (!_keyState.LastDown && down)
        {
            if (_items.Count == 0) { }
            else
            {
                System.Int32 next = _selectedIndex >= _items.Count - 1 ? 0 : _selectedIndex + 1;
                Select(next);
            }
        }
        if (!_keyState.LastEnter && enter)
        {
            // Enter behaves like click: if hovered, select hovered
            if (_hoverIndex >= 0)
            {
                Select(_hoverIndex);
            }
        }

        _keyState.LastUp = up;
        _keyState.LastDown = down;
        _keyState.LastEnter = enter;
        _lastMouseDown = isDown;
    }

    public override void Draw(IRenderTarget target)
    {
        if (!this.IsVisible)
        {
            return;
        }

        // draw panel background
        var panel = new RectangleShape(_size) { Position = _position, FillColor = WithAlpha(Themes.PanelTheme.Normal, 220) };
        target.Draw(panel);

        // draw items
        for (System.Int32 i = 0; i < _items.Count; i++)
        {
            System.Single y = _position.Y + (i * _itemHeight);
            var bg = _bgPool[i];
            bg.Position = new Vector2f(_position.X, y);
            bg.Size = new Vector2f(_size.X, _itemHeight);

            // visual states
            bg.FillColor = i == _selectedIndex
                ? WithAlpha(Themes.DataGridRowSelectionColor, 220)
                : i == _hoverIndex ? WithAlpha(Themes.PanelTheme.Hover, 160) : Color.Transparent;

            target.Draw(bg);

            System.Single x = _position.X + 8f;
            if (_items[i].Icon != null)
            {
                var spr = new Sprite(_items[i].Icon)
                {
                    Position = new Vector2f(x, y + 6f),
                    Scale = new Vector2f((_itemHeight - 12f) / _items[i].Icon.Size.X, (_itemHeight - 12f) / _items[i].Icon.Size.Y)
                };
                target.Draw(spr);
                x += _itemHeight; // reserve icon area
            }

            var txt = _textPool[i];
            txt.CharacterSize = 14;
            txt.DisplayedString = _items[i].Title;
            txt.FillColor = (i == _selectedIndex) ? Themes.TextTheme.Hover : Themes.PrimaryTextColor;
            txt.Position = new Vector2f(x + 8f, y + ((_itemHeight - txt.CharacterSize) / 2f) - 2f);
            target.Draw(txt);
        }
    }

    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override IDrawable GetDrawable() => throw new System.NotSupportedException("NavigationPane uses Draw(IRenderTarget).");

    #endregion APIs

    #region Private Methods

    private static Color WithAlpha(Color c, System.Byte a) => new(c.R, c.G, c.B, a);

    #endregion Private Methods
}
