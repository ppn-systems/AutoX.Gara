// Copyright (c) 2026 PPN Corporation. All rights reserved.

using SFML.Graphics;
using SFML.System;

namespace Nalix.Graphics.Internal.Effects;

/// <summary>
/// Overlay effect: a solid cover slides inwards (closing) and slides out (opening) from a screen edge.
/// (VN) Hiệu ứng phủ trượt: tấm màu trượt vào giữa, đổi cảnh, rồi trượt ra.
/// </summary>
internal sealed class SlideCoverOverlay : ScreenOverlayBase
{
    private readonly RectangleShape _rect;
    private readonly System.Boolean _fromLeft;

    /// <summary>
    /// Initializes a slide cover overlay.
    /// </summary>
    /// <param name="color">Overlay cover color.</param>
    /// <param name="fromLeft">
    /// If <c>true</c>, slide from left edge. If <c>false</c>, slide from right edge.
    /// </param>
    public SlideCoverOverlay(Color color, System.Boolean fromLeft)
        : base(color)
    {
        _fromLeft = fromLeft;
        _rect = new RectangleShape(Size)
        {
            FillColor = new Color(color.R, color.G, color.B, 255)
        };
    }

    /// <summary>
    /// Updates the sliding cover position for the current frame.
    /// </summary>
    /// <param name="t">Phase progress [0..1].</param>
    /// <param name="closing">True for sliding-in (cover), false for sliding-out (reveal).</param>
    public override void Update(System.Single t, System.Boolean closing)
    {
        // Clamp progress
        t = System.Single.Clamp(t, 0f, 1f);

        System.Single travel = base.Size.X;
        System.Single slideT = closing ? t : 1f - t;

        // Compute the X position so the cover moves across the whole screen horizontally
        System.Single x = _fromLeft ? -travel + (travel * slideT) : Size.X - (travel * slideT);

        _rect.Position = new Vector2f(x, 0f);
    }

    /// <summary>
    /// Gets the sliding cover overlay rectangle.
    /// </summary>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public override IDrawable GetDrawable() => _rect;
}
