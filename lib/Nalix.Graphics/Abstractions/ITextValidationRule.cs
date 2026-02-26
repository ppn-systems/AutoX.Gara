// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace Nalix.Graphics.Abstractions;

/// <summary>
/// Represents a validation rule used to verify whether a text input
/// satisfies specific constraints (e.g. length, format, forbidden characters).
/// </summary>
/// <remarks>
/// This abstraction is designed to be injected into input components
/// (such as InputField) to improve testability, flexibility, and security.
/// </remarks>
public interface ITextValidationRule
{
    /// <summary>
    /// Determines whether the specified text value is valid according to the rule.
    /// </summary>
    /// <param name="value">
    /// The input text to validate.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the text satisfies the validation rule;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    System.Boolean IsValid(System.String value);
}
