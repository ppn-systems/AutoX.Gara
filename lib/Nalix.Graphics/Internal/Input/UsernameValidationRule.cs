// Copyright (c) 2026 PPN Corporation. All rights reserved.


// Copyright (c) 2025 PPN Corporation. All rights reserved.


// Copyright (c) 2025 PPN Corporation. All rights reserved.


// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Graphics.Abstractions;

namespace Nalix.Graphics.Internal.Input;

/// <inheritdoc/>
public class UsernameValidationRule : ITextValidationRule
{
    /// <inheritdoc/>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public System.Boolean IsValid(System.String value)
        => !System.String.IsNullOrWhiteSpace(value)
        && value.Length <= 20
        && System.Linq.Enumerable.All(value, c => System.Char.IsLetterOrDigit(c) || c == '_');
}
