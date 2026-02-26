// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Graphics.Entities;

namespace Nalix.Graphics.Scenes;

/// <summary>
/// Represents a base class for a scene in the game.
/// This class provides methods to manage initial scene objects and ensures that derived scenes implement object loading.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BaseScene"/> class with a specified name.
/// </remarks>
/// <param name="name">The name of the scene.</param>
[System.Diagnostics.DebuggerDisplay("Scene = {Name}, Objects = {_sceneObjects.Count}")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
public abstract class BaseScene(System.String name)
{
    #region Fields

    private readonly System.Collections.Generic.List<SceneObject> _sceneObjects = [];

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the name of the scene.
    /// </summary>
    public readonly System.String Name = name;

    #endregion Properties

    #region Protected Methods

    /// <summary>
    /// An abstract method that must be implemented by derived scenes to load their specific objects.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    protected abstract void LoadObjects();

    /// <summary>
    /// Clears all objects from the initial objects list.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private void ClearObjects() => _sceneObjects.Clear();

    #endregion Protected Methods

    #region Public Methods

    /// <summary>
    /// Retrieves the list of initial objects in the scene.
    /// </summary>
    /// <returns>ScreenSize list of <see cref="SceneObject"/>.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Collections.Generic.List<SceneObject> GetObjects() => _sceneObjects;

    /// <summary>
    /// Adds an object to the list of initial objects in the scene.
    /// </summary>
    /// <param name="o">The <see cref="SceneObject"/> to add.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void AddObject(SceneObject o) => _sceneObjects.Add(o);

    /// <summary>
    /// Creates the scene by clearing and loading its objects.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void InitializeScene()
    {
        this.ClearObjects();
        this.LoadObjects();
    }

    #endregion Public Methods
}
