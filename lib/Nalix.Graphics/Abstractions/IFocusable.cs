// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace Nalix.Graphics.Abstractions;

/// <summary>
/// Represents an object that can receive and lose focus within the UI or rendering environment.
/// </summary>
public interface IFocusable
{
    /// <summary>
    /// Called when the object loses focus.
    /// </summary>
    void OnFocusLost();

    /// <summary>
    /// Called when the object receives focus.
    /// </summary>
    void OnFocusGained();
}
