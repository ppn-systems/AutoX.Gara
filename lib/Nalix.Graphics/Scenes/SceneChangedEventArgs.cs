// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace Nalix.Graphics.Scenes;

/// <summary>
/// Provides data for an event that is raised when the current scene changes.
/// </summary>
/// <remarks>
/// This class contains information about the scene that was previously active and the scene that is now active.
/// </remarks>
public sealed class SceneChangedEventArgs : System.EventArgs
{
    #region Properties

    /// <summary>
    /// Gets the name of the previous scene before the change occurred.
    /// </summary>
    public System.String PreviousScene { get; }

    /// <summary>
    /// Gets the name of the newly activated scene.
    /// </summary>
    public System.String CurrentScene { get; }

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SceneChangedEventArgs"/> class.
    /// </summary>
    /// <param name="previous">
    /// The name of the scene that was previously active.
    /// </param>
    /// <param name="current">
    /// The name of the scene that is now active.
    /// </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public SceneChangedEventArgs(System.String previous, System.String current)
    {
        this.CurrentScene = current;
        this.PreviousScene = previous;
    }

    #endregion Constructors
}
