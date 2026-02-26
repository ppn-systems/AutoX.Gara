// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Graphics.Abstractions;
using Nalix.Graphics.Assets;
using Nalix.Graphics.Entities;
using Nalix.Graphics.Extensions;
using Nalix.Graphics.Input;
using Nalix.Graphics.Layout;
using Nalix.Graphics.UI.Theme;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Collections.Generic;

namespace Nalix.Graphics.UI.Controls;

/// <summary>
/// ComboBox control supporting single and multiple selection modes.
/// Uses dropdown with optional ScrollBar, themed rendering, and Unicode check marks.
/// </summary>
public class ComboBox<T> : RenderObject, IUpdatable where T : notnull
{

    private const System.Single ItemHeight = 28f;
    private const System.Single HeaderHeight = 32f;
    private const System.Single MaxDropdownItems = 8;

    #region Fields

    private static System.Boolean _wasMouseDownLastFrame = false;

    private readonly List<T> _items;
    private readonly List<System.Int32> _selectedIndices = [];
    private readonly System.Boolean _isMultiSelect;
    private readonly Texture _checkIconTexture;
    private readonly Vector2f _checkIconSize = new(18f, 18f);
    private System.Boolean _dropdownOpen = false;
    private System.Int32 _hoveredIndex = -1;

    private readonly NineSlicePanel _headerPanel;
    private readonly Text _headerText;
    private readonly RectangleShape _dropdownBox;
    private readonly List<Text> _itemTexts = [];
    private readonly Font _font;
    private readonly ScrollBar _scrollBar;

    private System.Single _dropdownHeight = 0f;
    private System.Single _width = 180f;

    private Vector2f _position = new(0, 0);

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets or sets the ComboBox's position.
    /// </summary>
    public Vector2f Position
    {
        get => _position;
        set
        {
            _position = value;
            UPDATE_LAYOUT();
        }
    }

    /// <summary>
    /// Gets or sets the ComboBox's width.
    /// </summary>
    public System.Single Width
    {
        get => _width;
        set
        {
            _width = value;
            UPDATE_LAYOUT();
        }
    }

    /// <summary>
    /// Gets the items in the ComboBox.
    /// </summary>
    public IReadOnlyList<T> Items => _items;

    /// <summary>
    /// Gets the indices of selected items.
    /// </summary>
    public IReadOnlyList<System.Int32> SelectedIndices => _selectedIndices;

    /// <summary>
    /// Gets or sets whether this ComboBox is enabled.
    /// </summary>
    public new System.Boolean IsEnabled { get; set; } = true;

    /// <summary>
    /// Event fired when selection changes after closing dropdown.
    /// Provides the list of selected item strings.
    /// </summary>
    public event System.Action<List<System.String>> OnSelectionChanged;

    /// <summary>
    /// Gets or sets whether the ComboBox's dropdown is open.
    /// </summary>
    public System.Boolean IsDropdownOpen
    {
        get => _dropdownOpen;
        set
        {
            if (_dropdownOpen != value)
            {
                _dropdownOpen = value;
                UPDATE_LAYOUT();
                if (!_dropdownOpen)
                {
                    // Fire selection event when dropdown closes
                    OnSelectionChanged?.Invoke(GetSelectedItems());
                }
            }
        }
    }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new ComboBox instance.
    /// </summary>
    /// <param name="items">The fixed list of selectable items (string).</param>
    /// <param name="multiSelect">True to enable multi-selection. False: single-select.</param>
    /// <param name="width">ComboBox width in pixels.</param>
    public ComboBox(IEnumerable<T> items, System.Boolean multiSelect = false, System.Single width = 180f)
    {
        _font = EmbeddedAssets.JetBrainsMono.ToFont();
        _checkIconTexture = EmbeddedAssets.Done.ToTexture();

        _items = [.. items];
        _isMultiSelect = multiSelect;
        _width = width;

        // Header panel = NineSlicePanel (giống Button)
        _headerPanel = new NineSlicePanel(EmbeddedAssets.SquareOutline.ToTexture(), new Thickness(32), default);
        _headerPanel.SetPosition(_position).SetSize(new Vector2f(_width, HeaderHeight));
        _headerPanel.SetTintColor(Themes.PanelTheme.Normal);

        _headerText = new Text(_font)
        {
            CharacterSize = 18,
            DisplayedString = GetHeaderText(),
            FillColor = Themes.PrimaryTextColor,
            Position = new Vector2f(_position.X + 12f, _position.Y + 7f),
        };

        // Dropdown setup
        _dropdownBox = new RectangleShape();
        _itemTexts.Clear();
        for (System.Int32 i = 0; i < _items.Count; i++)
        {
            var t = new Text(_font)
            {
                CharacterSize = 16,
                DisplayedString = _items[i]?.ToString() ?? "",
                FillColor = Themes.TextTheme.Normal,
                Position = new Vector2f(_position.X + 24f, _position.Y + HeaderHeight + (i * ItemHeight))
            };
            _itemTexts.Add(t);
        }

        // ScrollBar setup (if needed)
        if (_items.Count > MaxDropdownItems)
        {
            _scrollBar = new ScrollBar
            {
                Size = new Vector2f(12f, MaxDropdownItems * ItemHeight),
                Position = new Vector2f(_position.X + _width - 12f, _position.Y + HeaderHeight),
            };
            _scrollBar.SetViewportRatio(MaxDropdownItems / _items.Count);
            _scrollBar.ValueChanged += (_) => UPDATE_LAYOUT();
        }

        UPDATE_LAYOUT();
    }

    #endregion Constructor

    #region APIs

    /// <summary>
    /// Gets the current selected item(s) as string list.
    /// </summary>
    public List<System.String> GetSelectedItems()
    {
        var selected = new List<System.String>();
        foreach (System.Int32 idx in _selectedIndices)
        {
            selected.Add(_items[idx]?.ToString() ?? "");
        }

        return selected;
    }

    #endregion APIs

    #region Main Loop

    /// <summary>
    /// Handles input, hover/select logic and updates ScrollBar.
    /// </summary>
    public override void Update(System.Single dt)
    {
        if (!IsEnabled)
        {
            return;
        }

        Vector2i mouse = MouseManager.Instance.GetMousePosition();
        System.Boolean mouseDown = Mouse.IsButtonPressed(Mouse.Button.Left);

        var headerBounds = new FloatRect(_headerPanel.Position, new Vector2f((System.Single)(_headerPanel.Size.X * 1.5), (System.Single)(_headerPanel.Size.Y * 1.5)));

        // Tạo bộ nhớ tạm cho trạng thái mouse trước đó


        // Nếu click vào header
        if (headerBounds.Contains(mouse))
        {
            // Chỉ toggle khi nhấn xuống lần đầu (tránh giữ chuột toggle liên tục)
            if (mouseDown && !_wasMouseDownLastFrame)
            {
                IsDropdownOpen = !_dropdownOpen;
                _hoveredIndex = -1;
            }
        }
        else if (mouseDown && _dropdownOpen)
        {
            // Click ngoài dropdown để đóng lại
            var dropdownBounds = new FloatRect(_dropdownBox.Position, _dropdownBox.Size);
            if (!dropdownBounds.Contains(mouse))
            {
                IsDropdownOpen = false;
            }
        }

        // Dropdown open: handle item hover/select and ScrollBar interaction
        if (_dropdownOpen)
        {
            System.Single y = mouse.Y - _dropdownBox.Position.Y;
            System.Int32 itemIdx = (System.Int32)(y / ItemHeight);
            if (itemIdx >= 0 && itemIdx < _items.Count)
            {
                _hoveredIndex = itemIdx;
                if (mouseDown)
                {
                    if (_isMultiSelect)
                    {
                        if (!_selectedIndices.Remove(itemIdx))
                        {
                            _selectedIndices.Add(itemIdx);
                        }
                    }
                    else
                    {
                        _selectedIndices.Clear();
                        _selectedIndices.Add(itemIdx);
                    }
                    _headerText.DisplayedString = GetHeaderText();

                    FloatRect textBounds = _headerText.GetLocalBounds();
                    _headerText.Position = new Vector2f(
                        _headerPanel.Position.X + ((_headerPanel.Size.X - textBounds.Width) / 2f) - textBounds.Left,
                        _headerPanel.Position.Y + ((_headerPanel.Size.Y - textBounds.Height) / 2f) - textBounds.Top + 15
                    );
                }
            }

            _scrollBar?.Update(dt);
        }
        else
        {
            _hoveredIndex = -1;
        }
        // Lưu trạng thái mouse cho frame kế tiếp
        _wasMouseDownLastFrame = mouseDown;
    }

    /// <summary>
    /// Renders ComboBox header and dropdown (if open). Includes items, check marks and scrollbar.
    /// </summary>
    public override void Draw(IRenderTarget target)
    {
        if (!this.IsVisible)
        {
            return;
        }

        // Draw header
        target.Draw(_headerPanel);
        target.Draw(_headerText);

        // Draw dropdown if open
        if (_dropdownOpen)
        {
            target.Draw(_dropdownBox);

            System.Int32 startIdx = 0;
            System.Int32 endIdx = _items.Count;
            if (_scrollBar != null)
            {
                System.Single maxOffset = _items.Count - MaxDropdownItems;
                startIdx = (System.Int32)(_scrollBar.Value * maxOffset);
                endIdx = System.Math.Min(startIdx + (System.Int32)MaxDropdownItems, _items.Count);
            }

            for (System.Int32 i = startIdx; i < endIdx; i++)
            {
                var text = _itemTexts[i];

                // Update text position for scrolling
                System.Single y = _dropdownBox.Position.Y + ((i - startIdx) * ItemHeight);
                text.Position = new Vector2f(_dropdownBox.Position.X + 24f, y + 4f);

                // Item background highlight
                if (i == _hoveredIndex)
                {
                    var bg = new RectangleShape(new Vector2f(_width, ItemHeight))
                    {
                        Position = new Vector2f(_dropdownBox.Position.X, y),
                        FillColor = Themes.DataGridRowHoverColor
                    };
                    target.Draw(bg);
                }
                // Item text color
                text.FillColor = _selectedIndices.Contains(i)
                    ? Themes.DataGridRowSelectionColor
                    : Themes.TextTheme.Normal;

                target.Draw(text);

                // Draw check mark if selected
                if (_selectedIndices.Contains(i) && _checkIconTexture != null)
                {
                    var checkMarkSprite = new Sprite(_checkIconTexture)
                    {
                        Position = new Vector2f(_dropdownBox.Position.X + _width - _checkIconSize.X - 15f, y + ((ItemHeight - _checkIconSize.Y) / 2f)),
                        Scale = new Vector2f(_checkIconSize.X / _checkIconTexture.Size.X, _checkIconSize.Y / _checkIconTexture.Size.Y)
                    };
                    target.Draw(checkMarkSprite);
                }
            }

            // Draw ScrollBar
            _scrollBar?.Draw(target);
        }
    }

    /// <summary>
    /// Not supported, because ComboBox renders multiple components.
    /// </summary>
    protected override IDrawable GetDrawable() => throw new System.NotSupportedException("ComboBox uses Draw(IRenderTarget).");

    #endregion Main Loop

    #region Layout & Helpers

    /// <summary>
    /// Updates layout (positions, sizes) of header, dropdown box, scrollbar.
    /// </summary>
    private void UPDATE_LAYOUT()
    {
        _headerPanel.SetPosition(_position).SetSize(new Vector2f(_width, HeaderHeight));

        FloatRect textBounds = _headerText.GetLocalBounds();
        _headerText.Position = new Vector2f(
            _headerPanel.Position.X + ((_headerPanel.Size.X - textBounds.Width) / 2f) - textBounds.Left,
            _headerPanel.Position.Y + ((_headerPanel.Size.Y - textBounds.Height) / 2f) - textBounds.Top + 15
        );

        _dropdownHeight = (_items.Count > MaxDropdownItems ? MaxDropdownItems : _items.Count) * ItemHeight;
        _dropdownBox.Position = new Vector2f(_position.X, _position.Y + HeaderHeight + 30);
        _dropdownBox.Size = new Vector2f(_width, _dropdownHeight);
        _dropdownBox.FillColor = Themes.DataGridRowBackgroundColor;

        if (_scrollBar != null)
        {
            _scrollBar.Position = new Vector2f(_dropdownBox.Position.X + _width - 12f, _dropdownBox.Position.Y);
            _scrollBar.Size = new Vector2f(12f, _dropdownHeight);
            _scrollBar.SetViewportRatio(MaxDropdownItems / _items.Count);
        }
    }

    /// <summary>
    /// Gets header text reflecting selection.
    /// </summary>
    private System.String GetHeaderText()
    {
        return _selectedIndices.Count == 0
            ? "-- Select --"
            : _isMultiSelect ? System.String.Join(", ", GetSelectedItems()) : GetSelectedItems()[0];
    }

    #endregion Layout & Helpers
}