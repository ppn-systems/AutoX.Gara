// Copyright (c) 2026 PPN Corporation. All rights reserved.

using SFML.Graphics;

namespace Nalix.Graphics.Extensions;

/// <summary>
/// Extension methods for converting byte arrays to SFML font objects.
/// </summary>
public static class FontExtensions
{
    #region Fields

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<System.Int32, Font> _cache = new();

    #endregion Fields

    #region APIs

    /// <summary>
    /// Converts a byte array (containing a valid font file) to an SFML <see cref="Font"/> object.
    /// </summary>
    /// <param name="fontBytes">The byte array representing the font file (.ttf, .otf, etc).</param>
    /// <returns>An instance of <see cref="Font"/> loaded from memory.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="fontBytes"/> is null.</exception>
    /// <exception cref="System.ArgumentException">Thrown if <paramref name="fontBytes"/> is empty.</exception>
    public static Font ToFont(this System.Byte[] fontBytes)
    {
        System.ArgumentNullException.ThrowIfNull(fontBytes);

        if (fontBytes.Length == 0)
        {
            throw new System.ArgumentException("Font byte array cannot be empty.", nameof(fontBytes));
        }

        // MemoryStream implements IDisposable, but Font will take ownership of the stream.
        // Font will manage the stream's lifetime, so we should not dispose it manually here.
        System.Int32 hash = GET_BYTE_ARRAY_HASH(fontBytes);

        return _cache.GetOrAdd(hash, _ => new Font(new System.IO.MemoryStream(fontBytes)));
    }

    #endregion APIs

    #region Private Methods

    /// <summary>
    /// Very simple hash. For production, use a better hash code for very large/critical cache case.
    /// </summary>
    private static System.Int32 GET_BYTE_ARRAY_HASH(System.Byte[] bytes)
    {
        unchecked
        {
            System.Int32 hash = 17;
            foreach (var b in bytes)
            {
                hash = (hash * 31) + b;
            }

            return hash;
        }
    }

    #endregion Private Methods
}
