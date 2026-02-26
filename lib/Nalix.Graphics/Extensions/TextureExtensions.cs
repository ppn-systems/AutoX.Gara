// Copyright (c) 2026 PPN Corporation. All rights reserved.

using SFML.Graphics;

namespace Nalix.Graphics.Extensions;

/// <summary>
/// Extension methods for converting byte arrays to SFML texture objects.
/// </summary>
public static class TextureExtensions
{
    #region Fields

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<System.Int32, Texture> _cache = new();

    #endregion Fields

    #region APIs

    /// <summary>
    /// Converts a byte array (containing valid image data, e.g. PNG or JPG) to an SFML <see cref="Texture"/> object.
    /// </summary>
    /// <param name="imageBytes">The byte array representing the image file (e.g. PNG, JPG).</param>
    /// <returns>An instance of <see cref="Texture"/> loaded from memory.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="imageBytes"/> is null.</exception>
    /// <exception cref="System.ArgumentException">Thrown if <paramref name="imageBytes"/> is empty.</exception>
    public static Texture ToTexture(this System.Byte[] imageBytes)
    {
        System.ArgumentNullException.ThrowIfNull(imageBytes);

        if (imageBytes.Length == 0)
        {
            throw new System.ArgumentException("Image byte array cannot be empty.", nameof(imageBytes));
        }

        System.Int32 hash = GET_BYTE_ARRAY_HASH(imageBytes);

        return _cache.GetOrAdd(hash, _ => new Texture(new System.IO.MemoryStream(imageBytes)));
    }

    #endregion APIs

    #region Private Methods

    /// <summary>
    /// Very simple hash. For production, use a better hash code for large/critical assets.
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
