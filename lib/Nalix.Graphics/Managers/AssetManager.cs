// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Framework.Injection.DI;
using Nalix.Graphics.Engine;
using Nalix.Graphics.Loaders;
using SFML.Audio;
using SFML.Graphics;

namespace Nalix.Graphics.Managers;

/// <summary>
/// Centralized manager that handles textures, fonts, and sound effects loading/unloading.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AssetManager"/> class.
/// </remarks>
/// <param name="rootFolder">The root directory for assets.</param>
public sealed class AssetManager(System.String rootFolder = null!) : SingletonBase<AssetManager>, System.IDisposable
{
    #region Properties

    /// <summary>
    /// Gets the font loader instance.
    /// </summary>
    public FontLoader FontManager { get; } = new FontLoader(rootFolder ?? GraphicsConfig.AssetRoot);

    /// <summary>
    /// Gets the texture loader instance.
    /// </summary>
    public TextureLoader TextureManager { get; } = new TextureLoader(rootFolder ?? GraphicsConfig.AssetRoot);

    /// <summary>
    /// Gets the sound effects loader instance.
    /// </summary>
    public SoundEffectLoader SoundEffectManager { get; } = new SoundEffectLoader(rootFolder ?? GraphicsConfig.AssetRoot);

    #endregion Properties

    #region Constructors

    public AssetManager() : this(null) { }

    #endregion Constructors

    #region Public Methods

    /// <summary>
    /// Load a texture by name (from file or memory).
    /// </summary>
    /// <param name="name">The name of the texture.</param>
    /// <param name="data">The binary data of the texture (optional).</param>
    /// <returns>ScreenSize <see cref="Texture"/> object.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public Texture LoadTexture(System.String name, System.Byte[] data = null) => this.TextureManager.Load(name, data);

    /// <summary>
    /// Load a font by name (from file or memory).
    /// </summary>
    /// <param name="name">The name of the font.</param>
    /// <param name="data">The binary data of the font (optional).</param>
    /// <returns>ScreenSize <see cref="Font"/> object.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public Font LoadFont(System.String name, System.Byte[] data = null) => this.FontManager.Load(name, data);

    /// <summary>
    /// Load a sound buffer by name (from file or memory).
    /// </summary>
    /// <param name="name">The name of the sound buffer.</param>
    /// <param name="data">The binary data of the sound buffer (optional).</param>
    /// <returns>ScreenSize <see cref="SoundBuffer"/> object.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public SoundBuffer LoadSound(System.String name, System.Byte[] data = null) => this.SoundEffectManager.Load(name, data);

    /// <summary>
    /// Load a sound buffer by name (from stream).
    /// </summary>
    /// <param name="name">The name of the sound buffer.</param>
    /// <param name="stream">The stream containing the sound buffer data.</param>
    /// <returns>ScreenSize <see cref="SoundBuffer"/> object.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public SoundBuffer LoadSound(System.String name, System.IO.Stream stream) => this.SoundEffectManager.Load(name, stream);

    /// <summary>
    /// Release all loaded assets.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public new void Dispose()
    {
        this.FontManager.Dispose();
        this.TextureManager.Dispose();
        this.SoundEffectManager.Dispose();
    }

    #endregion Public Methods
}
