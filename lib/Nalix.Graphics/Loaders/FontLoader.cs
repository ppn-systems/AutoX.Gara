// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Logging.Extensions;
using SFML.Graphics;

namespace Nalix.Graphics.Loaders;

/// <summary>
/// Font management class. Handles loading/unloading of unmanaged font resources.
/// </summary>
/// <remarks>
/// Creates a new instance of the FontLoader class.
/// </remarks>
/// <param name="rootFolder">Optional root path of the managed asset folder</param>
public sealed class FontLoader(System.String rootFolder = "") : AssetLoader<Font>(SupportedExtensions, rootFolder)
{
    #region Properties

    /// <summary>
    /// List of supported file endings for this FontLoader
    /// </summary>
    public static readonly System.Collections.Generic.IEnumerable<System.String> SupportedExtensions = [".ttf", ".cff", ".fnt", ".ttf", ".otf", ".eot"];

    #endregion Properties

    #region APIs

    /// <inheritdoc/>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override Font Load(System.Byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
        {
            NLogixFx.Error(message: "FontLoader.Load: Raw data is null or empty.", source: "FontLoader");
            throw new System.ArgumentException("Raw data is null or empty.", nameof(bytes));
        }

        using System.IO.MemoryStream ms = new(bytes, writable: false);
        NLogixFx.Debug(message: $"FontLoader.Load: Loaded font from raw data, size={bytes.Length} bytes.", source: "FontLoader");

        return new Font(ms);
    }

    /// <inheritdoc/>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    protected override Font CreateInstanceFromPath(System.String path)
        => System.String.IsNullOrWhiteSpace(path) ? throw new System.ArgumentException("Path is null or empty.", nameof(path)) : new Font(path);

    #endregion APIs
}
