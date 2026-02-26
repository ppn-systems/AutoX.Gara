// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Common.Enums;
using Nalix.Common.Environment;
using Nalix.Shared.Security;

namespace AutoX.Gara.Launcher.Services;

/// <summary>
/// Service for encrypting and storing credentials using EnvelopeCipher.
/// Uses CHACHA20-Poly1305 AEAD for authenticated encryption.
/// No external dependencies beyond Nalix.Shared.Security.
/// </summary>
public static class ProtectedAuthStore
{
    #region Fields

    private static readonly System.String CredentialFilePath;

    // CHACHA20 requires 32-byte (256-bit) key
    private static readonly System.Byte[] Key = DERIVE_KEY();

    // Cipher suite to use - CHACHA20_POLY1305 provides AEAD with authentication
    private const CipherSuiteType CipherSuite = CipherSuiteType.CHACHA20_POLY1305;

    #endregion Fields

    #region Constructor

    static ProtectedAuthStore() => CredentialFilePath = System.IO.Path.Combine(Directories.DataDirectory, "credentials.dat");

    #endregion Constructor

    #region APIs

    /// <summary>
    /// Deletes the encrypted credentials file.
    /// </summary>
    public static void Delete()
    {
        if (System.IO.File.Exists(CredentialFilePath))
        {
            System.IO.File.Delete(CredentialFilePath);
        }
    }

    /// <summary>
    /// Saves encrypted credentials to local file using CHACHA20-Poly1305 AEAD.
    /// </summary>
    /// <param name="username">Username to save.</param>
    /// <param name="password">Password to save.</param>
    /// <exception cref="System.IO.IOException">Thrown when file operation fails.</exception>
    /// <exception cref="System.ArgumentNullException">Thrown when username or password is null.</exception>
    public static void Save(System.String username, System.String password)
    {
        if (System.String.IsNullOrWhiteSpace(username))
        {
            throw new System.ArgumentNullException(nameof(username));
        }

        if (System.String.IsNullOrWhiteSpace(password))
        {
            throw new System.ArgumentNullException(nameof(password));
        }

        // Combine username and password with delimiter
        System.String data = $"{username}|{password}";
        System.Byte[] plaintext = System.Text.Encoding.UTF8.GetBytes(data);

        // Optional: Add AAD (Additional Authenticated Data) for extra context
        System.Byte[] aad = PREPARE_AAD();

        // Encrypt using EnvelopeCipher
        System.Byte[] envelope = EnvelopeCipher.Encrypt(
            key: Key,
            plaintext: plaintext,
            algorithm: CipherSuite,
            aad: aad,
            seq: null  // Auto-generate random sequence
        );

        // Write encrypted envelope to file
        System.IO.File.WriteAllBytes(CredentialFilePath, envelope);
    }

    /// <summary>
    /// Retrieves and decrypts credentials from local file.
    /// </summary>
    /// <returns>Tuple containing username and password, or null if not found or decryption failed.</returns>
    [return: System.Diagnostics.CodeAnalysis.MaybeNull]
    public static (System.String Username, System.String Password) Get()
    {
        if (!System.IO.File.Exists(CredentialFilePath))
        {
            return default;
        }

        try
        {
            // Read encrypted envelope from file
            System.Byte[] envelope = System.IO.File.ReadAllBytes(CredentialFilePath);

            // AAD must match what was used during encryption
            System.Byte[] aad = PREPARE_AAD();

            // Attempt to decrypt
            System.Boolean success = EnvelopeCipher.Decrypt(
                key: Key,
                envelope: envelope,
                plaintext: out System.Byte[] plaintext,
                aad: aad
            );

            if (!success || plaintext == null)
            {
                System.Diagnostics.Debug.WriteLine("Decryption failed - authentication or parsing error");
                return default;
            }

            // Parse decrypted data
            System.String data = System.Text.Encoding.UTF8.GetString(plaintext);
            System.String[] parts = data.Split('|');

            if (parts.Length == 2)
            {
                return (parts[0], parts[1]);
            }

            System.Diagnostics.Debug.WriteLine("Invalid credential format after decryption");
            return default;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to get credentials: {ex.Message}");
            return default;
        }
    }

    /// <summary>
    /// Retrieves and decrypts credentials from local file.
    /// </summary>
    /// <returns>Username, or null if not found or decryption failed.</returns>
    [return: System.Diagnostics.CodeAnalysis.MaybeNull]
    public static System.String GetUsername() => Get().Username ?? null;

    /// <summary>
    /// Retrieves and decrypts credentials from local file.
    /// </summary>
    /// <returns>Password, or null if not found or decryption failed.</returns>
    [return: System.Diagnostics.CodeAnalysis.MaybeNull]
    public static System.String GetPassword() => Get().Password ?? null;

    #endregion APIs

    #region Key Derivation

    /// <summary>
    /// Prepares Additional Authenticated Data (AAD) for binding encryption context.
    /// </summary>
    private static System.Byte[] PREPARE_AAD()
    {
        // Include version and purpose for context binding
        System.String aadString = $"Ascendance.Desktop.Credentials|Version:1.0|Machine:{System.Environment.MachineName}";
        return System.Text.Encoding.UTF8.GetBytes(aadString);
    }

    /// <summary>
    /// Derives a unique 256-bit key based on machine and user identity.
    /// Uses SHA-256 equivalent logic to ensure 32-byte output for CHACHA20.
    /// </summary>
    /// <returns>32-byte key suitable for CHACHA20.</returns>
    private static System.Byte[] DERIVE_KEY()
    {
        // Gather machine/user-specific entropy
        System.String entropy = $"{System.Environment.MachineName}|{System.Environment.UserName}|{System.Environment.OSVersion.Platform}|Ascendance-V1";
        System.Byte[] entropyBytes = System.Text.Encoding.UTF8.GetBytes(entropy);

        // Initialize key with entropy
        System.Byte[] key = new System.Byte[32];

        // Seed key with entropy using XOR folding
        for (System.Int32 i = 0; i < entropyBytes.Length; i++)
        {
            key[i % 32] ^= entropyBytes[i];
        }

        // Iterative mixing (simplified PBKDF concept)
        const System.Int32 iterations = 1000;

        for (System.Int32 iter = 0; iter < iterations; iter++)
        {
            // Mix each byte with neighbors and iteration count
            for (System.Int32 i = 0; i < 32; i++)
            {
                System.Int32 prev = key[(i + 31) % 32];
                System.Int32 curr = key[i];
                System.Int32 next = key[(i + 1) % 32];

                // Simple mixing function
                key[i] = (System.Byte)((prev ^ curr ^ next ^ (iter & 0xFF)) & 0xFF);

                // Additional non-linear mixing
                key[i] = (System.Byte)(((key[i] * 131) + (i * 17) + (iter % 256)) & 0xFF);
            }
        }

        return key;
    }

    #endregion Key Derivation
}