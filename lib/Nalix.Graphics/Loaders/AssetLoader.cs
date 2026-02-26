// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Logging.Extensions;

namespace Nalix.Graphics.Loaders;

/// <summary>
/// Asset management class. Handles loading/unloading of assets located in a specified root folder.
/// Multiple instances of the AssetManager class can be used to simplify asset memory management.
/// </summary>
public abstract class AssetLoader<[
    System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(
    System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors)] T>
    : System.IDisposable where T : class, System.IDisposable
{
    #region Fields

    private volatile System.Boolean _disposed;

    /// <summary>
    /// List of supported file endings for this AssetLoader
    /// </summary>
    protected System.String[] _fileExtensions;

    /// <summary>
    /// Dictionary of loaded assets, where the key is the asset name and the value is the asset instance.
    /// </summary>
    protected System.Collections.Generic.Dictionary<System.String, T> _assets = [];

    #endregion Fields

    #region Properties

    /// <summary>
    /// Indicates whether the asset loader has been disposed.
    /// </summary>
    public System.Boolean Disposed => _disposed;

    /// <summary>
    /// The root folder where assets are located.
    /// </summary>
    public System.String RootFolder { get; set; }

    /// <summary>
    /// Indicates whether the asset loader should log debug information.
    /// </summary>
    public System.Boolean EnableLogging { get; set; }

    /// <summary>
    /// List of supported file endings for this AssetLoader
    /// </summary>
    public System.Collections.Generic.IEnumerable<System.String> FileEndings => _fileExtensions;

    #endregion Properties

    #region Constructor

    internal AssetLoader(
        System.Collections.Generic.IEnumerable<System.String> supportedFileEndings,
        System.String assetRoot = "")
    {
        this.RootFolder = assetRoot;
        _fileExtensions = [.. System.Linq.Enumerable.Distinct(System.Linq.Enumerable.Concat([System.String.Empty], supportedFileEndings))];
    }

    #endregion Constructor

    #region Public Methods

    /// <summary>
    /// Loads or retrieves an already loaded instance of T from a File or Raw Data Source
    /// </summary>
    /// <param name="name"></param>
    /// <param name="rawData"></param>
    /// <returns></returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [return: System.Diagnostics.CodeAnalysis.MaybeNull]
    public virtual T Load(System.String name, System.Byte[] rawData = null)
    {
        System.ObjectDisposedException.ThrowIf(Disposed, nameof(AssetLoader<>));
        System.ArgumentNullException.ThrowIfNull(name);

        if (_assets.TryGetValue(name, out T value))
        {
            return value;
        }

        System.String input = null;
        try
        {
            T asset;

            if (rawData != null)
            {
                asset = Load(rawData);
                input = "rawData";
            }
            else
            {
                input = ResolveFileExtension(name);
                asset = CreateInstanceFromPath(input);
            }

            _assets.Add(name, asset);

            $"[AssetLoader<{typeof(T).Name}>] Loaded asset '{name}' successfully from {input}".Debug();
            return asset;
        }
        catch (System.Exception ex)
        {
            $"[AssetLoader<{typeof(T).Name}>] Failed to load asset '{name}'. InputState: {input ?? "null"}. Error: {ex.Message}\n{ex}".Error();
            if (EnableLogging)
            {
                throw;
            }
        }

        return null;
    }

    /// <summary>
    /// Loads all files in the specified directory and adds them to the asset manager.
    /// </summary>
    /// <param name="logErrors"></param>
    /// <returns></returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public virtual System.String[] LoadAllAssetsInDirectory(System.Boolean logErrors = false)
    {
        System.Collections.Generic.List<System.String> assetNames = [];

        foreach (System.String file in System.IO.Directory.EnumerateFiles(RootFolder))
        {
            try
            {
                T asset = CreateInstanceFromPath(file);
                System.String name = System.IO.Path.GetFileNameWithoutExtension(file);

                _assets.Add(name, asset);
                assetNames.Add(name);

                if (EnableLogging)
                {
                    $"[AssetLoader<{typeof(T).Name}>] Loaded asset: '{name}' from file: '{file}'".Info();
                }
            }
            catch (System.Exception e)
            {
                if (logErrors)
                {
                    $"""
                    [AssetLoader<{typeof(T).Name}>] Failed to load asset from file: '{file}'
                    Reason: {e.GetType().Name} - {e.Message}
                    """.Error();
                }

                if (EnableLogging)
                {
                    throw;
                }
            }
        }

        return [.. assetNames];
    }

    /// <summary>
    /// Releases the asset with the specified name.
    /// </summary>
    /// <param name="name"></param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void Unload(System.String name)
    {
        System.ObjectDisposedException.ThrowIf(Disposed, nameof(AssetLoader<>));
        if (!_assets.TryGetValue(name, out T value))
        {
            return;
        }

        value.Dispose();
        _ = _assets.Remove(name);
    }

    /// <summary>
    /// Disposes the asset loader and all loaded assets.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
         System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        Dispose(true);
        System.GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the asset loader and all loaded assets.
    /// </summary>
    /// <param name="disposing"></param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    protected virtual void Dispose(System.Boolean disposing)
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        foreach (var kvp in _assets)
        {
            try
            {
                kvp.Value.Dispose();
                $"[AssetLoader<{typeof(T).Name}>] Disposed asset '{kvp.Key}' successfully.".Debug();
            }
            catch (System.Exception e)
            {
                $"[AssetLoader<{typeof(T).Name}>] Failed to dispose asset '{kvp.Key}'. Error: {e.Message}\n{e}".Error();
            }
        }

        _assets.Clear();
        $"[AssetLoader<{typeof(T).Name}>] AssetLoader disposed. All assets released.".Info();
    }

    /// <summary>
    /// Creates an instance of type <typeparamref name="T"/> from raw binary data.
    /// </summary>
    /// <param name="bytes">The raw byte array representing the asset data.</param>
    /// <returns>An instance of <typeparamref name="T"/> created from the raw data.</returns>
    /// <exception cref="System.NotSupportedException">
    /// Thrown if this type <typeparamref name="T"/> does not support loading from raw data.
    /// Override this method in a derived class to provide a valid implementation.
    /// </exception>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    protected virtual T Load(System.Byte[] bytes)
        => throw new System.NotSupportedException($"{typeof(T).Name} does not support loading from raw data. Override this method.");

    /// <summary>
    /// Creates an instance of type <typeparamref name="T"/> from a file path.
    /// </summary>
    /// <param name="path">The full or relative file path to the asset.</param>
    /// <returns>
    /// An instance of <typeparamref name="T"/> created using a constructor that accepts a single <see cref="System.String"/> argument.
    /// </returns>
    /// <exception cref="System.MissingMethodException">
    /// Thrown if <typeparamref name="T"/> does not have a public constructor accepting a <see cref="System.String"/> path.
    /// </exception>
    /// <exception cref="System.Reflection.TargetInvocationException">
    /// Thrown if the constructor of <typeparamref name="T"/> throws an exception.
    /// </exception>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    protected virtual T CreateInstanceFromPath(System.String path) => (T)System.Activator.CreateInstance(typeof(T), [path]);

    #endregion Public Methods

    #region Private Methods

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private System.String ResolveFileExtension(System.String name)
    {
        foreach (System.String ending in _fileExtensions)
        {
            System.String candidate = System.IO.Path.Combine(RootFolder, $"{name}{ending}");
            if (System.IO.File.Exists(candidate))
            {
                return candidate;
            }
        }

        System.String[] attemptedPaths = [.. System.Linq.Enumerable.Select(_fileExtensions, f => System.IO.Path.Combine(RootFolder, $"{name}{f}"))];

        $"""
        [AssetLoader] Could not find a matching file for asset '{name}'.
        Tried extensions: {System.String.Join(", ", _fileExtensions)}
        Attempted paths:
        {System.String.Join("\n", attemptedPaths)}
        Root folder: {RootFolder}
        Fallback path used: {System.IO.Path.Combine(RootFolder, name)}
        """.Warn();

        return System.IO.Path.Combine(RootFolder, name);
    }

    #endregion Private Methods
}
