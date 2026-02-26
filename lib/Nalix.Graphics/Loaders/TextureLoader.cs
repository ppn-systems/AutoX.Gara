// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Logging.Extensions;
using SFML.Graphics;

namespace Nalix.Graphics.Loaders;

/// <summary>
/// Texture management class. Handles loading/unloading of unmanaged Texture resources.
/// </summary>
/// <remarks>
/// Creates a new instance of the TextureLoader class.
/// </remarks>
/// <param name="assetRoot">Optional root path of the managed asset folder</param>
/// <param name="repeat">Determines if loaded Textures should repeat when the texture rectangle exceeds its dimension</param>
/// <param name="smoothing">Determines if a smoothing should be applied onto newly loaded Textures</param>
public sealed class TextureLoader(System.String assetRoot = "", System.Boolean repeat = false, System.Boolean smoothing = false)
    : AssetLoader<Texture>(SupportedFormats, assetRoot)
{
    #region Properties

    /// <summary>
    /// List of supported file endings for this TextureLoader
    /// </summary>
    public static readonly System.Collections.Generic.IEnumerable<System.String> SupportedFormats =
    [
        ".bmp", ".png", ".tga", ".jpg",
        ".gif", ".psd", ".hdr", ".pic"
    ];

    /// <summary>
    /// Determines if loaded Textures should repeat when the texture rectangle exceeds its dimension.
    /// </summary>
    public System.Boolean Repeat { get; set; } = repeat;

    /// <summary>
    /// Determines if a smoothing should be applied onto newly loaded Textures.
    /// </summary>
    public System.Boolean Smoothing { get; set; } = smoothing;

    #endregion Properties

    #region APIs

    /// <summary>
    /// Loads or retrieves an already loaded instance of a Texture from a File or Raw Data Source
    /// </summary>
    /// <param name="name">Name of the Texture</param>
    /// <param name="rawData">Optional byte array containing the raw data of the Texture</param>
    /// <returns>The managed Texture</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public override Texture Load(System.String name, System.Byte[] rawData = null) => this.Load(name, Repeat, Smoothing, rawData);

    /// <summary>Loads or retrieves an already loaded instance of a Texture from a File or Raw Data Source</summary>
    /// <param name="name">Name of the Texture</param>
    /// <param name="repeat">Determines if loaded Textures should repeat when the texture rectangle exceeds its dimension.</param>
    /// <param name="smoothing">Determines if a smoothing should be applied onto newly loaded Textures.</param>
    /// <param name="rawData">Optional byte array containing the raw data of the Texture</param>
    /// <returns>The managed Texture</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public Texture Load(
        System.String name, System.Boolean? repeat = null,
        System.Boolean? smoothing = null, System.Byte[] rawData = null)
    {
        Texture tex = base.Load(name, rawData);
        if (tex != null)
        {
            tex.Repeated = repeat ?? Repeat;
            tex.Smooth = smoothing ?? Smoothing;

        }
        return tex;
    }

    /// <inheritdoc/>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override Texture Load(System.Byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
        {
            NLogixFx.Error(message: "TextureLoader.Load: Raw data is null or empty.", source: "TextureLoader");
            throw new System.ArgumentException("Raw data is null or empty.", nameof(bytes));
        }

        using System.IO.MemoryStream ms = new(bytes);
        Texture texture = new(ms); // Pass the MemoryStream to the constructor
        NLogixFx.Debug(message: $"TextureLoader.Load: Loaded texture from raw data ({bytes.Length} bytes).", source: "TextureLoader");

        return texture;
    }

    /// <inheritdoc/>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override Texture CreateInstanceFromPath(System.String path)
    {
        if (System.String.IsNullOrWhiteSpace(path))
        {
            NLogixFx.Error(message: "TextureLoader.CreateInstanceFromPath: Path is null or empty.", source: "TextureLoader");
            throw new System.ArgumentException("Path is null or empty.", nameof(path));
        }
        using System.IO.FileStream fs = System.IO.File.OpenRead(path);
        Texture texture = new(fs);
        NLogixFx.Debug(message: $"TextureLoader.CreateInstanceFromPath: Loaded texture from path: {path}", source: "TextureLoader");

        return texture;
    }

    #endregion APIs
}
