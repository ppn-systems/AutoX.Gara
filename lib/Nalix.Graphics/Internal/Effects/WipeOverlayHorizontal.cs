// Copyright (c) 2026 PPN Corporation. All rights reserved.

using SFML.Graphics;
using SFML.System;

namespace Nalix.Graphics.Internal.Effects;

/// <summary>
/// Horizontal wipe overlay: covers the screen from left to right, then uncovers in reverse.
/// (VN) Che màn hình từ trái qua phải, đổi scene, rồi mở lại từ phải sang trái.
/// </summary>
internal sealed class WipeOverlayHorizontal : ScreenOverlayBase
{
    private readonly RectangleShape _rect;

    /// <summary>
    /// Initializes the horizontal wipe overlay with the specified color.
    /// </summary>
    /// <param name="color">Wipe color.</param>
    public WipeOverlayHorizontal(Color color)
        : base(color)
    {
        _rect = new RectangleShape(new Vector2f(0, Size.Y))
        {
            FillColor = new Color(color.R, color.G, color.B, 255),
            Position = new Vector2f(0, 0)
        };
    }

    /// <summary>
    /// Updates the wipe width for the current frame.
    /// </summary>
    /// <param name="t">Phase progress [0..1].</param>
    /// <param name="closing">True for covering, false for uncovering.</param>
    public override void Update(System.Single t, System.Boolean closing)
    {
        t = System.Single.Clamp(t, 0f, 1f); // Clamp progress for safe sizing

        System.Single w = closing
            ? Size.X * t
            : Size.X * (1f - t);

        _rect.Size = new Vector2f(System.MathF.Max(0f, w), Size.Y);
    }

    /// <summary>
    /// Gets the wipe overlay rectangle covering the screen.
    /// </summary>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public override IDrawable GetDrawable() => _rect;
}
