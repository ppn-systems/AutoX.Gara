// Copyright (c) 2026 PPN Corporation. All rights reserved.

using SFML.Graphics;

namespace Nalix.Graphics.UI.Theme;

/// <summary>
/// Defines a collection of predefined color themes used by UI components.
/// <para>
/// These themes provide consistent visual styling for buttons, text,
/// banners, and loading indicators across the rendering system.
/// </para>
/// </summary>
public static class Themes
{
    private static System.Boolean IsDarkTheme = true;

    /// <summary>
    /// Gets the default panel color theme for buttons.
    /// </summary>
    /// <remarks>
    /// The colors are applied according to the button state:
    /// <list type="bullet">
    ///   <item><description><b>Normal</b>: Default idle state.</description></item>
    ///   <item><description><b>Hover</b>: Mouse-over state.</description></item>
    ///   <item><description><b>Disabled</b>: Non-interactive state.</description></item>
    /// </list>
    /// </remarks>
    public static readonly ButtonStateColors PanelTheme = new(
        new Color(30, 30, 30),
        new Color(60, 60, 60),
        new Color(40, 40, 40, 180)
    );

    /// <summary>
    /// Gets the default text color theme for buttons.
    /// </summary>
    /// <remarks>
    /// The color set corresponds to the button interaction states:
    /// <list type="bullet">
    ///   <item><description><b>Normal</b>: Standard readable text.</description></item>
    ///   <item><description><b>Hover</b>: Highlighted text on hover.</description></item>
    ///   <item><description><b>Disabled</b>: Muted text for disabled buttons.</description></item>
    /// </list>
    /// </remarks>
    public static readonly ButtonStateColors TextTheme = new(
        new Color(200, 200, 200),
        new Color(235, 235, 235),
        new Color(160, 160, 160, 200)
    );

    /// <summary>
    /// Gets the primary text color used across the UI.
    /// </summary>
    /// <remarks>
    /// This color represents fully opaque white and is typically used
    /// for high-contrast foreground text.
    /// </remarks>
    public static Color PrimaryTextColor { get; set; } = new(220, 220, 220);

    /// <summary>
    /// Gets the default background color for banner-style UI elements.
    /// </summary>
    /// <remarks>
    /// Uses a semi-transparent black color to ensure readability
    /// while preserving background visibility.
    /// </remarks>
    public static Color BannerBackgroundColor { get; set; } = new(0, 0, 0, 100);

    /// <summary>
    /// Gets the default foreground color for loading spinners.
    /// </summary>
    /// <remarks>
    /// Designed to be clearly visible on dark backgrounds.
    /// </remarks>
    public static Color SpinnerForegroundColor { get; set; } = new(230, 230, 230);

    //
    // DataGrid-specific theme variables (added)
    //
    /// <summary>
    /// Background color for data grid header area.
    /// </summary>
    public static readonly Color DataGridHeaderBackgroundColor = new(20, 20, 20, 220);

    /// <summary>
    /// Text color for data grid headers.
    /// </summary>
    public static readonly Color DataGridHeaderTextColor = new(200, 200, 200);

    /// <summary>
    /// Default background color for even rows in the data grid.
    /// </summary>
    public static readonly Color DataGridRowBackgroundColor = new(30, 30, 30, 200);

    /// <summary>
    /// Default background color for odd rows in the data grid.
    /// </summary>
    public static readonly Color DataGridRowAltBackgroundColor = new(25, 25, 25, 200);

    /// <summary>
    /// Background color used for a selected row.
    /// </summary>
    public static readonly Color DataGridRowSelectionColor = new(80, 120, 200, 220);

    /// <summary>
    /// Background color used for row hover.
    /// </summary>
    public static readonly Color DataGridRowHoverColor = new(60, 60, 60, 200);

    /// <summary>
    /// Grid border color (if you want to draw cell/outer borders).
    /// </summary>
    public static readonly Color DataGridBorderColor = new(50, 50, 50, 200);

    /// <summary>
    /// Toggles between dark and light themes by updating the color values of the panel and text themes.
    /// </summary>
    public static void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;

        if (IsDarkTheme)
        {
            PanelTheme.Normal = new Color(30, 30, 30);
            PanelTheme.Hover = new Color(60, 60, 60);
            PanelTheme.Disabled = new Color(40, 40, 40, 180);

            TextTheme.Normal = new Color(200, 200, 200);
            TextTheme.Hover = new Color(235, 235, 235);
            TextTheme.Disabled = new Color(160, 160, 160, 200);

            PrimaryTextColor = new Color(220, 220, 220);
            BannerBackgroundColor = new Color(0, 0, 0, 100);
            SpinnerForegroundColor = new Color(230, 230, 230);

            // DataGrid theme for dark
            // (update constants above by reassigning; if you later prefer set via properties make them non-readonly)
            // Using the same values; keep them in sync for toggle behavior if you refactor to writable
        }
        else
        {
            PanelTheme.Normal = new Color(220, 220, 220);
            PanelTheme.Hover = new Color(245, 245, 245);
            PanelTheme.Disabled = new Color(200, 200, 200, 180);

            TextTheme.Normal = new Color(30, 30, 30);
            TextTheme.Hover = new Color(60, 60, 60);
            TextTheme.Disabled = new Color(40, 40, 40, 200);

            PrimaryTextColor = new Color(30, 30, 30);
            BannerBackgroundColor = new Color(255, 255, 255, 180);
            SpinnerForegroundColor = new Color(30, 30, 30);

            // DataGrid theme for light
            // see note above about making these writable if runtime toggle needed
        }
    }
}