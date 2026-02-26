// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Graphics.Entities;
using Nalix.Graphics.Scenes;

namespace Nalix.Graphics.Extensions;

/// <summary>
/// Provides extension methods for <see cref="SceneObject"/> related to scene management queues.
/// </summary>
internal static class SceneExtensions
{
    /// <summary>
    /// Determines whether the specified <see cref="SceneObject"/> is currently in the pending spawn queue.
    /// </summary>
    /// <param name="o">The <see cref="SceneObject"/> to check.</param>
    /// <returns>
    /// <c>true</c> if the object is in the pending spawn queue; otherwise, <c>false</c>.
    /// </returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal static System.Boolean InSpawnQueue(this SceneObject o) => SceneManager.Instance.PendingSpawnObjects.Contains(o);

    /// <summary>
    /// Determines whether the specified <see cref="SceneObject"/> is currently in the pending destroy queue.
    /// </summary>
    /// <param name="o">The <see cref="SceneObject"/> to check.</param>
    /// <returns>
    /// <c>true</c> if the object is in the pending destroy queue; otherwise, <c>false</c>.
    /// </returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal static System.Boolean InDestroyQueue(this SceneObject o) => SceneManager.Instance.PendingDestroyObjects.Contains(o);
}
