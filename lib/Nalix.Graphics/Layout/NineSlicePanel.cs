// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Graphics.Entities;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Graphics.Layout;

/// <summary>
/// A lightweight and efficient 9-slice panel for crisp, scalable UI frames.
/// Corners are not scaled, edges only scale in one axis, and the center scales in both axes.
/// </summary>
[System.Diagnostics.DebuggerDisplay(
    "NineSlicePanel | Pos=({Position.X},{Position.Y}), Size=({Size.X},{Size.Y}), Border={Border}")]
public sealed class NineSlicePanel : RenderObject
{
    #region Fields

    private System.Boolean _dirty;
    private readonly Sprite[] _parts;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the texture used for rendering the panel.
    /// </summary>
    public Texture Texture { get; }

    /// <summary>
    /// Gets the overall size (in pixels) of the panel.
    /// </summary>
    public Vector2f Size { get; private set; }

    /// <summary>
    /// Gets the border thickness (in source pixels) for each edge.
    /// </summary>
    public Thickness Border { get; private set; }

    /// <summary>
    /// Gets the current position of the panel (top-left corner) in screen coordinates.
    /// </summary>
    public Vector2f Position { get; private set; }

    /// <summary>
    /// Gets the source rectangle used on the texture for slicing.
    /// </summary>
    public IntRect SourceRect { get; private set; }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new <see cref="NineSlicePanel"/> instance.
    /// </summary>
    /// <param name="texture">The source texture for the panel.</param>
    /// <param name="border">
    /// The thickness of each border in source pixels (left, top, right, bottom).
    /// Determines the size of corners and edges.
    /// </param>
    /// <param name="sourceRect">
    /// (Optional) The rectangle region in the texture to use. If not set, uses the entire texture.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown if <paramref name="texture"/> is null.
    /// </exception>
    public NineSlicePanel(Texture texture, Thickness border, IntRect sourceRect = default)
    {
        this.Texture = texture ?? throw new System.ArgumentNullException(nameof(texture));
        this.Border = border;
        this.SourceRect = sourceRect == default
            ? new IntRect(new Vector2i(0, 0), new Vector2i((System.Int32)texture.Size.X, (System.Int32)texture.Size.Y))
            : sourceRect;

        _dirty = true;
        _parts = new Sprite[9];

        for (System.Int32 i = 0; i < 9; i++)
        {
            _parts[i] = new Sprite(Texture);
        }

        this.Position = default;
        this.Size = new Vector2f(SourceRect.Width, SourceRect.Height);
    }

    #endregion Constructor

    #region Fluent Setters

    /// <summary>
    /// Sets the top-left position of the panel in screen coordinates.
    /// </summary>
    /// <param name="pos">The new position of the panel.</param>
    /// <returns>This panel instance for fluent chaining.</returns>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public NineSlicePanel SetPosition(Vector2f pos)
    {
        if (pos != Position)
        {
            Position = pos;
            _dirty = true;
        }
        return this;
    }

    /// <summary>
    /// Sets the desired size of the panel in pixels.
    /// </summary>
    /// <param name="size">The new size, in pixels.</param>
    /// <returns>This panel instance for fluent chaining.</returns>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public NineSlicePanel SetSize(Vector2f size)
    {
        if (size != Size)
        {
            Size = size;
            _dirty = true;
        }
        return this;
    }

    /// <summary>
    /// Sets the source rectangle in texture coordinates for 9-slice slicing.
    /// </summary>
    /// <param name="rect">The new source region on the texture.</param>
    /// <returns>This panel instance for fluent chaining.</returns>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public NineSlicePanel SetSourceRect(IntRect rect)
    {
        if (rect != SourceRect)
        {
            SourceRect = rect;
            _dirty = true;
        }
        return this;
    }

    /// <summary>
    /// Sets the border thickness used for slicing.
    /// </summary>
    /// <param name="border">The new border thickness.</param>
    /// <returns>This panel instance for fluent chaining.</returns>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public NineSlicePanel SetBorder(Thickness border)
    {
        if (!border.Equals(Border))
        {
            Border = border;
            _dirty = true;
        }
        return this;
    }

    /// <summary>
    /// Sets the tint color applied to all 9 slices of the panel.
    /// </summary>
    /// <param name="color">The tint color to apply.</param>
    /// <returns>This panel instance for fluent chaining.</returns>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public NineSlicePanel SetTintColor(Color color)
    {
        for (System.Int32 i = 0; i < _parts.Length; i++)
        {
            _parts[i].Color = color;
        }
        // Tint does not require relayout, so _dirty not set.
        return this;
    }

    /// <summary>
    /// Gets the current tint color of all slices (returns color of the top-left slice).
    /// </summary>
    /// <returns>The current tint color.</returns>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public Color GetTintColor() => _parts.Length > 0 ? _parts[0].Color : Color.White;

    #endregion Fluent Setters

    #region Overrides

    /// <summary>
    /// Draws the panel to the given render target.
    /// </summary>
    /// <param name="target">The render target (e.g. window, texture) to draw to.</param>
    public override void Draw(IRenderTarget target)
    {
        this.COMPUTE_RECTS();

        for (System.Int32 i = 0; i < 9; i++)
        {
            target.Draw(_parts[i]);
        }
    }

    /// <summary>
    /// Returns a <see cref="Drawable"/> for advanced rendering scenarios.
    /// Not supported for <see cref="NineSlicePanel"/>.
    /// </summary>
    /// <returns>None. This panel must be drawn via <see cref="Draw(RenderTarget)"/>.</returns>
    /// <exception cref="System.NotImplementedException">Always thrown.</exception>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override IDrawable GetDrawable()
        => throw new System.NotImplementedException("Use Draw(RenderTarget) instead.");

    #endregion Overrides

    #region Private Layout Logic

    /// <summary>
    /// Recomputes geometry and positioning for all 9 slices based on current state.
    /// </summary>
    private void COMPUTE_RECTS()
    {
        if (!_dirty)
        {
            return;
        }

        // Clamp to minimum size (border sum + 1) so center area never collapses.
        System.Int32 L = Border.Left, T = Border.Top, R = Border.Right, B = Border.Bottom;
        System.Single minW = L + R + 1, minH = T + B + 1;

        System.Single x = (System.Single)System.Math.Round(Position.X);
        System.Single y = (System.Single)System.Math.Round(Position.Y);
        System.Single w = (System.Single)System.Math.Round(System.Math.Max(Size.X, minW));
        System.Single h = (System.Single)System.Math.Round(System.Math.Max(Size.Y, minH));

        System.Int32 sx = SourceRect.Left, sy = SourceRect.Top, sw = SourceRect.Width, sh = SourceRect.Height;

        // Define source rectangles (texture space) for each 9-slice region.
        // Corners
        IntRect[] src = new IntRect[9];
        src[0] = new IntRect(new Vector2i(sx, sy), new Vector2i(L, T));                           // Top-left
        src[2] = new IntRect(new Vector2i(sx + sw - R, sy), new Vector2i(R, T));                  // Top-right
        src[6] = new IntRect(new Vector2i(sx, sy + sh - B), new Vector2i(L, B));                  // Bottom-left
        src[8] = new IntRect(new Vector2i(sx + sw - R, sy + sh - B), new Vector2i(R, B));         // Bottom-right
        // Edges
        src[1] = new IntRect(new Vector2i(sx + L, sy), new Vector2i(sw - L - R, T));              // Top edge
        src[3] = new IntRect(new Vector2i(sx, sy + T), new Vector2i(L, sh - T - B));              // Left edge
        src[5] = new IntRect(new Vector2i(sx + sw - R, sy + T), new Vector2i(R, sh - T - B));     // Right edge
        src[7] = new IntRect(new Vector2i(sx + L, sy + sh - B), new Vector2i(sw - L - R, B));     // Bottom edge
        // Center
        src[4] = new IntRect(new Vector2i(sx + L, sy + T), new Vector2i(sw - L - R, sh - T - B)); // Center

        // Target rectangles (UI space): corners fixed size, edges/center stretch as needed.
        System.Single Lw = L, Tw = T, Rw = R, Bw = B;
        System.Single centerW = w - Lw - Rw;
        System.Single centerH = h - Tw - Bw;

        FloatRect[] dst = new FloatRect[9];
        // Corners
        dst[0] = new FloatRect(new Vector2f(x, y), new Vector2f(Lw, Tw));
        dst[2] = new FloatRect(new Vector2f(x + w - Rw, y), new Vector2f(Rw, Tw));
        dst[6] = new FloatRect(new Vector2f(x, y + h - Bw), new Vector2f(Lw, Bw));
        dst[8] = new FloatRect(new Vector2f(x + w - Rw, y + h - Bw), new Vector2f(Rw, Bw));
        // Edges
        dst[1] = new FloatRect(new Vector2f(x + Lw, y), new Vector2f(centerW, Tw));               // Top edge (stretch X)
        dst[3] = new FloatRect(new Vector2f(x, y + Tw), new Vector2f(Lw, centerH));               // Left edge (stretch Y)
        dst[5] = new FloatRect(new Vector2f(x + w - Rw, y + Tw), new Vector2f(Rw, centerH));      // Right edge (stretch Y)
        dst[7] = new FloatRect(new Vector2f(x + Lw, y + h - Bw), new Vector2f(centerW, Bw));      // Bottom edge (stretch X)
        // Center
        dst[4] = new FloatRect(new Vector2f(x + Lw, y + Tw), new Vector2f(centerW, centerH));

        // Apply calculated geometry to each region.
        for (System.Int32 i = 0; i < 9; i++)
        {
            // Clamp region widths/heights to be non-negative to prevent invalid transforms.
            System.Single dw = System.Math.Max(0f, dst[i].Width);
            System.Single dh = System.Math.Max(0f, dst[i].Height);
            System.Single swp = System.Math.Max(1f, src[i].Width);
            System.Single shp = System.Math.Max(1f, src[i].Height);

            Sprite s = _parts[i];
            s.TextureRect = src[i];
            s.Scale = new Vector2f(dw / swp, dh / shp);
            s.Position = new Vector2f(dst[i].Left, dst[i].Top);
        }

        _dirty = false;
    }

    #endregion Private Layout Logic
}
