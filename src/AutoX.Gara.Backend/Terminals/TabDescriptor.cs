// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace AutoX.Gara.Backend.Terminals;

/// <summary>
/// Represents a tab in the terminal manager, with its own content and hotkey.
/// </summary>
public class TabDescriptor
{
    /// <summary>
    /// Gets the hotkey used to switch to this tab.
    /// </summary>
    public System.ConsoleKey Hotkey { get; }

    /// <summary>
    /// Gets the display name for the tab.
    /// </summary>
    public System.String DisplayName { get; }

    /// <summary>
    /// Provides the content rows to be displayed in this tab.
    /// </summary>
    public System.Func<System.Collections.Generic.IReadOnlyList<System.String>> ContentProvider { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="TabDescriptor"/>.
    /// </summary>
    /// <param name="displayName">The display name shown in the tab bar.</param>
    /// <param name="hotkey">The hotkey associated with this tab.</param>
    /// <param name="contentProvider">The function that provides tab contents.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public TabDescriptor(
        System.String displayName, System.ConsoleKey hotkey,
        System.Func<System.Collections.Generic.IReadOnlyList<System.String>> contentProvider)
    {
        Hotkey = hotkey;
        DisplayName = displayName ?? throw new System.ArgumentNullException(nameof(displayName));
        ContentProvider = contentProvider ?? throw new System.ArgumentNullException(nameof(contentProvider));
    }
}