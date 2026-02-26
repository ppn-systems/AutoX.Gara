// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Graphics.Abstractions;
using Nalix.Graphics.Entities;
using Nalix.Graphics.Input;
using Nalix.Graphics.UI.Theme;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Nalix.Graphics.UI.Controls;

/// <summary>
/// A simple reusable vertical scrollbar control.
/// Value is normalized 0..1. ViewportRatio is viewportHeight / contentHeight (0..1).
/// </summary>
public sealed class ScrollBar : RenderObject, IUpdatable
{
    #region Fields

    private readonly RectangleShape _track;
    private readonly RectangleShape _thumb;

    private System.Boolean _wasMouseDown = false;

    private Vector2f _position;
    private Vector2f _size = new(12f, 100f);

    private System.Boolean _thumbDragging;
    private System.Single _dragOffsetY;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Normalized value 0..1, where 0 = top, 1 = bottom.
    /// </summary>
    public System.Single Value { get; private set; } = 0f;

    /// <summary>
    /// Ratio of viewportHeight/contentHeight (controls thumb size). Range 0..1.
    /// </summary>
    public System.Single ViewportRatio { get; private set; } = 1f;

    /// <summary>
    /// Minimum usable thumb height.
    /// </summary>
    public System.Single MinThumbHeight { get; set; } = 18f;

    /// <summary>
    /// Whether to render the scrollbar. You can hide it when not needed.
    /// </summary>
    public System.Boolean IsVisibleForRender { get; set; } = true;

    /// <summary>
    /// Fired when Value changes by user interaction (or programmatically if raiseEvent true).
    /// Parameter: new normalized value (0..1).
    /// </summary>
    public event System.Action<System.Single> ValueChanged;

    /// <summary>
    /// Top-left position.
    /// </summary>
    public Vector2f Position
    {
        get => _position;
        set
        {
            if (_position != value)
            {
                _position = value;
                UPDATE_SHAPES();
            }
        }
    }

    /// <summary>
    /// Size (width,height) of the track.
    /// </summary>
    public Vector2f Size
    {
        get => _size;
        set
        {
            if (_size != value)
            {
                _size = value;
                UPDATE_SHAPES();
            }
        }
    }

    /// <summary>
    /// The step applied to Value for each mouse-wheel notch when calling OnMouseWheel.
    /// It is in normalized coordinates (0..1). Default is 0.05 (5%).
    /// </summary>
    public System.Single WheelStep { get; set; } = 0.05f;

    /// <summary>
    /// If true, wheel events will only be applied when the mouse cursor is over the track area.
    /// </summary>
    public System.Boolean WheelRequiresHover { get; set; } = true;

    #endregion Properties

    #region Constructor

    public ScrollBar()
    {
        _track = new RectangleShape(new Vector2f(_size.X, _size.Y))
        {
            FillColor = WITH_ALPHA(Themes.DataGridHeaderBackgroundColor, 200)
        };
        _thumb = new RectangleShape(new Vector2f(_size.X, System.MathF.Max(MinThumbHeight, _size.Y * ViewportRatio)))
        {
            FillColor = Themes.DataGridRowHoverColor
        };

        UPDATE_SHAPES();
    }

    #endregion Constructor

    #region Public Methods

    /// <summary>
    /// Set the normalized value (0..1). Optionally prevent raising ValueChanged.
    /// </summary>
    public void SetValue(System.Single value, System.Boolean raiseEvent = true)
    {
        System.Single v = System.Math.Clamp(value, 0f, 1f);
        if (System.Math.Abs(v - Value) > 0.0001f)
        {
            Value = v;
            UPDATE_THUMB_POSITION_FROM_VALUE();
            if (raiseEvent)
            {
                ValueChanged?.Invoke(Value);
            }
        }
        else
        {
            // still update thumb position to be safe
            UPDATE_THUMB_POSITION_FROM_VALUE();
        }
    }

    /// <summary>
    /// Update viewport ratio (0..1) and recompute thumb size/position.
    /// </summary>
    public void SetViewportRatio(System.Single viewportRatio)
    {
        ViewportRatio = System.Math.Clamp(viewportRatio, 0f, 1f);
        UPDATE_SHAPES();
    }

    /// <summary>
    /// Convert normalized Value to absolute offset given contentHeight and viewportHeight.
    /// </summary>
    public System.Single ValueToOffset(System.Single contentHeight, System.Single viewportHeight)
    {
        System.Single maxOffset = System.MathF.Max(0f, contentHeight - viewportHeight);
        return Value * maxOffset;
    }

    /// <summary>
    /// Set Value from absolute offset and content/view sizes.
    /// </summary>
    public void OffsetToValue(System.Single offset, System.Single contentHeight, System.Single viewportHeight)
    {
        System.Single maxOffset = System.MathF.Max(0f, contentHeight - viewportHeight);
        SetValue(maxOffset <= 0f ? 0f : offset / maxOffset);
    }

    /// <summary>
    /// Handle a wheel delta expressed as normalized fraction of content (optional).
    /// Backwards compatible method that directly applies ratioDelta.
    /// </summary>
    public void OnMouseWheelDelta(System.Single ratioDelta) => SetValue(Value - ratioDelta, raiseEvent: true);

    /// <summary>
    /// Convenient method to be called from window.MouseWheelScrolled handler.
    /// delta is SFML wheel delta (typically +1/-1). The method maps it to normalized Value changes using WheelStep.
    /// If WheelRequiresHover is true, this call will only change Value when mouse is over the track.
    /// </summary>
    public void OnMouseWheel(System.Single delta)
    {
        // check hover if required
        if (WheelRequiresHover)
        {
            Vector2i mp = MouseManager.Instance.GetMousePosition();
            var trackBounds = new FloatRect(_track.Position, _track.Size);
            if (!trackBounds.Contains(mp))
            {
                return;
            }
        }

        // Apply change: positive delta should move content up (consistent with typical UI behavior).
        // We follow existing convention: SetValue(Value - delta * WheelStep)
        System.Single newValue = Value - (delta * WheelStep);
        SetValue(newValue, raiseEvent: true);
    }

    public override void Update(System.Single dt)
    {
        Vector2i mpos = MouseManager.Instance.GetMousePosition();
        System.Boolean isDown = Mouse.IsButtonPressed(Mouse.Button.Left);
        FloatRect thumbBounds = new(_thumb.Position, _thumb.Size);
        FloatRect trackBounds = new(_track.Position, _track.Size);

        if (!_thumbDragging)
        {
            if (!isDown && _thumbDragging)
            {
                _thumbDragging = false;
            }

            if (!isDown)
            {
                // start drag if pressed on thumb this frame
                if (thumbBounds.Contains(mpos) && isDown)
                {
                    _thumbDragging = true;
                    _dragOffsetY = mpos.Y - _thumb.Position.Y;
                }
            }

            // click on track (not on thumb) -> move thumb center to cursor
            if (!isDown && trackBounds.Contains(mpos) && !thumbBounds.Contains(mpos) && isDown)
            {
                // handled above but keeping consistent (rare due to immediate state)
            }

            // Note: we intentionally check edge transitions in caller UI if needed.
        }
        // Simpler and more robust approach: react to mouse down events here using previous frame state
        // We'll use MouseManager's state via last frame press detection in caller if necessary.
        // For simplicity we implement basic dragging detection using isDown and thumbBounds and previous frame.

        // Low-level drag handling:
        if (!_thumbDragging)
        {
            // start dragging when mouse just pressed on thumb
            if (isDown && thumbBounds.Contains(mpos) && !_wasMouseDown)
            {
                _thumbDragging = true;
                _dragOffsetY = mpos.Y - _thumb.Position.Y;
            }
            else if (isDown && trackBounds.Contains(mpos) && !_wasMouseDown && !thumbBounds.Contains(mpos))
            {
                // click on track: move thumb center to mouse
                System.Single half = _thumb.Size.Y * 0.5f;
                System.Single newY = mpos.Y - _track.Position.Y - half;
                System.Single max = _track.Size.Y - _thumb.Size.Y;
                System.Single clamped = System.Math.Clamp(newY, 0f, System.MathF.Max(0f, max));
                _thumb.Position = new Vector2f(_thumb.Position.X, _track.Position.Y + clamped);
                UPDATE_VALUE_FROM_THUMB_POSITION();
            }
        }
        else
        {
            if (!isDown)
            {
                _thumbDragging = false;
            }
            else
            {
                System.Single y = mpos.Y - _dragOffsetY;
                System.Single rel = y - _track.Position.Y;
                System.Single max = System.MathF.Max(0f, _track.Size.Y - _thumb.Size.Y);
                System.Single clamped = System.Math.Clamp(rel, 0f, max);
                _thumb.Position = new Vector2f(_thumb.Position.X, _track.Position.Y + clamped);
                UPDATE_VALUE_FROM_THUMB_POSITION();
            }
        }

        _wasMouseDown = isDown;
    }

    public override void Draw(IRenderTarget target)
    {
        if (!this.IsVisible || !IsVisibleForRender)
        {
            return;
        }

        target.Draw(_track);
        target.Draw(_thumb);
    }

    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override IDrawable GetDrawable() => throw new System.NotSupportedException("ScrollBar uses Draw(IRenderTarget).");

    #endregion Public Methods

    #region Private Methods

    private void UPDATE_SHAPES()
    {
        _track.Size = _size;
        _track.Position = _position;

        System.Single thumbH = System.MathF.Max(MinThumbHeight, _size.Y * ViewportRatio);
        _thumb.Size = new Vector2f(_size.X, thumbH);

        UPDATE_THUMB_POSITION_FROM_VALUE();
    }

    private static Color WITH_ALPHA(Color c, System.Byte a) => new(c.R, c.G, c.B, a);

    private void UPDATE_THUMB_POSITION_FROM_VALUE()
    {
        System.Single available = System.MathF.Max(0f, _track.Size.Y - _thumb.Size.Y);
        System.Single t = System.Math.Clamp(Value, 0f, 1f);
        _thumb.Position = new Vector2f(_track.Position.X, _track.Position.Y + (available * t));
    }

    private void UPDATE_VALUE_FROM_THUMB_POSITION()
    {
        System.Single available = System.MathF.Max(0f, _track.Size.Y - _thumb.Size.Y);
        if (available <= 0f)
        {
            SetValue(0f);
            return;
        }

        System.Single rel = _thumb.Position.Y - _track.Position.Y;
        rel = System.Math.Clamp(rel, 0f, available);
        System.Single t = rel / available;
        SetValue(t); // raises event
    }

    #endregion Private Methods
}