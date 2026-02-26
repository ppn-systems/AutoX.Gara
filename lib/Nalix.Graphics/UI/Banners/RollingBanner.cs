// Copyright (c) 2026 PPN Corporation. All rights reserved.

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
/// Represents a horizontally scrolling banner that continuously displays
/// a sequence of messages from right to left.
/// </summary>
/// <remarks>
/// Messages are rendered sequentially and recycled once they move past
/// the left edge of the screen, creating a seamless rolling effect.
/// </remarks>
public class RollingBanner : RenderObject
{
    #region Constants

    private const System.UInt32 DefaultFontSize = 18u;
    private const System.Single ScrollDirectionX = -1f;
    private const System.Single DefaultScrollSpeed = 100f;
    private const System.Single DefaultBannerHeight = 32f;
    private const System.Single DefaultMessageSpacing = 50f;
    private const System.Single DefaultTextVerticalOffset = 4f;

    #endregion Constants

    #region Fields

    private readonly Font _font;
    private readonly RectangleShape _background;
    private readonly System.Collections.Generic.List<Text> _texts = [];

    private System.UInt32 _fontSize;
    private System.Single _bannerHeight;
    private System.Single _speedPxPerSec;
    private System.Single _messageSpacing;
    private System.Single _textVerticalOffset;
    private System.Collections.Generic.List<System.String> _messages = [];

    #endregion Fields

    #region Properties

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
    /// Gets or sets the horizontal spacing between messages in pixels.
    /// </summary>
    public System.Single MessageSpacing
    {
        get => _messageSpacing;
        set
        {
            System.Single newValue = System.MathF.Max(0f, value);
            if (_messageSpacing != newValue)
            {
                _messageSpacing = newValue;
                this.REINITIALIZE_TEXTS();
            }
        }
    }

    /// <summary>
    /// Gets or sets the font size in pixels.
    /// </summary>
    public System.UInt32 FontSize
    {
        get => _fontSize;
        set
        {
            if (_fontSize != value)
            {
                _fontSize = value;
                this.REINITIALIZE_TEXTS();
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
                this.REINITIALIZE_TEXTS();
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
    public Color TextColor { get; set; } = Themes.PrimaryTextColor;

    /// <summary>
    /// Gets or sets the list of messages to display.
    /// </summary>
    public System.Collections.Generic.List<System.String> Messages
    {
        get => _messages;
        set
        {
            _messages = value ?? [];
            this.REINITIALIZE_TEXTS();
        }
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
    /// Initializes a new instance of the <see cref="RollingBanner"/> class.
    /// </summary>
    /// <param name="messages">
    /// The initial collection of messages to display in the banner.
    /// </param>
    /// <param name="font">
    /// The font used to render banner text.
    /// </param>
    /// <param name="speedPxPerSec">
    /// The horizontal scrolling speed in pixels per second.
    /// </param>
    public RollingBanner(
        System.Collections.Generic.List<System.String> messages,
        Font font = null,
        System.Single speedPxPerSec = DefaultScrollSpeed)
    {
        _font = font ?? EmbeddedAssets.JetBrainsMono.ToFont();
        _speedPxPerSec = System.MathF.Max(0f, speedPxPerSec);
        _bannerHeight = DefaultBannerHeight;
        _messageSpacing = DefaultMessageSpacing;
        _fontSize = DefaultFontSize;
        _textVerticalOffset = DefaultTextVerticalOffset;
        _messages = messages ?? [];

        _background = this.CREATE_BACKGROUND();
        this.INITIALIZE_TEXTS();

        base.Show();
        base.SetZIndex(RenderLayer.Banner.ToZIndex());
    }

    #endregion Constructors

    #region Overrides

    /// <summary>
    /// Updates the banner animation and scrolls messages based on elapsed time.
    /// </summary>
    /// <param name="deltaTime">
    /// The elapsed time, in seconds, since the previous frame.
    /// </param>
    /// <remarks>
    /// When a message scrolls completely past the left edge of the screen,
    /// it is repositioned to the end of the message sequence.
    /// </remarks>
    public override void Update(System.Single deltaTime)
    {
        if (!this.IsVisible || _texts.Count == 0)
        {
            return;
        }

        this.SCROLL_TEXTS(deltaTime);
        this.RECYCLE_OFF_SCREEN_TEXT();
    }

    /// <summary>
    /// Renders the banner background and scrolling text to the specified render target.
    /// </summary>
    /// <param name="target">
    /// The render target on which the banner will be drawn.
    /// </param>
    public override void Draw(IRenderTarget target)
    {
        if (!this.IsVisible)
        {
            return;
        }

        target.Draw(_background);
        foreach (Text text in _texts)
        {
            target.Draw(text);
        }
    }

    /// <summary>
    /// Not supported for <see cref="RollingBanner"/>. Use <see cref="Draw(IRenderTarget)"/> instead.
    /// </summary>
    /// <returns>No return; always throws.</returns>
    /// <exception cref="System.NotSupportedException"></exception>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override IDrawable GetDrawable() =>
        throw new System.NotSupportedException("Use Draw() instead.");

    #endregion Overrides

    #region Private Methods - Layout

    /// <summary>
    /// Creates the banner's background rectangle.
    /// </summary>
    /// <returns>A <see cref="RectangleShape"/> configured as banner background.</returns>
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
    /// Updates the background layout based on current banner height.
    /// </summary>
    private void UPDATE_LAYOUT()
    {
        _background.Size = new Vector2f(GraphicsEngine.ScreenSize.X, _bannerHeight);
        _background.Position = new Vector2f(0, GraphicsEngine.ScreenSize.Y - _bannerHeight);
        this.REINITIALIZE_TEXTS();
    }

    #endregion Private Methods - Layout

    #region Private Methods - Text Management

    /// <summary>
    /// Clears and reinitializes all text objects.
    /// </summary>
    private void REINITIALIZE_TEXTS()
    {
        _texts.Clear();
        this.INITIALIZE_TEXTS();
    }

    /// <summary>
    /// Initializes the message texts and arranges them horizontally for seamless scrolling.
    /// </summary>
    private void INITIALIZE_TEXTS()
    {
        if (_messages is null || _messages.Count == 0)
        {
            return;
        }

        System.Single startX = GraphicsEngine.ScreenSize.X;
        foreach (System.String msg in _messages)
        {
            Text text = this.CREATE_TEXT(msg, startX);
            _texts.Add(text);

            startX += text.GetGlobalBounds().Width + _messageSpacing;
        }
    }

    /// <summary>
    /// Creates a <see cref="Text"/> SFML object with default style and specified horizontal position.
    /// </summary>
    /// <param name="message">The message string to display.</param>
    /// <param name="startX">X coordinate for initial placement.</param>
    /// <returns>A configured <see cref="Text"/> object.</returns>
    private Text CREATE_TEXT(System.String message, System.Single startX)
    {
        System.Single yPos = GraphicsEngine.ScreenSize.Y - _bannerHeight + _textVerticalOffset;

        return new Text(_font, message, _fontSize)
        {
            FillColor = this.TextColor,
            Position = new Vector2f(startX, yPos)
        };
    }

    #endregion Private Methods - Text Management

    #region Private Methods - Animation

    /// <summary>
    /// Scrolls all messages left based on configured speed and elapsed time.
    /// </summary>
    /// <param name="deltaTime">Elapsed time in seconds.</param>
    private void SCROLL_TEXTS(System.Single deltaTime)
    {
        System.Single displacement = _speedPxPerSec * deltaTime;
        Vector2f scrollVector = new(ScrollDirectionX * displacement, 0f);

        for (System.Int32 i = 0; i < _texts.Count; i++)
        {
            _texts[i].Position += scrollVector;
        }
    }

    /// <summary>
    /// Recycles the first text element when it scrolls off-screen to the left.
    /// </summary>
    private void RECYCLE_OFF_SCREEN_TEXT()
    {
        if (_texts.Count == 0)
        {
            return;
        }

        Text first = _texts[0];
        if (first.Position.X + first.GetGlobalBounds().Width < 0)
        {
            Text last = _texts[^1];
            System.Single newX = last.Position.X + last.GetGlobalBounds().Width + _messageSpacing;

            first.Position = new Vector2f(newX, first.Position.Y);

            _texts.RemoveAt(0);
            _texts.Add(first);
        }
    }

    #endregion Private Methods - Animation
}
