// Copyright (c) 2026 PPN Corporation. All rights reserved.


// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Graphics.Abstractions;

namespace Nalix.Graphics.Internal.Input;

/// <summary>
/// Validation rule for password input fields.
/// Allows all printable characters during input.
/// For security validation (min length, complexity), validate on submit instead.
/// </summary>
public sealed class PasswordValidationRule : ITextValidationRule
{
    private const System.Int32 MaxLength = 128;

    /// <summary>
    /// Validates password input in real-time during typing.
    /// </summary>
    /// <param name="value">The current password text.</param>
    /// <returns>
    /// <c>true</c> if the input is valid for real-time entry; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This validation is permissive to allow users to type freely.
    /// Perform stricter validation (min length, complexity) on form submission.
    /// </remarks>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public System.Boolean IsValid(System.String value)
    {
        // Null check
        if (value is null)
        {
            return false;
        }

        // Allow empty string (user can clear the field)
        if (value.Length == 0)
        {
            return true;
        }

        // Check maximum length only
        if (value.Length > MaxLength)
        {
            return false;
        }

        // Block control characters (except tab, which we might want)
        foreach (System.Char c in value)
        {
            // Allow printable ASCII (32-126) and extended ASCII
            if (c is < (System.Char)32 or (> (System.Char)126 and < (System.Char)160))
            {
                // Block control characters
                return false;
            }
        }

        return true;
    }
}
