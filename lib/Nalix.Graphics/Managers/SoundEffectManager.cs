// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Logging.Extensions;
using Nalix.Graphics.Loaders;
using SFML.Audio;
using SFML.System;

namespace Nalix.Graphics.Managers;

/// <summary>
/// Simplifies access and management of sound effects
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SoundEffectManager" /> class.
/// </remarks>
/// <param name="loader">Resolution <see cref="SoundEffectLoader" /> instance to load the required sound effects</param>
/// <param name="volume">Resolution function that returns the current volume.</param>
/// <exception cref="System.ArgumentNullException">loader</exception>
public class SoundEffectManager(SoundEffectLoader loader, System.Func<System.Int32> volume)
{
    #region Fields

    private readonly System.Func<System.Int32> _getCurrentVolume = volume;
    private readonly System.Collections.Generic.Dictionary<System.String, SoundEffectPool> _soundLibrary = [];
    private readonly SoundEffectLoader _soundEffectLoader = loader ?? throw new System.ArgumentNullException(nameof(loader));

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets or sets the global listener position for spatial sounds.
    /// </summary>
    public static Vector2f ListenerPosition
    {
        get => new(Listener.Position.X, Listener.Position.Y);
        set => Listener.Position = new Vector3f(value.X, value.Y, Listener.Position.Z);
    }

    /// <summary>
    /// Gets or sets the initial volume drop-off start distance for spatial sounds.
    /// This defines the maximum distance a sound can still be heard at max volume.
    /// </summary>
    public System.Single VolumeDropoffStartDistance { get; set; } = 500;

    /// <summary>
    /// Gets or sets the initial volume drop-off factor for spatial sounds.
    /// Defines how fast the volume drops beyond the <see cref="VolumeDropoffStartDistance"/>
    /// </summary>
    public System.Single VolumeDropoffFactor { get; set; } = 10;

    #endregion Properties

    #region APIs

    /// <summary>
    /// Loads all compatible files from a folder into the sound library.
    /// </summary>
    /// <param name="root">Optional: root folder path when different from the default asset root.</param>
    /// <param name="parallelSounds">The amount of times each sound can be played in parallel.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void LoadAllFromDirectory(System.String root = null, System.Int32 parallelSounds = 2)
    {
        System.ObjectDisposedException.ThrowIf(_soundEffectLoader.Disposed, nameof(_soundEffectLoader));

        System.String oldRoot = _soundEffectLoader.RootFolder;
        if (root != null)
        {
            _soundEffectLoader.RootFolder = root;
        }

        System.String[] sounds = _soundEffectLoader.LoadAllAssetsInDirectory();
        $"[SfxManager] Loaded {sounds.Length} sound files from '{_soundEffectLoader.RootFolder}'".Debug();
        _soundEffectLoader.RootFolder = oldRoot;

        LoadFromFileList(sounds, parallelSounds);
    }

    /// <summary>
    /// Loads the specified files into the sound library.
    /// </summary>
    /// <param name="files">The files to load.</param>
    /// <param name="parallelSounds">The amount of times each sound can be played in parallel.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void LoadFromFileList(System.Collections.Generic.IEnumerable<System.String> files, System.Int32 parallelSounds)
    {
        foreach (System.String file in files)
        {
            RegisterSound(file, parallelSounds);
        }
    }

    /// <summary>
    /// Loads a new entry into the sound library.
    /// </summary>
    /// <param name="name">The name of the sound effect.</param>
    /// <param name="parallelSounds">The amount of times this sound can be played in parallel.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void RegisterSound(System.String name, System.Int32 parallelSounds)
    {
        System.ObjectDisposedException.ThrowIf(_soundEffectLoader.Disposed, nameof(_soundEffectLoader));
        try
        {
            if (_soundLibrary.TryGetValue(name, out SoundEffectPool sound))
            {
                _ = _soundLibrary.Remove(name);
                sound.Dispose();
                RegisterSound(name, parallelSounds);
            }
            else
            {
                // Add new SoundManager
                SoundBuffer buffer = _soundEffectLoader.Load(name);
                if (buffer != null)
                {
                    sound = new SoundEffectPool(name, buffer, parallelSounds);
                    _soundLibrary.Add(name, sound);
                    $"[SfxManager] Loaded sound '{name}'".Info();
                }
                else
                {
                    $"[SfxManager] Failed to load sound buffer for '{name}'".Warn();
                }
            }
        }
        catch (System.Exception ex)
        {
            ex.Error(source: "SfxManager", message: $"Error loading sound '{name}': {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves a sound effect from the sound library if it is currently available.
    /// </summary>
    /// <param name="name">The name of the sound effect to retrieve.</param>
    /// <param name="spatial">Resolution boolean value that determines if the sound is spatial (3D) or not.
    /// If true, the sound will be spatialized with distance attenuation. If false, the sound is 2D and will play relative to the listener.</param>
    /// <returns>Resolution <see cref="Sound"/> instance if available, otherwise throws an <see cref="System.ArgumentException"/>.</returns>
    /// <exception cref="System.ObjectDisposedException">Thrown if the loader has been disposed.</exception>
    /// <exception cref="System.ArgumentException">Thrown if no sound is found with the specified name.</exception>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public Sound GetSound(System.String name, System.Boolean spatial = false)
    {
        System.ObjectDisposedException.ThrowIf(_soundEffectLoader.Disposed, nameof(_soundEffectLoader));

        if (_soundLibrary.TryGetValue(name, out SoundEffectPool SoundManager))
        {
            Sound sound = SoundManager.GetAvailableInstance();
            if (sound != null)
            {
                sound.Volume = _getCurrentVolume.Invoke();
                sound.RelativeToListener = !spatial;

                if (spatial)
                {
                    sound.Attenuation = VolumeDropoffFactor;
                    sound.MinDistance = VolumeDropoffStartDistance;
                }
            }
            return sound;
        }

        throw new System.ArgumentException($"There is no sound named '{name}'");
    }

    /// <summary>
    /// Plays a sound effect when currently available.
    /// </summary>
    /// <param name="name">The name of the sound effect.</param>
    /// <param name="position">Optional position of the sound. Only relevant when sound is supposed to be spatial.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void Play(System.String name, Vector2f? position = null)
    {
        System.ObjectDisposedException.ThrowIf(_soundEffectLoader.Disposed, nameof(_soundEffectLoader));

        var sound = GetSound(name, position.HasValue);
        if (sound == null)
        {
            return;
        }

        // If position is specified, set it in 3D space
        if (position.HasValue)
        {
            sound.Position = new Vector3f(position.Value.X, position.Value.Y, 0f); // Assuming z = 0 for 2D position.
        }

        sound.Play();
    }

    #endregion APIs
}
