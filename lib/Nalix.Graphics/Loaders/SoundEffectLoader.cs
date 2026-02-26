// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Logging.Extensions;
using SFML.Audio;

namespace Nalix.Graphics.Loaders;

/// <summary>
/// Sound management class. Handles loading/unloading of unmanaged sound resources.
/// </summary>
/// <remarks>
/// Creates a new instance of the SfxLoader class.
/// </remarks>
/// <param name="rootFolder">Optional root path of the managed asset folder</param>
public sealed class SoundEffectLoader(System.String rootFolder = "") : AssetLoader<SoundBuffer>(SupportedFormats, rootFolder)
{
    #region Properties

    /// <summary>
    /// List of supported file endings for this SfxLoader
    /// </summary>
    public static readonly System.Collections.Generic.IEnumerable<System.String> SupportedFormats =
    [
            ".ogg", ".wav", ".flac", ".aiff", ".au", ".raw",
            ".paf", ".svx", ".nist", ".voc", ".ircam", ".w64",
            ".mat4", ".mat5", ".pvf", ".htk", ".sds", ".avr",
            ".sd2", ".caf", ".wve", ".mpc2k", ".rf64"
    ];

    #endregion Properties

    #region APIs

    /// <summary>
    /// Loads or retrieves an already loaded instance of a Sound from a Stream Source
    /// </summary>
    /// <param name="name">Name of the Resource</param>
    /// <param name="stream">Readable stream containing the raw data of the sound</param>
    /// <returns>The managed Sound</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public SoundBuffer Load(System.String name, System.IO.Stream stream)
    {
        System.ObjectDisposedException.ThrowIf(Disposed, nameof(SoundEffectLoader));
        System.ArgumentNullException.ThrowIfNull(name);

        if (_assets.TryGetValue(name, out SoundBuffer value))
        {
            return value;
        }

        if (stream?.CanRead != true)
        {
            NLogixFx.Error(message: "SoundEffectLoader.Load: Stream is null or not readable.", source: "SoundEffectLoader");
            throw new System.ArgumentNullException(nameof(stream));
        }

        System.Byte[] data = new System.Byte[stream.Length];
        stream.ReadExactly(data);
        NLogixFx.Debug(message: $"SoundEffectLoader.Load: Loaded sound '{name}' from stream ({data.Length} bytes).", source: "SoundEffectLoader");

        return Load(name, data);
    }

    /// <inheritdoc/>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override SoundBuffer Load(System.Byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
        {
            NLogixFx.Error(message: "SoundEffectLoader.Load: Raw data is null or empty.", source: "SoundEffectLoader");
            throw new System.ArgumentException("Raw data is null or empty.", nameof(bytes));
        }

        using System.IO.MemoryStream memoryStream = new(bytes, writable: false);
        NLogixFx.Debug(message: $"SoundEffectLoader.Load: Loaded sound from raw data ({bytes.Length} bytes).", source: "SoundEffectLoader");

        return new SoundBuffer(memoryStream);
    }

    /// <inheritdoc/>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override SoundBuffer CreateInstanceFromPath(System.String path)
        => System.String.IsNullOrWhiteSpace(path) ? throw new System.ArgumentException("Path is null or empty.", nameof(path)) : new SoundBuffer(path);

    #endregion APIs
}
