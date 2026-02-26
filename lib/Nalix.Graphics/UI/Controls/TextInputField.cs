// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Graphics.Abstractions;
using Nalix.Graphics.Assets;
using Nalix.Graphics.Entities;
using Nalix.Graphics.Enums;
using Nalix.Graphics.Extensions;
using Nalix.Graphics.Input;
using Nalix.Graphics.Internal;
using Nalix.Graphics.Internal.Input;
using Nalix.Graphics.Layout;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Nalix.Graphics.UI.Controls;

/// <summary>
/// Lightweight single-line text input built atop your <c>NineSlicePanel</c>.
/// </summary>
/// <remarks>
/// <para>
/// - Click to focus; caret blinks when focused.<br/>
/// - Typing supports A–Z, 0–9, a few punctuation keys (space, '.' ',' '-' '\''), Backspace/Delete with key-repeat.<br/>
/// - Text scrolls to ensure the caret (at end) is always visible inside the box width.<br/>
/// - Rendering order: panel → text → caret (if focused &amp; visible).<br/>
/// </para>
/// </remarks>
public class TextInputField : RenderObject, IFocusable
{
    #region Constants

    private const System.Single CaretYOffset = 2f;
    private const System.Single MinSizeOffset = 1f;
    private const System.Single DefaultPaddingY = 6f;
    private const System.Single MinCaretWidth = 0.5f;
    private const System.Single DefaultPaddingX = 16f;
    private const System.Single DefaultCaretWidth = 2f;
    private const System.Single CaretBlinkPeriod = 0.5f;
    private const System.Single KeyRepeatNextDelay = 0.05f;
    private const System.Single KeyRepeatFirstDelay = 0.35f;

    #endregion Constants

    #region Fields

    private readonly Text _text;
    private readonly Text _measure;
    private readonly RectangleShape _caret;
    private readonly NineSlicePanel _panel;
    private readonly System.UInt32 _fontSize;
    private readonly KeyRepeatController _deleteRepeat;
    private readonly KeyRepeatController _backspaceRepeat;
    private readonly System.Text.StringBuilder _buffer = new();

    private Vector2f _padding;
    private FloatRect _hitBox;
    private System.Int32 _caretIndex;
    private System.Int32 _scrollStart;
    private System.Single _caretTimer;
    private System.Single _caretWidth;
    private System.Boolean _caretVisible;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Maximum number of characters allowed; <c>null</c> means unlimited.
    /// </summary>
    public System.Int32? MaxLength { get; set; }

    /// <summary>
    /// Optional placeholder (shown when <see cref="Text"/> is empty and unfocused).
    /// </summary>
    public System.String Placeholder { get; set; } = System.String.Empty;

    /// <summary>
    /// Validation rule for input text; can be <c>null</c> for no validation.
    /// </summary>
    public ITextValidationRule ValidationRule { get; set; }

    /// <summary>
    /// Gets or sets the current text content.
    /// </summary>
    public System.String Text
    {
        get => _buffer.ToString();
        set
        {
            _ = _buffer.Clear().Append(value ?? System.String.Empty);
            _caretIndex = _buffer.Length;
            this.CLAMP_TO_MAX_LENGTH();
            this.RESET_SCROLL_AND_CARET();
            this.TextChanged?.Invoke(_buffer.ToString());
        }
    }

    /// <summary>
    /// Gets whether the field is currently focused.
    /// </summary>
    public System.Boolean Focused { get; private set; }

    /// <summary>
    /// Gets or sets the position of the text input field.
    /// Text position is derived from panel position + padding.
    /// </summary>
    public Vector2f Position
    {
        get => _panel.Position;
        set
        {
            if (_panel.Position != value)
            {
                _ = _panel.SetPosition(value);
                this.RELAYOUT_TEXT();
                this.UPDATE_HIT_BOX();
                this.UPDATE_CARET_IMMEDIATE();
            }
        }
    }

    /// <summary>
    /// Gets or sets the size of the text input field.
    /// Panel size; text area is inner size minus padding.
    /// </summary>
    public Vector2f Size
    {
        get => _panel.Size;
        set
        {
            Vector2f newSize = ENSURE_MIN_SIZE(value, _panel.Border);
            if (_panel.Size != newSize)
            {
                _ = _panel.SetSize(newSize);
                this.RELAYOUT_TEXT();
                this.UPDATE_HIT_BOX();
                this.RESET_SCROLL_AND_CARET();
            }
        }
    }

    /// <summary>
    /// Gets or sets the padding (x,y) inside the panel.
    /// </summary>
    public Vector2f Padding
    {
        get => _padding;
        set
        {
            if (_padding != value)
            {
                _padding = value;
                this.RELAYOUT_TEXT();
                this.RESET_SCROLL_AND_CARET();
            }
        }
    }

    /// <summary>
    /// Gets or sets the width of the caret in pixels.
    /// </summary>
    public System.Single CaretWidth
    {
        get => _caretWidth;
        set
        {
            System.Single newWidth = System.MathF.Max(MinCaretWidth, value);
            if (_caretWidth != newWidth)
            {
                _caretWidth = newWidth;
                this.UPDATE_CARET_IMMEDIATE();
            }
        }
    }

    /// <summary>
    /// Gets or sets the text and caret color.
    /// </summary>
    public Color TextColor
    {
        get => _text.FillColor;
        set
        {
            _text.FillColor = value;
            _caret.FillColor = value;
        }
    }

    /// <summary>
    /// Gets or sets the panel's tint color.
    /// </summary>
    public Color PanelColor
    {
        set => _panel.SetTintColor(value);
    }

    #endregion Properties

    #region Events

    /// <summary>
    /// Raised whenever <see cref="Text"/> changes.
    /// </summary>
    public event System.Action<System.String> TextChanged;

    /// <summary>
    /// Raised when user presses Enter while focused.
    /// </summary>
    public event System.Action<System.String> TextSubmitted;

    #endregion Events

    #region Construction

    /// <summary>
    /// Creates a new <see cref="TextInputField"/>.
    /// </summary>
    /// <param name="panelTexture">9-slice texture.</param>
    /// <param name="border">9-slice borders.</param>
    /// <param name="sourceRect">Texture rect.</param>
    /// <param name="size">Panel size (will be clamped to minimal size by borders).</param>
    /// <param name="position">Top-left position.</param>
    /// <param name="font">SFML font to render text.</param>
    /// <param name="fontSize">Font size in points.</param>
    public TextInputField(
        Texture panelTexture,
        Thickness border,
        IntRect sourceRect,
        Vector2f size,
        Vector2f position,
        Font font = null,
        System.UInt32 fontSize = 16)
    {
        _caretWidth = DefaultCaretWidth;
        _fontSize = fontSize;
        _deleteRepeat = new();
        _backspaceRepeat = new();
        font ??= EmbeddedAssets.JetBrainsMono.ToFont();
        _padding = new(DefaultPaddingX, DefaultPaddingY);

        _panel = new NineSlicePanel(panelTexture, border, sourceRect);
        _ = _panel.SetPosition(position).SetSize(ENSURE_MIN_SIZE(size, border));

        Color textColor = new(30, 30, 30);

        _measure = new Text(font, System.String.Empty, _fontSize)
        {
            FillColor = textColor
        };

        _text = new Text(font, System.String.Empty, _fontSize)
        {
            FillColor = textColor
        };

        this.RELAYOUT_TEXT();

        _caret = new RectangleShape(new Vector2f(_caretWidth, _fontSize))
        {
            FillColor = textColor
        };

        this.ValidationRule = new UsernameValidationRule();

        this.UPDATE_HIT_BOX();
        this.UPDATE_CARET_IMMEDIATE();

        base.SetZIndex(RenderLayer.InputField.ToZIndex());
    }

    /// <summary>
    /// Creates a new <see cref="TextInputField"/> with default border thickness.
    /// </summary>
    /// <param name="panelTexture">9-slice texture.</param>
    /// <param name="sourceRect">Texture rect.</param>
    /// <param name="size">Panel size (will be clamped to minimal size by borders).</param>
    /// <param name="position">Top-left position.</param>
    /// <param name="font">SFML font to render text.</param>
    /// <param name="fontSize">Font size in points.</param>
    public TextInputField(
        Texture panelTexture,
        IntRect sourceRect,
        Vector2f size,
        Vector2f position,
        Font font = null,
        System.UInt32 fontSize = 16)
        : this(panelTexture, new Thickness(32), sourceRect, size, position, font, fontSize)
    {
    }

    /// <summary>
    /// Creates a new <see cref="TextInputField"/> with default border and source rect.
    /// </summary>
    /// <param name="panelTexture">9-slice texture.</param>
    /// <param name="size">Panel size (will be clamped to minimal size by borders).</param>
    /// <param name="position">Top-left position.</param>
    /// <param name="font">SFML font to render text.</param>
    /// <param name="fontSize">Font size in points.</param>
    public TextInputField(
        Texture panelTexture,
        Vector2f size,
        Vector2f position,
        Font font = null,
        System.UInt32 fontSize = 16)
        : this(panelTexture, new Thickness(32), default, size, position, font, fontSize)
    {
    }

    #endregion Construction

    #region Overrides

    /// <summary>
    /// Not used by engine (we render explicitly in <see cref="Draw"/>), but must be provided.
    /// </summary>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override IDrawable GetDrawable() => _text;

    /// <inheritdoc/>
    public void OnFocusGained()
    {
        this.Focused = true;
        _caretVisible = true;
        _caretTimer = 0f;
    }

    /// <inheritdoc/>
    public void OnFocusLost()
    {
        this.Focused = false;
        _caretVisible = false;
    }

    /// <inheritdoc/>
    public override void Update(System.Single dt)
    {
        this.HANDLE_FOCUS_INPUT();

        if (this.Focused)
        {
            this.UPDATE_CARET_BLINK(dt);
            this.HANDLE_KEY_INPUT(dt);
        }

        this.UPDATE_VISIBLE_TEXT();
        this.UPDATE_CARET_IMMEDIATE();
    }

    /// <inheritdoc/>
    public override void Draw(IRenderTarget target)
    {
        if (!this.IsVisible)
        {
            return;
        }

        _panel.Draw(target);
        target.Draw(_text);

        if (this.Focused && _caretVisible)
        {
            target.Draw(_caret);
        }
    }

    /// <summary>
    /// Returns what should be displayed: placeholder, masked password, or raw text.
    /// </summary>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected virtual System.String GetRenderText()
        => _buffer.Length == 0 && !this.Focused && !System.String.IsNullOrEmpty(this.Placeholder)
            ? this.Placeholder
            : _buffer.ToString();

    #endregion Overrides

    #region Private Methods - Input Handling

    /// <summary>
    /// Handles mouse click for focus management.
    /// </summary>
    private void HANDLE_FOCUS_INPUT()
    {
        if (!MouseManager.Instance.IsMouseButtonPressed(Mouse.Button.Left))
        {
            return;
        }

        Vector2i mp = MouseManager.Instance.GetMousePosition();

        if (_hitBox.Contains(mp))
        {
            System.Boolean wasFocused = this.Focused;
            FocusManager.Instance.RequestFocus(this);

            if (!wasFocused || this.Focused)
            {
                _caretTimer = 0f;
                _caretVisible = true;
            }
        }
        else
        {
            FocusManager.Instance.ClearFocus(this);
        }
    }

    /// <summary>
    /// Updates caret blink animation.
    /// </summary>
    private void UPDATE_CARET_BLINK(System.Single dt)
    {
        _caretTimer += dt;

        if (_caretTimer >= CaretBlinkPeriod)
        {
            _caretTimer = 0f;
            _caretVisible = !_caretVisible;
        }
    }

    /// <summary>
    /// Handles key presses and key repeats for Backspace/Delete.
    /// </summary>
    private void HANDLE_KEY_INPUT(System.Single dt)
    {
        System.Boolean shift = KeyboardManager.Instance.IsKeyDown(Keyboard.Key.LShift)
                             || KeyboardManager.Instance.IsKeyDown(Keyboard.Key.RShift);

        // Submit: Enter
        if (KeyboardManager.Instance.IsKeyPressed(Keyboard.Key.Enter))
        {
            this.TextSubmitted?.Invoke(_buffer.ToString());
        }

        // Letters A..Z
        if (_buffer.Length < (this.MaxLength ?? System.Int32.MaxValue))
        {
            System.Boolean mapped = KeyboardCharMapper.Instance.TryMapKeyToChar(out System.Char ch, shift);

            if (mapped)
            {
                this.TRY_INSERT_CHAR(ch);
            }
        }

        // Backspace with repeat
        if (_backspaceRepeat.Update(
                KeyboardManager.Instance.IsKeyDown(Keyboard.Key.Backspace),
                dt,
                KeyRepeatFirstDelay,
                KeyRepeatNextDelay))
        {
            this.BACKSPACE();
        }

        // Delete with repeat
        if (_deleteRepeat.Update(
                KeyboardManager.Instance.IsKeyDown(Keyboard.Key.Delete),
                dt,
                KeyRepeatFirstDelay,
                KeyRepeatNextDelay))
        {
            this.DELETE();
        }
    }

    /// <summary>
    /// Attempts to insert a character at the caret position if validation passes.
    /// </summary>
    private void TRY_INSERT_CHAR(System.Char ch)
    {
        System.String preview = _buffer.ToString().Insert(_caretIndex, ch.ToString());

        if (this.ValidationRule?.IsValid(preview) == false)
        {
            return;
        }

        this.APPEND_CHAR(ch);
    }

    #endregion Private Methods - Input Handling

    #region Private Methods - Text Manipulation

    /// <summary>
    /// Appends a character at the caret position with MaxLength enforcement and change notification.
    /// </summary>
    private void APPEND_CHAR(System.Char c)
    {
        if (this.MaxLength.HasValue && _buffer.Length >= this.MaxLength.Value)
        {
            return;
        }

        _ = _buffer.Insert(_caretIndex, c);
        _caretIndex++;

        this.TextChanged?.Invoke(_buffer.ToString());
    }

    /// <summary>
    /// Removes the character before the caret position, if any; raises <see cref="TextChanged"/>.
    /// </summary>
    private void BACKSPACE()
    {
        if (_caretIndex <= 0)
        {
            return;
        }

        _ = _buffer.Remove(_caretIndex - 1, 1);
        _caretIndex--;

        this.TextChanged?.Invoke(_buffer.ToString());
    }

    /// <summary>
    /// Deletes the character at the caret position, if any; raises <see cref="TextChanged"/>.
    /// </summary>
    private void DELETE()
    {
        if (_caretIndex >= _buffer.Length)
        {
            return;
        }

        _ = _buffer.Remove(_caretIndex, 1);
        this.TextChanged?.Invoke(_buffer.ToString());
    }

    /// <summary>
    /// Clamps the current text to <see cref="MaxLength"/> if needed.
    /// </summary>
    private void CLAMP_TO_MAX_LENGTH()
    {
        if (this.MaxLength.HasValue && _buffer.Length > this.MaxLength.Value)
        {
            _buffer.Length = this.MaxLength.Value;
        }
    }

    #endregion Private Methods - Text Manipulation

    #region Private Methods - Layout & Rendering

    /// <summary>
    /// Updates caret position/size immediately to the end of visible text.
    /// </summary>
    private void UPDATE_CARET_IMMEDIATE()
    {
        System.String visible = _text.DisplayedString;
        System.Int32 visibleCaret = System.Math.Clamp(_caretIndex - _scrollStart, 0, visible.Length);
        _measure.DisplayedString = visible;
        Vector2f caretLocalPos = _measure.FindCharacterPos((System.UInt32)visibleCaret);

        _caret.Size = new Vector2f(_caretWidth, _fontSize);
        _caret.Position = new Vector2f(_text.Position.X + caretLocalPos.X, _text.Position.Y + CaretYOffset);
    }

    /// <summary>
    /// Computes the portion of <see cref="GetRenderText"/> that fits into the inner width,
    /// ensuring the tail (caret at end) remains visible. Then assigns to <see cref="_text"/>.
    /// </summary>
    private void UPDATE_VISIBLE_TEXT()
    {
        System.String full = this.GetRenderText();
        _measure.DisplayedString = full;

        System.Single innerWidth = _panel.Size.X - (_padding.X * 2f) - _caretWidth;
        System.UInt32 n = (System.UInt32)full.Length;

        if (n == 0)
        {
            _scrollStart = 0;
            _text.DisplayedString = System.String.Empty;
            this.APPLY_TEXT_POSITION();
            return;
        }

        // If full text fits, reset scroll
        if (this.GET_TEXT_WIDTH(0, n) <= innerWidth)
        {
            _scrollStart = 0;
            _text.DisplayedString = full;
            this.APPLY_TEXT_POSITION();
            return;
        }

        // Ensure tail is visible
        while (this.GET_TEXT_WIDTH((System.UInt32)_scrollStart, n) > innerWidth && _scrollStart < full.Length)
        {
            _scrollStart++;
        }

        // Try to reveal more of the head if space allows
        while (_scrollStart > 0 && this.GET_TEXT_WIDTH((System.UInt32)(_scrollStart - 1), n) <= innerWidth)
        {
            _scrollStart--;
        }

        _text.DisplayedString = full[_scrollStart..];
        this.APPLY_TEXT_POSITION();
    }

    /// <summary>
    /// Calculates the width of a substring from index <paramref name="start"/> to <paramref name="end"/>.
    /// </summary>
    private System.Single GET_TEXT_WIDTH(System.UInt32 start, System.UInt32 end)
        => _measure.FindCharacterPos(end).X - _measure.FindCharacterPos(start).X;

    /// <summary>
    /// Repositions both measure and draw texts from panel position and padding.
    /// </summary>
    private void RELAYOUT_TEXT() => this.APPLY_TEXT_POSITION();

    /// <summary>
    /// Applies calculated text position based on panel position and padding.
    /// </summary>
    private void APPLY_TEXT_POSITION()
    {
        System.Single textY = _panel.Position.Y + ((_panel.Size.Y - _fontSize) / 2f) - CaretYOffset;
        System.Single textX = _panel.Position.X + _padding.X;

        _text.Position = new Vector2f(textX, textY);
        _measure.Position = _text.Position;
    }

    /// <summary>
    /// Recomputes the hit-box based on panel position and size.
    /// </summary>
    private void UPDATE_HIT_BOX() => _hitBox = new FloatRect(_panel.Size, _panel.Position);

    /// <summary>
    /// Resets scrolling window and caret visibility after large layout changes.
    /// </summary>
    private void RESET_SCROLL_AND_CARET()
    {
        _caretTimer = 0f;
        _scrollStart = 0;
        _caretVisible = true;
        this.UPDATE_VISIBLE_TEXT();
        this.UPDATE_CARET_IMMEDIATE();
    }

    /// <summary>
    /// Ensures panel size never violates border minimums.
    /// </summary>
    private static Vector2f ENSURE_MIN_SIZE(Vector2f size, Thickness border)
    {
        System.Single minW = border.Left + border.Right + MinSizeOffset;
        System.Single minH = border.Top + border.Bottom + MinSizeOffset;
        return new Vector2f(System.MathF.Max(size.X, minW), System.MathF.Max(size.Y, minH));
    }

    #endregion Private Methods - Layout & Rendering

    #region Nested Types

    /// <summary>
    /// Controls key repeat timing for keyboard input, supporting initial and repeated activation intervals.
    /// </summary>
    private sealed class KeyRepeatController
    {
        private System.Single _timer;
        private System.Boolean _repeating;

        /// <summary>
        /// Updates the state of the key repeat controller.
        /// </summary>
        /// <param name="isKeyDown">Indicates whether the key is currently pressed down.</param>
        /// <param name="dt">Elapsed time since the last update, in seconds.</param>
        /// <param name="firstDelay">Delay before the first repeat fires, in seconds.</param>
        /// <param name="repeatDelay">Delay between subsequent repeats, in seconds.</param>
        /// <returns>
        /// <c>true</c> if the key should be considered activated (either initially or as a repeat); otherwise, <c>false</c>.
        /// </returns>
        public System.Boolean Update(System.Boolean isKeyDown, System.Single dt, System.Single firstDelay, System.Single repeatDelay)
        {
            if (!isKeyDown)
            {
                _repeating = false;
                _timer = 0f;
                return false;
            }

            if (!_repeating)
            {
                _repeating = true;
                _timer = firstDelay;
                return true;
            }

            _timer -= dt;
            if (_timer <= 0f)
            {
                _timer = repeatDelay;
                return true;
            }

            return false;
        }
    }

    #endregion Nested Types
}
