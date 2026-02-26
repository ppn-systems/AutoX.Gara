// Copyright (c) 2026 PPN Corporation. All rights reserved.

using SFML.Graphics;
using SFML.System;

namespace Nalix.Graphics.Internal.Effects;

/// <summary>
/// Zoom overlay effect: a colored rectangle scales from the center (ZoomIn: 0→1 then 1→0; ZoomOut: 1→0 then 0→1).
/// (VN) Hiệu ứng zoom phủ: hình chữ nhật co/phóng từ tâm màn hình.
/// </summary>
internal sealed class ZoomOverlay : ScreenOverlayBase
{
    private readonly RectangleShape _rect;
    private readonly System.Boolean _modeIn; // true: ZoomIn, false: ZoomOut

    /// <summary>
    /// Initializes the zoom overlay.
    /// </summary>
    /// <param name="color">Overlay color.</param>
    /// <param name="modeIn">true for ZoomIn, false for ZoomOut.</param>
    public ZoomOverlay(Color color, System.Boolean modeIn)
        : base(color)
    {
        _modeIn = modeIn;
        _rect = new RectangleShape(Size)
        {
            Origin = base.Size / 2f,
            Position = base.Size / 2f,
            FillColor = new Color(color.R, color.G, color.B, 255)
        };
    }

    /// <summary>
    /// Updates rectangle scale for the current frame.
    /// </summary>
    /// <param name="t">Phase progress [0..1].</param>
    /// <param name="closing">True for closing (cover), false for opening (reveal).</param>
    public override void Update(System.Single t, System.Boolean closing)
    {
        // For ZoomIn: closing scales 0→1; opening scales 1→0
        // For ZoomOut: closing scales 1→0; opening scales 0→1
        System.Single s = _modeIn
            ? (closing ? t : 1f - t)
            : (closing ? 1f - t : t);

        // Avoid 0 to ensure still visible; Clamp t into [0.0001, 1f]
        s = System.Single.Clamp(s, 0.0001f, 1f);

        _rect.Scale = new Vector2f(s, s);
    }

    /// <summary>
    /// Gets the zoom overlay rectangle for the current frame.
    /// </summary>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public override IDrawable GetDrawable() => _rect;
}
