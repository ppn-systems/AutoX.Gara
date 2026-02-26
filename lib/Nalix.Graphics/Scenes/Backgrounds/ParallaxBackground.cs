// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Graphics.Abstractions;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Graphics.Scenes.Backgrounds;

/// <summary>
/// Provides parallax scrolling functionality by managing multiple background layers with varying scroll speeds.
/// </summary>
public class ParallaxBackground(Vector2u viewport) : IUpdatable
{
    #region Fields

    private readonly Vector2u _viewport = viewport;
    private readonly System.Collections.Generic.List<Layer> _layers = [];

    #endregion Fields

    #region Construction

    /// <summary>
    /// Initializes a new instance of the <see cref="ParallaxBackground"/> class with the specified viewport size.
    /// </summary>
    public ParallaxBackground(System.UInt32 width, System.UInt32 height)
        : this(new Vector2u(width, height))
    {
    }

    #endregion Construction

    #region APIs

    /// <summary>
    /// Adds a new layer to the parallax system.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void AddBackgroundLayer(Texture texture, System.Single speed, System.Boolean autoScale)
    {
        System.ArgumentNullException.ThrowIfNull(texture);

        if (texture.Size.X == 0 || texture.Size.Y == 0)
        {
            throw new System.ArgumentException("Texture must have nonzero size.", nameof(texture));
        }

        _layers.Add(new Layer(_viewport, texture, speed, autoScale));
    }

    /// <summary>
    /// Removes all background layers.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void ClearBackgroundLayers() => _layers.Clear();

    /// <summary>
    /// Updates the parallax scrolling based on elapsed time.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void Update(System.Single deltaTime)
    {
        foreach (Layer layer in _layers)
        {
            layer.Offset += layer.Speed * deltaTime;

            // Wrap offset to avoid overflow
            System.Single textureWidth = layer.Texture.Size.X;
            if (textureWidth > 0)
            {
                layer.Offset %= textureWidth;
            }

            System.Int32 left = (System.Int32)layer.Offset;
            layer.Rect = new IntRect(new Vector2i(left, 0), new Vector2i((System.Int32)_viewport.X, (System.Int32)_viewport.Y));
            layer.Sprite.TextureRect = layer.Rect;
        }
    }

    /// <summary>
    /// Draws all layers to the specified render target.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void Draw(IRenderTarget target)
    {
        foreach (Layer layer in _layers)
        {
            target.Draw(layer.Sprite);
        }
    }

    #endregion APIs

    #region Private Classes

    private class Layer
    {
        public IntRect Rect;
        public Sprite Sprite { get; }
        public Texture Texture { get; }

        public System.Single Speed { get; }
        public System.Single Offset { get; set; }

        public Layer(
            Vector2u viewport, Texture texture,
            System.Single speed, System.Boolean autoScale = false)
        {
            this.Texture = texture;
            this.Speed = speed;
            this.Offset = 0;

            this.Texture.Repeated = true;
            this.Rect = new IntRect(new Vector2i(0, 0), new Vector2i((System.Int32)viewport.X, (System.Int32)viewport.Y));
            this.Sprite = new Sprite(Texture) { TextureRect = Rect };

            if (autoScale)
            {
                System.Single scaleX = (System.Single)viewport.X / texture.Size.X;
                System.Single scaleY = (System.Single)viewport.Y / texture.Size.Y;
                this.Sprite.Scale = new(scaleX, scaleY);
            }
        }
    }

    #endregion Private Classes
}
