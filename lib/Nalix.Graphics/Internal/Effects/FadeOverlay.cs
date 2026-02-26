// Copyright (c) 2026 PPN Corporation. All rights reserved.

using SFML.Graphics;

namespace Nalix.Graphics.Internal.Effects;

/// <summary>
/// Overlay effect that produces a fade in/out by gradually adjusting overlay alpha from 0→255 then 255→0.
/// (VN) Hiệu ứng overlay chuyển alpha từ trong suốt sang kín (và ngược lại).
/// </summary>
internal sealed class FadeOverlay : ScreenOverlayBase
{
    private readonly RectangleShape _rect;

    /// <summary>
    /// Initializes a new fade overlay with the specified color.
    /// </summary>
    /// <param name="color">Base overlay color (RGB component, alpha will be controlled dynamically).</param>
    public FadeOverlay(Color color)
        : base(color)
    {
        _rect = new RectangleShape(Size)
        {
            FillColor = new Color(color.R, color.G, color.B, 0)
        };
    }

    /// <summary>
    /// Updates the overlay alpha for fade effect.
    /// </summary>
    /// <param name="t">
    /// Progress [0..1] of the current half (0: start, 1: end of phase).
    /// </param>
    /// <param name="closing">True if in closing (fade-in black), false if opening (fade-out black).</param>
    public override void Update(System.Single t, System.Boolean closing)
    {
        // Clamp progress to [0,1] để tránh giá trị alpha bất hợp lệ
        t = System.Single.Clamp(t, 0f, 1f);
        System.Byte a = closing ? (System.Byte)System.Math.Round(255f * t) : (System.Byte)System.Math.Round(255f * (1f - t));
        _rect.FillColor = new Color(BaseColor.R, BaseColor.G, BaseColor.B, a);
    }

    /// <summary>
    /// Gets the drawable overlay rectangle for the current frame.
    /// </summary>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public override IDrawable GetDrawable() => _rect;
}
