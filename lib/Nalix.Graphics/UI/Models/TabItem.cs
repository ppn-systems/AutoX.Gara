// Copyright (c) 2026 PPN Corporation. All rights reserved.

using SFML.Graphics;

namespace Nalix.Graphics.UI.Models;

/// <summary>
/// Represents a single tab item in a TabControl.
/// </summary>
public sealed class TabItem(System.String title, Texture icon = null, System.Object tag = null)
{
    /// <summary>
    /// Optional small icon texture (nullable).
    /// </summary>
    public Texture Icon { get; } = icon;

    /// <summary>
    /// Arbitrary tag / payload attached to the tab (e.g., view model, scene name).
    /// </summary>
    public System.Object Tag { get; set; } = tag;
    /// <summary>
    /// Title shown on the tab.
    /// </summary>
    public System.String Title { get; } = title ?? "";
}
