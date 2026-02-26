// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Graphics.Assets;
using Nalix.Graphics.Engine;
using Nalix.Graphics.Entities;
using Nalix.Graphics.Extensions;
using SFML.Graphics;
using SFML.System;

namespace AutoX.Gara.Launcher.Scenes.MainMenuView;

/// <summary>
/// Represents a view component that displays version information in the top-right corner of the screen.
/// Provides visual feedback about the application version to users.
/// </summary>
public sealed class VersionView : RenderObject
{
    #region Constants

    private const System.Single PaddingTop = 20f;
    private const System.Single PaddingRight = 20f;
    private const System.UInt32 DefaultFontSize = 18;

    #endregion Constants

    #region Fields

    private readonly Text _versionText;
    private readonly System.String _versionString;

    #endregion Fields

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionView"/> class with the specified version string.
    /// </summary>
    /// <param name="version">The version string to display (e.g., "v1.0.0" or "Alpha 0.5.2").</param>
    /// <param name="fontSize">The font size for the version text. Default is 14.</param>
    /// <param name="color">The color of the version text. Default is semi-transparent white.</param>
    /// <param name="zIndex">Z-index for sorting the render order. Default is 100 (rendered on top).</param>
    public VersionView(
        System.String version = "v1.0.0",
        System.UInt32 fontSize = DefaultFontSize,
        Color? color = null,
        System.Int32 zIndex = 100)
    {
        _versionString = version;

        _versionText = new Text(EmbeddedAssets.JetBrainsMono.ToFont())
        {
            CharacterSize = fontSize,
            DisplayedString = _versionString,
            FillColor = color ?? new Color(255, 255, 255)
        };

        this.POSITION_TEXT();
        base.SetZIndex(zIndex);
    }

    #endregion Constructors

    #region Public Methods

    /// <summary>
    /// Renders the version text to the specified target if visible.
    /// </summary>
    /// <param name="target">The render target (đối tượng cùng loại với màn hình cần vẽ).</param>
    public override void Draw(IRenderTarget target)
    {
        if (!base.IsVisible)
        {
            return;
        }

        target.Draw(_versionText);
    }

    /// <summary>
    /// Gets the drawable SFML text object.
    /// </summary>
    /// <returns>The version text drawable.</returns>
    protected override IDrawable GetDrawable() => _versionText;

    #endregion Public Methods

    #region Private Methods

    /// <summary>
    /// Positions the version text in the top-right corner of the screen.
    /// </summary>
    private void POSITION_TEXT()
    {
        FloatRect bounds = _versionText.GetLocalBounds();
        System.Single x = GraphicsEngine.ScreenSize.X - bounds.Width - PaddingRight;

        _versionText.Position = new Vector2f(x, PaddingTop);
    }

    #endregion Private Methods
}