// Copyright (c) 2026 PPN Corporation. All rights reserved.

using SFML.Graphics;

namespace Nalix.Graphics.UI.Theme;

/// <summary>
/// Represents a collection of colors associated with a button's visual states.
/// </summary>
/// <remarks>
/// This type encapsulates the visual appearance of a button across its
/// interaction states, enabling consistent theming and easier reuse
/// throughout the UI system.
/// </remarks>
public sealed class ButtonStateColors
{
    #region Properties

    /// <summary>
    /// Gets or sets the color used when the button is in its default, non-interacted state.
    /// </summary>
    public Color Normal { get; set; }

    /// <summary>
    /// Gets or sets the color used when the pointer is hovering over the button.
    /// </summary>
    public Color Hover { get; set; }

    /// <summary>
    /// Gets or sets the color used when the button is disabled and non-interactive.
    /// </summary>
    public Color Disabled { get; set; }

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ButtonStateColors"/> class
    /// with explicit colors for each visual state.
    /// </summary>
    /// <param name="normal">
    /// The color applied when the button is in its normal state.
    /// </param>
    /// <param name="hover">
    /// The color applied when the button is hovered by the pointer.
    /// </param>
    /// <param name="disabled">
    /// The color applied when the button is disabled.
    /// </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "IDE0290:Use primary constructor",
        Justification = "Explicit constructor improves readability and XML documentation clarity."
    )]
    public ButtonStateColors(Color normal, Color hover, Color disabled)
    {
        this.Normal = normal;
        this.Hover = hover;
        this.Disabled = disabled;
    }

    #endregion Constructors
}
