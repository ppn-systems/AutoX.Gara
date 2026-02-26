// Copyright (c) 2026 PPN Corporation. All rights reserved.

using SFML.Audio;

namespace Nalix.Graphics.Managers;

/// <summary>
/// Represents a single Sound Effect
/// </summary>
public class SoundEffectPool : System.IDisposable
{
    #region Fields

    private SoundBuffer _buffer;
    private Sound[] _soundInstances;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the name of this <see cref="SoundEffectPool"/>.
    /// </summary>
    public System.String Name { get; }

    /// <summary>
    /// Determines whether this <see cref="SoundEffectPool"/> has been disposed.
    /// </summary>
    public System.Boolean Disposed { get; private set; }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="SoundEffectPool" /> class.
    /// </summary>
    /// <param name="name">The sounds name</param>
    /// <param name="soundBuffer">Sound buffer containing the audio data to play with the sound instance</param>
    /// <param name="parallelSounds">The maximum number of parallel playing sounds.</param>
    public SoundEffectPool(System.String name, SoundBuffer soundBuffer, System.Int32 parallelSounds)
    {
        if (System.String.IsNullOrWhiteSpace(name))
        {
            throw new System.ArgumentException($"Invalid {nameof(name)}:{name}");
        }

        Name = name;
        _buffer = soundBuffer ?? throw new System.ArgumentNullException(nameof(soundBuffer));
        _soundInstances = new Sound[System.Math.Clamp(parallelSounds, 1, 25)];
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="SoundEffectPool" /> class.
    /// </summary>
    ~SoundEffectPool()
    {
        Dispose(false);
    }

    #endregion Constructor

    #region Public Methods

    /// <summary>
    /// Retrieves a sound when available. The amount of sounds per frame is limited.
    /// </summary>
    /// <returns>The sound instance or null when too many instances of the same sound are already active</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [return: System.Diagnostics.CodeAnalysis.MaybeNull]
    public Sound GetAvailableInstance()
    {
        System.ObjectDisposedException.ThrowIf(Disposed, Name);

        for (System.Int32 i = 0; i < _soundInstances.Length; i++)
        {
            Sound sound = _soundInstances[i];
            if (sound == null)
            {
                _soundInstances[i] = sound = new Sound(_buffer);
            }

            if (sound.Status != SoundStatus.Stopped)
            {
                continue;
            }

            return sound;
        }

        return null; // when all sounds are busy none shall be added
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        Dispose(true);
        System.GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    protected virtual void Dispose(System.Boolean disposing)
    {
        if (!Disposed)
        {
            if (disposing)
            {
                for (System.Int32 i = 0; i < _soundInstances.Length; i++)
                {
                    _soundInstances[i]?.Dispose();
                    _soundInstances[i] = null;
                }
            }
            _soundInstances = null;
            _buffer = null;
            Disposed = true;
        }
    }

    #endregion Public Methods
}
