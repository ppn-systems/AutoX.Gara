// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Graphics.Abstractions;
using Nalix.Graphics.Assets;
using Nalix.Graphics.Entities;
using Nalix.Graphics.Extensions;
using Nalix.Graphics.Input;
using Nalix.Graphics.UI.Models;
using Nalix.Graphics.UI.Theme;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Collections.Generic;

namespace Nalix.Graphics.UI.Tab;

/// <summary>
/// Horizontal tab control. Provides tab creation, selection and keyboard navigation.
/// </summary>
public sealed class TabControl : RenderObject, IUpdatable
{
    private readonly List<TabItem> _tabs = [];
    private System.Int32 _selectedIndex = -1;
    private readonly Font _font;
    private Vector2f _position = new(0f, 0f);
    private System.Single _height = 36f;
    private System.Single _tabPadding = 12f;
    private System.Single _tabSpacing = 4f;
    private readonly List<Text> _tabTexts = [];
    private readonly List<RectangleShape> _tabBackgrounds = [];

    // keyboard state helpers
    private (System.Boolean LastLeft, System.Boolean LastRight) _keyState;

    /// <summary>
    /// Fired when active tab is changed. Parameter is (index, TabItem).
    /// </summary>
    public event System.Action<System.Int32, TabItem> TabChanged;

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
    /// Height of the tab strip.
    /// </summary>
    public System.Single Height
    {
        get => _height; set => _height = System.MathF.Max(18f, value);
    }

    /// <summary>
    /// Creates a new TabControl using the embedded font by default.
    /// </summary>
    public TabControl(Font font = null) => _font = font ?? EmbeddedAssets.JetBrainsMono.ToFont();

    /// <summary>
    /// Add a new tab and optionally select it.
    /// </summary>
    public System.Int32 AddTab(TabItem item, System.Boolean select = false)
    {
        if (item == null)
        {
            throw new System.ArgumentNullException(nameof(item));
        }

        _tabs.Add(item);
        _tabTexts.Add(new Text(_font, item.Title, 14) { FillColor = Themes.PrimaryTextColor });
        _tabBackgrounds.Add(new RectangleShape(new Vector2f(100f, _height)) { FillColor = Color.Transparent });
        if (select || _selectedIndex == -1)
        {
            Select(_tabs.Count - 1);
        }
        return _tabs.Count - 1;
    }

    /// <summary>
    /// Remove tab at index.
    /// </summary>
    public void RemoveTabAt(System.Int32 index)
    {
        if (index < 0 || index >= _tabs.Count)
        {
            return;
        }

        _tabs.RemoveAt(index);
        _tabTexts.RemoveAt(index);
        _tabBackgrounds.RemoveAt(index);
        if (_selectedIndex >= _tabs.Count)
        {
            _selectedIndex = _tabs.Count - 1;
        }

        if (_selectedIndex >= 0)
        {
            TabChanged?.Invoke(_selectedIndex, _tabs[_selectedIndex]);
        }
    }

    /// <summary>
    /// Get or set selected index.
    /// </summary>
    public System.Int32 SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (value >= 0 && value < _tabs.Count)
            {
                Select(value);
            }
        }
    }

    private void Select(System.Int32 index)
    {
        if (index < 0 || index >= _tabs.Count)
        {
            return;
        }

        if (_selectedIndex == index)
        {
            return;
        }

        _selectedIndex = index;
        TabChanged?.Invoke(index, _tabs[index]);
    }

    /// <summary>
    /// Update handles mouse click on tabs and keyboard navigation (Left/Right).
    /// </summary>
    public override void Update(System.Single dt)
    {
        Vector2i mpos = MouseManager.Instance.GetMousePosition();
        System.Boolean isDown = Mouse.IsButtonPressed(Mouse.Button.Left);

        // Layout tabs horizontally and compute clickable areas each frame.
        System.Single x = _position.X + 4f;
        for (System.Int32 i = 0; i < _tabs.Count; i++)
        {
            var tt = _tabTexts[i];
            tt.CharacterSize = 14;
            var bounds = tt.GetLocalBounds();
            System.Single tabWidth = bounds.Width + (_tabPadding * 2f) + (_tabs[i].Icon != null ? _height : 0f);
            var bg = _tabBackgrounds[i];
            bg.Size = new Vector2f(tabWidth, _height);
            bg.Position = new Vector2f(x, _position.Y);
            System.Boolean over = new FloatRect(bg.Position, bg.Size).Contains(mpos);

            // hover visual
            bg.FillColor = (_selectedIndex == i) ? Themes.PanelTheme.Hover : (over ? WithAlpha(Themes.PanelTheme.Hover, 160) : Color.Transparent);

            // click detection (simple)
            if (over && isDown && !_wasMouseDown)
            {
                Select(i);
            }

            // draw text position updated during Draw; we only update background here
            x += tabWidth + _tabSpacing;
        }

        // keyboard nav (single frame transition)
        System.Boolean left = KeyboardManager.Instance.IsKeyPressed(Keyboard.Key.Left);
        System.Boolean right = KeyboardManager.Instance.IsKeyPressed(Keyboard.Key.Right);

        if (!_keyState.LastLeft && left)
        {
            if (_tabs.Count > 0)
            {
                System.Int32 next = (_selectedIndex <= 0) ? _tabs.Count - 1 : _selectedIndex - 1;
                Select(next);
            }
        }
        if (!_keyState.LastRight && right)
        {
            if (_tabs.Count > 0)
            {
                System.Int32 next = (_selectedIndex >= _tabs.Count - 1) ? 0 : _selectedIndex + 1;
                Select(next);
            }
        }

        _keyState.LastLeft = left;
        _keyState.LastRight = right;
        _wasMouseDown = isDown;
    }

    private System.Boolean _wasMouseDown = false;

    public override void Draw(IRenderTarget target)
    {
        if (!this.IsVisible)
        {
            return;
        }
        // draw each tab background then text/icon
        System.Single x = _position.X + 4f;
        for (System.Int32 i = 0; i < _tabs.Count; i++)
        {
            var bg = _tabBackgrounds[i];
            // bg already positioned in Update but ensure correct
            bg.Position = new Vector2f(x, _position.Y);
            target.Draw(bg);

            // draw optional icon at left of tab
            System.Single innerX = x + 6f;
            if (_tabs[i].Icon != null)
            {
                var sprite = new Sprite(_tabs[i].Icon)
                {
                    Position = new Vector2f(innerX, _position.Y + 4f),
                    Scale = new Vector2f((_height - 8f) / _tabs[i].Icon.Size.X, (_height - 8f) / _tabs[i].Icon.Size.Y)
                };
                target.Draw(sprite);
                innerX += _height; // reserve icon width
            }

            var tt = _tabTexts[i];
            tt.FillColor = (_selectedIndex == i) ? Themes.TextTheme.Hover : Themes.PrimaryTextColor;
            tt.Position = new Vector2f(innerX + 6f, _position.Y + ((_height - tt.CharacterSize) / 2f) - 2f);
            target.Draw(tt);

            x += bg.Size.X + _tabSpacing;
        }
    }

    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override IDrawable GetDrawable() => throw new System.NotSupportedException("TabControl uses Draw(IRenderTarget).");

    private static Color WithAlpha(Color c, System.Byte a) => new(c.R, c.G, c.B, a);
}

