// Copyright (c) 2026 PPN Corporation. All rights reserved.

using SFML.Graphics;

namespace Nalix.Graphics.Abstractions;

/// <summary>
/// Represents an interface for renderable objects.
/// </summary>
public interface IRenderable
{
    /// <summary>
    /// Draws the object on the specified render target.
    /// </summary>
    void Draw(IRenderTarget target);
}
