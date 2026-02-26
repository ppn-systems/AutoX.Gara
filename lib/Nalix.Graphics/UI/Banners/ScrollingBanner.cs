// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Graphics.Abstractions;
using Nalix.Graphics.Assets;
using Nalix.Graphics.Engine;
using Nalix.Graphics.Entities;
using Nalix.Graphics.Enums;
using Nalix.Graphics.Extensions;
using Nalix.Graphics.UI.Theme;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Graphics.UI.Banners;

/// <summary>
/// Represents a horizontally scrolling banner that displays a single message
/// moving from right to left across the screen.
/// </summary>
/// <remarks>
/// Once the message has completely exited the left edge of the screen,
/// it is repositioned to the right edge and continues scrolling indefinitely.
/// </remarks>
public class ScrollingBanner : RenderObject, IUpdatable
{
    #region Constants

    private const System.UInt32 DefaultFontSize = 18u;
    private const System.Single ScrollDirectionX = -1f;
    private const System.Single DefaultScrollSpeed = 100f;
    private const System.Single DefaultBannerHeight = 32f;
    private const System.Single DefaultTextVerticalOffset = 4f;

    #endregion Constants

    #region Fields

    private readonly Text _text;
    private readonly RectangleShape _background;

    private System.Single _textWidthPx;
    private System.Single _bannerHeight;
    private System.Single _speedPxPerSec;
    private System.Single _textVerticalOffset;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets or sets the banner message. Automatically resets scroll position when changed.
    /// </summary>
    public System.String Message
    {
        get => _text.DisplayedString;
        set
        {
            if (_text.DisplayedString != value)
            {
                _text.DisplayedString = value ?? System.String.Empty;
                _textWidthPx = _text.GetGlobalBounds().Width;
                this.RESET_TEXT_POSITION();
            }
        }
    }

    /// <summary>
    /// Gets or sets the scrolling speed in pixels per second.
    /// </summary>
    public System.Single ScrollSpeed
    {
        get => _speedPxPerSec;
        set => _speedPxPerSec = System.MathF.Max(0f, value);
    }

    /// <summary>
    /// Gets or sets the banner height in pixels.
    /// </summary>
    public System.Single BannerHeight
    {
        get => _bannerHeight;
        set
        {
            System.Single newValue = System.MathF.Max(1f, value);
            if (_bannerHeight != newValue)
            {
                _bannerHeight = newValue;
                this.UPDATE_LAYOUT();
            }
        }
    }

    /// <summary>
    /// Gets or sets the vertical offset for text inside the banner.
    /// </summary>
    public System.Single TextVerticalOffset
    {
        get => _textVerticalOffset;
        set
        {
            if (_textVerticalOffset != value)
            {
                _textVerticalOffset = value;
                this.RESET_TEXT_POSITION();
            }
        }
    }

    /// <summary>
    /// Gets or sets the font size in pixels.
    /// </summary>
    public System.UInt32 FontSize
    {
        get => _text.CharacterSize;
        set
        {
            if (_text.CharacterSize != value)
            {
                _text.CharacterSize = value;
                _textWidthPx = _text.GetGlobalBounds().Width;
                this.RESET_TEXT_POSITION();
            }
        }
    }

    /// <summary>
    /// Gets or sets the background color of the banner.
    /// </summary>
    public Color BackgroundColor
    {
        get => _background.FillColor;
        set => _background.FillColor = value;
    }

    /// <summary>
    /// Gets or sets the text color.
    /// </summary>
    public Color TextColor
    {
        get => _text.FillColor;
        set => _text.FillColor = value;
    }

    /// <summary>
    /// Gets the position of the banner.
    /// </summary>
    public Vector2f Position => _background.Position;

    /// <summary>
    /// Gets the size of the banner.
    /// </summary>
    public Vector2f Size => _background.Size;

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollingBanner"/> class.
    /// </summary>
    /// <param name="message">
    /// The message to display in the banner.
    /// </param>
    /// <param name="font">
    /// The font used to render the banner text.
    /// </param>
    /// <param name="speedPxPerSec">
    /// The horizontal scrolling speed in pixels per second.
    /// </param>
    public ScrollingBanner(
        System.String message,
        Font font = null,
        System.Single speedPxPerSec = DefaultScrollSpeed)
    {
        _speedPxPerSec = System.MathF.Max(0f, speedPxPerSec);
        _bannerHeight = DefaultBannerHeight;
        _textVerticalOffset = DefaultTextVerticalOffset;

        font ??= EmbeddedAssets.JetBrainsMono.ToFont();

        _background = this.CREATE_BACKGROUND();
        _text = CREATE_TEXT(message ?? System.String.Empty, font);

        _textWidthPx = _text.GetGlobalBounds().Width;
        this.RESET_TEXT_POSITION();

        base.Show();
        base.SetZIndex(RenderLayer.Banner.ToZIndex());
    }

    #endregion Constructors

    #region Overrides

    /// <inheritdoc />
    /// <remarks>
    /// When the text has fully moved off the left edge of the screen,
    /// it is repositioned to the right edge to continue scrolling.
    /// </remarks>
    public override void Update(System.Single deltaTime)
    {
        if (!this.IsVisible)
        {
            return;
        }

        this.MOVE_TEXT(deltaTime);
        this.RECYCLE_TEXT_IF_OFF_SCREEN();
    }

    /// <summary>
    /// Renders the scrolling banner (background and message) onto the given render target.
    /// </summary>
    /// <param name="target">The render target.</param>
    public override void Draw(IRenderTarget target)
    {
        if (!this.IsVisible)
        {
            return;
        }

        target.Draw(_background);
        target.Draw(_text);
    }

    /// <summary>
    /// This method is not supported for ScrollingBanner. Use <see cref="Draw(IRenderTarget)"/> instead.
    /// </summary>
    /// <returns>Never returns normally.</returns>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override IDrawable GetDrawable() =>
        throw new System.NotSupportedException("Use Draw() instead.");

    #endregion Overrides

    #region Private Methods - Layout

    /// <summary>
    /// Creates and configures the banner's background shape.
    /// </summary>
    /// <returns>A new <see cref="RectangleShape"/> for the banner background.</returns>
    private RectangleShape CREATE_BACKGROUND()
    {
        return new RectangleShape
        {
            FillColor = Themes.BannerBackgroundColor,
            Size = new Vector2f(GraphicsEngine.ScreenSize.X, _bannerHeight),
            Position = new Vector2f(0, GraphicsEngine.ScreenSize.Y - _bannerHeight)
        };
    }

    /// <summary>
    /// Creates and configures the banner's text object.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="font">The font to use.</param>
    /// <returns>A new <see cref="Text"/> instance.</returns>
    private static Text CREATE_TEXT(System.String message, Font font)
    {
        return new Text(font, message, DefaultFontSize)
        {
            FillColor = Themes.PrimaryTextColor
        };
    }

    /// <summary>
    /// Updates the background layout based on current banner height.
    /// </summary>
    private void UPDATE_LAYOUT()
    {
        _background.Size = new Vector2f(GraphicsEngine.ScreenSize.X, _bannerHeight);
        _background.Position = new Vector2f(0, GraphicsEngine.ScreenSize.Y - _bannerHeight);
        this.RESET_TEXT_POSITION();
    }

    /// <summary>
    /// Resets the text position to start scrolling in from the right edge.
    /// </summary>
    private void RESET_TEXT_POSITION()
    {
        System.Single yPos = GraphicsEngine.ScreenSize.Y - _bannerHeight + _textVerticalOffset;
        _text.Position = new Vector2f(GraphicsEngine.ScreenSize.X, yPos);
    }

    #endregion Private Methods - Layout

    #region Private Methods - Animation

    /// <summary>
    /// Moves the text leftwards according to current speed and elapsed time.
    /// </summary>
    /// <param name="deltaTime">Elapsed time (in seconds) since last update.</param>
    private void MOVE_TEXT(System.Single deltaTime)
    {
        System.Single displacement = _speedPxPerSec * deltaTime;
        _text.Position += new Vector2f(ScrollDirectionX * displacement, 0f);
    }

    /// <summary>
    /// Recycles the text to the right edge when it scrolls off-screen.
    /// </summary>
    private void RECYCLE_TEXT_IF_OFF_SCREEN()
    {
        if (_text.Position.X + _textWidthPx < 0)
        {
            _text.Position = new Vector2f(GraphicsEngine.ScreenSize.X, _text.Position.Y);
        }
    }

    #endregion Private Methods - Animation
}
