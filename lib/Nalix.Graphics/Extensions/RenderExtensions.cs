// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Graphics.Entities;
using Nalix.Graphics.Enums;
using SFML.Graphics;

namespace Nalix.Graphics.Extensions;

/// <inheritdoc/>
internal static class RenderExtensions
{
    /// <inheritdoc/>
    public static System.Int32 ToZIndex(this RenderLayer layer) => (System.Int32)layer;

    /// <inheritdoc/>
    public static void Draw(this IRenderTarget target, RenderObject renderObject) => renderObject.Draw(target);
}
