// Copyright (c) 2026 PPN Corporation. All rights reserved.

using SFML.Graphics;
using SFML.System;

namespace Nalix.Graphics.Entities;

/// <summary>
/// Represents an abstract base class for objects that render a Sprite.
/// Provides constructors for configuring the appearance and transformation of the Sprite.
/// </summary>
[System.Diagnostics.DebuggerDisplay("SpriteObject | Position={Sprite.Position}, Scale={Sprite.Scale}, Rotation={Sprite.Rotation}")]
public abstract class SpriteObject : RenderObject
{
    #region Properties

    /// <summary>
    /// The Sprite associated with this object.
    /// </summary>
    protected Sprite Sprite;

    /// <summary>
    /// Gets the global bounds of the Sprite.
    /// </summary>
    public virtual FloatRect GlobalBounds => this.Sprite.GetGlobalBounds();

    #endregion Properties

    #region Constructions

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteObject"/> class with a texture, rectangle, position, scale, and rotation.
    /// </summary>
    /// <param name="texture">The texture to be used for the Sprite.</param>
    /// <param name="rect">ScreenSize rectangle defining a subregion of the texture.</param>
    /// <param name="position">The position of the Sprite.</param>
    /// <param name="scale">The scale of the Sprite.</param>
    /// <param name="rotation">The rotation angle of the Sprite in degrees.</param>
    protected SpriteObject(
        Texture texture,
        IntRect rect,
        Vector2f position,
        Vector2f scale,
        System.Single rotation)
    {
        this.Sprite = new Sprite(texture, rect);
        APPLY_TRANSFORM(ref this.Sprite, position, scale, rotation);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteObject"/> class with a texture, position, scale, and rotation.
    /// </summary>
    /// <param name="texture">The texture to be used for the Sprite.</param>
    /// <param name="position">The position of the Sprite.</param>
    /// <param name="scale">The scale of the Sprite.</param>
    /// <param name="rotation">The rotation angle of the Sprite in degrees.</param>
    protected SpriteObject(
        Texture texture,
        Vector2f position,
        Vector2f scale,
        System.Single rotation)
    {
        this.Sprite = new Sprite(texture);
        APPLY_TRANSFORM(ref this.Sprite, position, scale, rotation);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteObject"/> class with a texture and rectangle.
    /// </summary>
    /// <param name="texture">The texture to be used for the Sprite.</param>
    /// <param name="rect">ScreenSize rectangle defining a subregion of the texture.</param>
    protected SpriteObject(Texture texture, IntRect rect)
    {
        Sprite = new Sprite(texture, rect);
        APPLY_TRANSFORM(ref Sprite, new Vector2f(0f, 0f), new Vector2f(1f, 1f), 0f);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteObject"/> class with a texture.
    /// </summary>
    /// <param name="texture">The texture to be used for the Sprite.</param>
    protected SpriteObject(Texture texture)
    {
        this.Sprite = new Sprite(texture);
        APPLY_TRANSFORM(ref this.Sprite, new Vector2f(0f, 0f), new Vector2f(1f, 1f), 0f);
    }

    #endregion Constructions

    #region APIs

    /// <summary>
    /// Gets the drawable object for rendering the Sprite.
    /// </summary>
    /// <returns>The Sprite as a drawable object.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected sealed override IDrawable GetDrawable() => this.Sprite;

    #endregion APIs

    #region Private Methods

    /// <summary>
    /// Sets the transformation properties of a Sprite.
    /// </summary>
    /// <param name="s">The Sprite to transform.</param>
    /// <param name="position">The position of the Sprite.</param>
    /// <param name="scale">The scale of the Sprite.</param>
    /// <param name="rotation">The rotation angle of the Sprite in degrees.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static void APPLY_TRANSFORM(ref Sprite s, Vector2f position, Vector2f scale, System.Single rotation)
    {
        s.Scale = scale;
        s.Position = position;
        s.Rotation = rotation;
    }

    #endregion Private Methods
}
