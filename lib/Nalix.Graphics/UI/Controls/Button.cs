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

namespace Nalix.Graphics.UI.Controls;

/// <summary>
/// Represents a resizable button based on NineSlicePanel (single image).
/// Changes text color on hover using tint, supports mouse and keyboard interactions,
/// allows custom colors, and provides a fluent configuration API.
/// </summary>
public class Button : RenderObject, IUpdatable
{
    #region Constants

    private const System.Single DefaultHeight = 64f;
    private const System.Single DefaultWidth = 200f;
    private const System.UInt32 DefaultFontSize = 20;
    private const System.Single HorizontalPaddingDefault = 16f;

    private static readonly IntRect DefaultSrc = default;
    private static readonly Thickness DefaultSlice = new(32);

    #endregion Constants

    #region Fields

    private readonly Text _label;
    private readonly NineSlicePanel _panel;

    // States
    private System.Boolean _isHovered;
    private System.Boolean _isPressed;
    private System.Boolean _wasMousePressed;
    private System.Boolean _keyboardPressed;
    private System.Boolean _isEnabled = true;
    private System.Boolean _needsLayout = false;

    // Layout
    private FloatRect _totalBounds;
    private Color _customTextColor;
    private Color _customPanelColor;
    private System.Single _buttonWidth;
    private Vector2f _position = new(0, 0);
    private System.Single _buttonHeight = DefaultHeight;
    private System.Single _horizontalPadding = HorizontalPaddingDefault;

    private event System.Action OnClick;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets or sets the position of the button in screen space.
    /// </summary>
    public Vector2f Position
    {
        get => _position;
        set
        {
            if (_position != value)
            {
                _position = value;
                this.UPDATE_LAYOUT();
            }
        }
    }

    /// <summary>
    /// Gets or sets the size (width and height) of the button.
    /// </summary>
    public Vector2f Size
    {
        get => new(_buttonWidth, _buttonHeight);
        set
        {
            if (_buttonWidth != value.X || _buttonHeight != value.Y)
            {
                _buttonWidth = value.X;
                _buttonHeight = value.Y;
                this.UPDATE_LAYOUT();
            }
        }
    }

    /// <summary>
    /// Gets or sets the width of the button in pixels.
    /// </summary>
    public System.Single Width
    {
        get => _buttonWidth;
        set
        {
            if (_buttonWidth != value)
            {
                _buttonWidth = value;
                this.UPDATE_LAYOUT();
            }
        }
    }

    /// <summary>
    /// Gets or sets the height of the button in pixels.
    /// </summary>
    public System.Single Height
    {
        get => _buttonHeight;
        set
        {
            if (_buttonHeight != value)
            {
                _buttonHeight = value;
                this.UPDATE_LAYOUT();
            }
        }
    }

    /// <summary>
    /// Gets or sets the button label text.
    /// </summary>
    public System.String Text
    {
        get => _label.DisplayedString;
        set
        {
            if (_label.DisplayedString != value)
            {
                _label.DisplayedString = value;
                this.UPDATE_LAYOUT();
            }
        }
    }

    /// <summary>
    /// Gets or sets the font size of the button label.
    /// </summary>
    public System.UInt32 FontSize
    {
        get => _label.CharacterSize;
        set
        {
            if (_label.CharacterSize != value)
            {
                _label.CharacterSize = value;
                this.UPDATE_LAYOUT();
            }
        }
    }

    /// <summary>
    /// Gets or sets the horizontal padding inside the button in pixels.
    /// </summary>
    public System.Single HorizontalPadding
    {
        get => _horizontalPadding;
        set
        {
            System.Single newValue = System.MathF.Max(0f, value);
            if (_horizontalPadding != newValue)
            {
                _horizontalPadding = newValue;
                this.UPDATE_LAYOUT();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the button is enabled for interaction.
    /// </summary>
    public new System.Boolean IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
                this.APPLY_TINT();
            }
        }
    }

    /// <summary>
    /// Gets or sets the text outline color.
    /// </summary>
    public Color TextOutlineColor
    {
        get => _label.OutlineColor;
        set => _label.OutlineColor = value;
    }

    /// <summary>
    /// Gets or sets the text outline thickness in pixels.
    /// </summary>
    public System.Single TextOutlineThickness
    {
        get => _label.OutlineThickness;
        set => _label.OutlineThickness = value;
    }

    /// <summary>
    /// Gets or sets the custom text color. 
    /// When set, overrides the theme-based text color for normal state.
    /// Set to null to revert to theme colors.
    /// </summary>
    public Color TextColor
    {
        get => _customTextColor;
        set
        {
            if (_customTextColor != value)
            {
                _customTextColor = value;
                this.APPLY_TINT();
            }
        }
    }

    /// <summary>
    /// Gets or sets the custom panel color.
    /// When set, overrides the theme-based color for normal state.
    /// Set to null to revert to theme colors.
    /// </summary>
    public Color PanelColor
    {
        get => _customPanelColor;
        set
        {
            if (_customPanelColor != value)
            {
                _customPanelColor = value;
                this.APPLY_TINT();
            }
        }
    }

    /// <summary>
    /// Gets the button's bounds in screen space.
    /// </summary>
    public FloatRect GlobalBounds => _totalBounds;

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="Button"/> class.
    /// </summary>
    /// <param name="text">The button label text.</param>
    /// <param name="texture">The texture used for the button panel background.</param>
    /// <param name="width">The initial button width in pixels. Default is 240.</param>
    /// <param name="sourceRect">The source rectangle on the texture (optional).</param>
    /// <param name="font">The font used for the button label text (optional).</param>
    public Button(
        System.String text, Texture texture = null,
        System.Single width = 240f, IntRect sourceRect = default, Font font = null)
    {
        font ??= EmbeddedAssets.JetBrainsMono.ToFont();
        texture ??= EmbeddedAssets.SquareOutline.ToTexture();

        _buttonWidth = System.Math.Max(DefaultWidth, width);
        _label = new Text(font, text, DefaultFontSize) { FillColor = Color.Black };
        _panel = new NineSlicePanel(texture, DefaultSlice, sourceRect == default ? DefaultSrc : sourceRect);

        this.UPDATE_LAYOUT();
        this.APPLY_TINT();
    }

    #endregion Constructor

    #region APIs

    /// <summary>
    /// Register a callback for click event.
    /// </summary>
    public void RegisterClickHandler(System.Action handler) => OnClick += handler;

    /// <summary>
    /// Unregister a previously registered click handler.
    /// </summary>
    public void UnregisterClickHandler(System.Action handler) => OnClick -= handler;

    #endregion APIs

    #region Main Loop

    /// <summary>
    /// Updates the interactive state, mouse/keyboard events, and visual highlights.
    /// </summary>
    public override void Update(System.Single dt)
    {
        if (_needsLayout)
        {
            this.UPDATE_LAYOUT();
            _needsLayout = false;
        }

        if (!this.IsVisible)
        {
            return;
        }

        Vector2i mousePos = MouseManager.Instance.GetMousePosition();
        System.Boolean isDown = Mouse.IsButtonPressed(Mouse.Button.Left);
        System.Boolean isOver = _totalBounds.Contains(mousePos);

        // Hover
        if (_isHovered != (isOver && _isEnabled))
        {
            _isHovered = isOver && _isEnabled;
            this.APPLY_TINT();
        }

        // Mouse click logic
        if (_isEnabled)
        {
            if (isOver && isDown && !_wasMousePressed)
            {
                _isPressed = true;
            }
            else if (_isPressed && !isDown && isOver)
            {
                this.FIRE_CLICK();
                _isPressed = false;
            }
            else if (!isDown)
            {
                _isPressed = false;
            }
        }
        _wasMousePressed = isDown;

        // Keyboard (Enter/Space) when hovered for gamepad/keyboard navigation
        System.Boolean keyDown = KeyboardManager.Instance.IsKeyPressed(Keyboard.Key.Enter) ||
                                 KeyboardManager.Instance.IsKeyPressed(Keyboard.Key.Space);

        if (_isEnabled && _isHovered)
        {
            if (keyDown && !_keyboardPressed)
            {
                _keyboardPressed = true;
            }
            else if (!keyDown && _keyboardPressed)
            {
                _keyboardPressed = false; this.FIRE_CLICK();
            }
        }
        else
        {
            _keyboardPressed = false;
        }
    }

    /// <summary>
    /// Renders the button and its label.
    /// </summary>
    public override void Draw(IRenderTarget target)
    {
        if (!this.IsVisible)
        {
            return;
        }

        _panel.Draw(target);
        target.Draw(_label);
    }

    /// <summary>
    /// This button does not support GetDrawable (use Render).
    /// </summary>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override IDrawable GetDrawable() => throw new System.NotSupportedException("Use Render() instead.");

    #endregion

    #region Layout

    /// <summary>
    /// Updates panel and text geometry based on current layout/padding/text.
    /// </summary>
    private void UPDATE_LAYOUT()
    {
        // Ensure enough room for text + padding
        FloatRect tb = _label.GetLocalBounds();

        System.Single minTextWidth = tb.Width + (_horizontalPadding * 2f);
        System.Single minWidth = _panel.Border.Left + _panel.Border.Right;
        System.Single minHeight = _panel.Border.Top + _panel.Border.Bottom;

        System.Single totalWidth = System.Math.Max(_buttonWidth, System.Math.Max(DefaultWidth, minTextWidth));
        totalWidth = System.Math.Max(totalWidth, minWidth);

        System.Single totalHeight = System.Math.Max(_buttonHeight, DefaultHeight);
        totalHeight = System.Math.Max(totalHeight, minHeight);

        System.Single x = _position.X + ((totalWidth - tb.Width) * 0.5f) - tb.Left;
        System.Single y = _position.Y + ((totalHeight - tb.Height) * 0.5f) - tb.Top;


        _label.Position = new Vector2f(x, y);
        _panel.SetPosition(_position).SetSize(new Vector2f(totalWidth, totalHeight));
        _totalBounds = new FloatRect(_position, new Vector2f(totalWidth, totalHeight));
    }

    #endregion Layout

    #region Visual Helpers

    /// <summary>
    /// Updates the panel and text color/tint based on state (normal/hover/disabled).
    /// </summary>
    private void APPLY_TINT()
    {
        if (!_isEnabled)
        {
            _label.FillColor = Themes.TextTheme.Disabled;
            _panel.SetTintColor(Themes.PanelTheme.Disabled);

            return;
        }

        _label.FillColor = _isHovered ? Themes.TextTheme.Hover : Themes.TextTheme.Normal;

        _label.FillColor = _customTextColor != default
            ? _isHovered ? this.BLEND_COLOR(_customTextColor, Themes.TextTheme.Hover) : _customTextColor
            : _isHovered ? Themes.TextTheme.Hover : Themes.TextTheme.Normal;

        if (_customPanelColor != default)
        {
            _panel.SetTintColor(_isHovered ? this.BLEND_COLOR(_customPanelColor, Themes.PanelTheme.Hover) : _customPanelColor);
        }
        else
        {
            _panel.SetTintColor(_isHovered ? Themes.PanelTheme.Hover : Themes.PanelTheme.Normal);
        }
    }

    /// <summary>
    /// Blends two colors for hover effect when using custom panel color.
    /// </summary>
    /// <param name="baseColor">The base custom color.</param>
    /// <param name="hoverColor">The hover theme color.</param>
    /// <returns>A blended color for hover state.</returns>
    private Color BLEND_COLOR(Color baseColor, Color hoverColor)
    {
        const System.Single blendFactor = 0.3f;

        return new Color(
            (System.Byte)((baseColor.R * (1f - blendFactor)) + (hoverColor.R * blendFactor)),
            (System.Byte)((baseColor.G * (1f - blendFactor)) + (hoverColor.G * blendFactor)),
            (System.Byte)((baseColor.B * (1f - blendFactor)) + (hoverColor.B * blendFactor)),
            baseColor.A
        );
    }

    /// <summary>
    /// Triggers registered click callbacks.
    /// </summary>
    private void FIRE_CLICK() => this.OnClick?.Invoke();

    #endregion Visual Helpers
}
