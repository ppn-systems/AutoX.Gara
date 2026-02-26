// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Framework.Injection.DI;
using Nalix.Logging.Extensions;
using Nalix.Graphics.Abstractions;
using Nalix.Graphics.Attributes;
using Nalix.Graphics.Engine;
using Nalix.Graphics.Entities;

namespace Nalix.Graphics.Scenes;

/// <summary>
/// The SceneManager class is responsible for managing scenes and objects within those scenes.
/// It handles scene transitions, object spawning, and object destruction.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
[System.Diagnostics.DebuggerDisplay("CurrentScene = {_currentScene?.Name}, ActiveObjects = {_activeSceneObjects.Count}")]
public class SceneManager : SingletonBase<SceneManager>, IUpdatable
{
    #region Events

    /// <summary>
    /// Invoked when objects are spawned or destroyed, requiring render cache update.
    /// </summary>
    public event System.EventHandler ObjectsModified;

    /// <summary>
    /// This event is invoked at the beginning of the next frame after all non-persisting objects have been queued to be destroyed
    /// and after the new objects have been queued to spawn, but before they are initialized.
    /// </summary>
    public event System.EventHandler<SceneChangedEventArgs> SceneChanged;

    #endregion Events

    #region Fields

    private BaseScene _currentScene;
    private System.String _nextScene = "";

    private readonly System.Collections.Generic.List<BaseScene> _loadedScenes = [];
    private readonly System.Collections.Generic.HashSet<SceneObject> _activeSceneObjects = [];

    internal readonly System.Collections.Generic.HashSet<SceneObject> PendingSpawnObjects = [];
    internal readonly System.Collections.Generic.HashSet<SceneObject> PendingDestroyObjects = [];

    #endregion Fields

    #region APIs

    /// <inheritdoc/>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void Update(System.Single deltaTime)
    {
        SceneObject[] snapshot = [.. _activeSceneObjects];

        System.Threading.Tasks.Parallel.ForEach(snapshot, o =>
        {
            if (o.IsEnabled && _activeSceneObjects.Contains(o)) // Check still active
            {
                o.Update(deltaTime);
            }
        });
    }

    /// <summary>
    /// Determines whether an active object of the specified type exists.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public System.Boolean HasActiveObject<T>() where T : SceneObject
    {
        foreach (SceneObject o in _activeSceneObjects)
        {
            if (o is T)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Queues a scene to be loaded on the next frame.
    /// </summary>
    /// <param name="name">The name of the scene to be loaded.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void ScheduleSceneChange(System.String name)
    {
        if (System.String.IsNullOrEmpty(name))
        {
            NLogixFx.Warn(message: "Attempted to schedule empty scene change.", source: "SceneManager");
            return;
        }

        if (name == _currentScene?.Name)
        {
            NLogixFx.Debug(message: $"Already in scene [{name}], ignoring change request.", source: "SceneManager");
            return;
        }

        _nextScene = name;
        NLogixFx.Debug(message: $"Scheduled scene change to [{name}]", source: "SceneManager");
    }

    /// <summary>
    /// Queues a single object to be spawned in the scene.
    /// </summary>
    /// <param name="o">The object to be spawned.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void EnqueueSpawn(SceneObject o)
    {
        if (o.IsInitialized)
        {
            NLogixFx.Error(message: $"Attempt to schedule already-initialized object {o.GetType().Name} (ID: {o.GetHashCode()}) for spawn.", source: "SceneManager");
            throw new System.Exception($"Instance of SceneObject {nameof(o)} already exists in Scenes");
        }
        if (!PendingSpawnObjects.Add(o))
        {
            $"Instance of SceneObject {nameof(o)} is already queued to be spawned.".Warn();
        }
    }

    /// <summary>
    /// Queues a collection of objects to be spawned in the scene.
    /// </summary>
    /// <param name="sceneObjects">The collection of objects to be spawned.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void EnqueueSpawn(System.Collections.Generic.IEnumerable<SceneObject> sceneObjects)
    {
        foreach (SceneObject o in sceneObjects)
        {
            EnqueueSpawn(o);
        }
    }

    /// <summary>
    /// Queues an object to be destroyed in the scene.
    /// </summary>
    /// <param name="o">The object to be destroyed.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void EnqueueDestroy(SceneObject o)
    {
        if (!_activeSceneObjects.Contains(o) && !PendingSpawnObjects.Contains(o))
        {
            NLogixFx.Error(message: $"Attempt to destroy non-existent SceneObject (ID: {o.GetHashCode()})", source: "SceneManager");
            throw new System.Exception("Instance of SceneObject does not exist in the scene.");
        }
        if (!PendingSpawnObjects.Remove(o) && !PendingDestroyObjects.Add(o))
        {
            "Instance of SceneObject is already queued to be destroyed.".Warn();
        }
    }

    /// <summary>
    /// Queues a collection of objects to be destroyed in the scene.
    /// </summary>
    /// <param name="sceneObjects">The collection of objects to be destroyed.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void EnqueueDestroy(System.Collections.Generic.IEnumerable<SceneObject> sceneObjects)
    {
        foreach (SceneObject o in sceneObjects)
        {
            EnqueueDestroy(o);
        }
    }

    /// <summary>
    /// Retrieves all objects in the scene of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of objects to retrieve.</typeparam>
    /// <returns>ScreenSize HashSet of all objects of the specified type.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public System.Collections.Generic.IReadOnlyCollection<T> GetActiveObjects<T>() where T : SceneObject
        => System.Linq.Enumerable.ToList(System.Linq.Enumerable.OfType<T>(_activeSceneObjects));

    /// <summary>
    /// Finds the first object of a specific type in the scene.
    /// </summary>
    /// <typeparam name="T">The type of object to find.</typeparam>
    /// <returns>The first object of the specified type, or null if none exist.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [return: System.Diagnostics.CodeAnalysis.MaybeNull]
    public T GetFirstActive<T>() where T : SceneObject
    {
        System.Collections.Generic.IReadOnlyCollection<T> objects = GetActiveObjects<T>();
        return objects.Count != 0 ? System.Linq.Enumerable.First(objects) : null;
    }

    /// <summary>
    /// Gets the name of the currently active scene.
    /// </summary>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public System.String GetActiveSceneName() => _currentScene?.Name ?? System.String.Empty;

    /// <summary>
    /// Determines whether the specified object is currently active in the scene.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public System.Boolean IsObjectActive(SceneObject o) => _activeSceneObjects.Contains(o);

    /// <summary>
    /// Determines whether the specified object is queued for spawn or destroy.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public System.Boolean IsObjectQueued(SceneObject o) => PendingSpawnObjects.Contains(o) || PendingDestroyObjects.Contains(o);

    #endregion APIs

    #region Internal Methods

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal void ProcessSceneChange()
    {
        if (System.String.IsNullOrEmpty(_nextScene))
        {
            return;
        }

        System.String targetScene = _nextScene;
        System.String lastScene = _currentScene?.Name ?? "";

        try
        {
            CLEAR_SCENE();
            LOAD_SCENE(targetScene);

            _nextScene = "";

            NLogixFx.Info(message: $"Scene changed from [{lastScene}] to [{_nextScene}].", source: "SceneManager");
            SceneChanged?.Invoke(this, new SceneChangedEventArgs(lastScene, _nextScene));
        }
        catch (System.Exception ex)
        {
            NLogixFx.Error(message: $"Error occurred during scene change: {ex}", source: "SceneManager");

            if (!System.String.IsNullOrEmpty(lastScene))
            {
                try
                {
                    LOAD_SCENE(lastScene);
                    _nextScene = "";
                    NLogixFx.Warn(message: $"Rolled back to scene [{lastScene}]", source: "SceneManager");
                }
                catch (System.Exception rollbackEx)
                {
                    NLogixFx.Error(message: $"Rollback also failed: {rollbackEx}", source: "SceneManager");
                }
            }

            throw;
        }
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal void ProcessPendingSpawn()
    {
        System.Boolean hasChanges = this.PendingSpawnObjects.Count > 0;

        foreach (SceneObject q in this.PendingSpawnObjects)
        {
            if (!_activeSceneObjects.Add(q))
            {
                throw new System.Exception("Instance of queued SceneObject already exists in scene.");
            }
        }

        this.PendingSpawnObjects.Clear();

        foreach (SceneObject o in _activeSceneObjects)
        {
            if (!o.IsInitialized)
            {
                o.InternalInitialize();
            }
        }

        if (hasChanges)
        {
            ObjectsModified?.Invoke(this, System.EventArgs.Empty);
        }
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal void ProcessPendingDestroy()
    {
        System.Boolean hasChanges = this.PendingDestroyObjects.Count > 0;

        foreach (SceneObject o in this.PendingDestroyObjects)
        {
            if (!_activeSceneObjects.Remove(o))
            {
                "Instance of SceneObject to be destroyed does not exist in scene".Warn();
                continue;
            }
            o.OnBeforeDestroy();
        }

        this.PendingDestroyObjects.Clear();

        if (hasChanges)
        {
            ObjectsModified?.Invoke(this, System.EventArgs.Empty);
        }
    }

    /// <summary>
    /// Creates instances of all classes inheriting from Scenes in the specified namespace.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' " +
        "require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Trimming",
        "IL2075:'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. " +
        "The return value of the source method does not have matching annotations.", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality",
        "IDE0079:Remove unnecessary suppression", Justification = "<Pending>")]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal void InitializeScenes()
    {
        // Get the types from the entry assembly that match the scene namespace
        System.Collections.Generic.IEnumerable<System.Type> sceneTypes =
            System.Linq.Enumerable.Where(
                System.Reflection.Assembly.GetEntryAssembly()!.GetTypes(), t => t.Namespace?.Contains(GraphicsEngine.GraphicsConfig.SceneNamespace) == true);

        // HashSet to check for duplicate scene names efficiently
        System.Collections.Generic.HashSet<System.String> sceneNames = [];

        foreach (System.Type type in sceneTypes)
        {
            // Skip compiler-generated types (like anonymous types or internal generic types)
            if (type.Name.Contains('<'))
            {
                continue;
            }

            // Check if the class has the IgnoredLoadAttribute
            if (System.Reflection.CustomAttributeExtensions.GetCustomAttribute<DynamicLoadAttribute>(type) == null)
            {
                //NLogixFx.Debug(
                //    message: $"Skipping load of scene {type.Name} because it is marked as not loadable.",
                //    source: type.Name);

                continue;
            }

            // Attempt to find a constructor with no parameters
            System.Reflection.ConstructorInfo constructor = type.GetConstructor(System.Type.EmptyTypes);
            if (constructor == null)
            {
                continue;
            }

            // Instantiate the scene using the parameterless constructor
            BaseScene scene;
            try
            {
                scene = (BaseScene)constructor.Invoke(null);
            }
            catch (System.Exception ex)
            {
                // Handle any exceptions that occur during instantiation
                ex.Error(source: type.Name, message: $"Error instantiating scene {type.Name}: {ex.Message}");
                continue;
            }

            // Check for duplicate scene names
            if (sceneNames.Contains(scene.Name))
            {
                NLogixFx.Error(message: $"Duplicate scene name '{scene.Name}' detected.", source: type.Name);
                throw new System.Exception($"Scenes with name {scene.Name} already exists.");
            }

            // Add the scene name to the HashSet for future checks
            _ = sceneNames.Add(scene.Name);

            // Add the scene to the list
            _loadedScenes.Add(scene);
        }

        // Switch to the main scene defined in the config
        this.ScheduleSceneChange(GraphicsEngine.GraphicsConfig.MainScene);
    }

    #endregion Internal Methods

    #region Private Methods

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private void CLEAR_SCENE()
    {
        System.Collections.Generic.List<SceneObject> toDestroy =
            System.Linq.Enumerable.ToList(
                System.Linq.Enumerable.Where(_activeSceneObjects, o => !o.IsPersistent));

        foreach (SceneObject sceneObject in toDestroy)
        {
            sceneObject.OnBeforeDestroy();
            _ = _activeSceneObjects.Remove(sceneObject);
        }

        System.Collections.Generic.List<SceneObject> pendingToDestroy =
            System.Linq.Enumerable.ToList(
                System.Linq.Enumerable.Where(PendingSpawnObjects, o => !o.IsPersistent));

        foreach (SceneObject queued in pendingToDestroy)
        {
            queued.OnBeforeDestroy();
            _ = PendingSpawnObjects.Remove(queued);
        }
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private void LOAD_SCENE(System.String name)
    {
        BaseScene found = System.Linq.Enumerable.FirstOrDefault(_loadedScenes, scene => scene.Name == name);
        if (found == null)
        {
            NLogixFx.Error(message: $"Scene '{name}' not found in scene list.", source: "SceneManager");
            throw new System.Exception($"Scene with name '{name}' does not exist.");
        }

        _currentScene = found;
        _currentScene.InitializeScene();

        this.EnqueueSpawn(_currentScene.GetObjects());
    }

    #endregion Private Methods
}
