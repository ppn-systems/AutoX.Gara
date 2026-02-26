// Copyright (c) 2026 PPN Corporation. All rights reserved.

using SFML.Graphics;
using SFML.System;

namespace Nalix.Graphics.Internal.Effects;

/// <summary>
/// Vertical wipe overlay: covers the screen from top to bottom, then uncovers in reverse.
/// (VN) Che màn hình từ trên xuống dưới, đổi cảnh, rồi mở lại từ dưới lên trên.
/// </summary>
internal sealed class WipeOverlayVertical : ScreenOverlayBase
{
    private readonly RectangleShape _rect;

    /// <summary>
    /// Initializes the vertical wipe overlay with the specified color.
    /// </summary>
    /// <param name="color">Wipe color.</param>
    public WipeOverlayVertical(Color color)
        : base(color)
    {
        _rect = new RectangleShape(new Vector2f(Size.X, 0))
        {
            FillColor = new Color(color.R, color.G, color.B, 255),
            Position = new Vector2f(0, 0)
        };
    }

    /// <summary>
    /// Updates the wipe height for the current frame.
    /// </summary>
    /// <param name="t">Phase progress [0..1].</param>
    /// <param name="closing">True for covering, false for uncovering.</param>
    public override void Update(System.Single t, System.Boolean closing)
    {
        t = System.Single.Clamp(t, 0f, 1f);

        System.Single h = closing
            ? Size.Y * t
            : Size.Y * (1f - t);

        _rect.Size = new Vector2f(Size.X, System.MathF.Max(0f, h));
    }

    /// <summary>
    /// Gets the wipe overlay rectangle covering the screen vertically.
    /// </summary>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public override IDrawable GetDrawable() => _rect;
}
