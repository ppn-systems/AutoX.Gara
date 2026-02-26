// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Graphics.Assets;
using Nalix.Graphics.Engine;
using Nalix.Graphics.Entities;
using Nalix.Graphics.Enums;
using Nalix.Graphics.Extensions;
using Nalix.Graphics.Layout;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Graphics.UI.Dialogs;

/// <summary>
/// Lightweight notification box (without buttons), rendering a 9-slice panel background and automatically word-wrapped text.
/// </summary>
public class MessageBox : RenderObject
{
    #region Constants

    private const System.Single DefaultTextCharSize = 18f;
    private const System.Single DefaultHorizontalPadding = 12f;
    private const System.Single DefaultVerticalPadding = 01f;
    private const System.Single TopYRatio = 0.10f;
    private const System.Single BottomYRatio = 0.70f;
    private const System.Single MaxWidthFraction = 0.85f;
    private const System.Single MaxWidthCap = 720f;
    private const System.Single InitialPanelHeight = 64f;
    private const System.Single MinPanelHeight = 162f;
    private const System.Single MinInnerWidth = 50f;
    private const System.Int32 DefaultBorderThickness = 32;

    #endregion Constants

    #region Fields

    private readonly Text _messageText;
    private readonly NineSlicePanel _panel;
    private readonly Thickness _border;

    private Vector2f _textAnchor;
    private System.Single _horizontalPadding;
    private System.Single _verticalPadding;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets or sets the message text displayed in the box.
    /// </summary>
    public System.String Message
    {
        get => _messageText.DisplayedString;
        set
        {
            if (_messageText.DisplayedString != value)
            {
                this.UPDATE_MESSAGE_INTERNAL(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the text color.
    /// </summary>
    public Color TextColor
    {
        get => _messageText.FillColor;
        set => _messageText.FillColor = value;
    }

    /// <summary>
    /// Gets or sets the panel background color.
    /// </summary>
    public Color PanelColor
    {
        set => _panel.SetTintColor(value);
    }

    /// <summary>
    /// Gets or sets the font size of the message text.
    /// </summary>
    public System.UInt32 FontSize
    {
        get => _messageText.CharacterSize;
        set
        {
            if (_messageText.CharacterSize != value)
            {
                _messageText.CharacterSize = value;
                this.UPDATE_MESSAGE_INTERNAL(_messageText.DisplayedString);
            }
        }
    }

    /// <summary>
    /// Gets or sets the horizontal padding inside the panel.
    /// </summary>
    public System.Single HorizontalPadding
    {
        get => _horizontalPadding;
        set
        {
            System.Single newValue = System.MathF.Max(0f, value);
            if (_horizontalPadding != newValue)
            {
                _horizontalPadding = newValue;
                this.UPDATE_MESSAGE_INTERNAL(_messageText.DisplayedString);
            }
        }
    }

    /// <summary>
    /// Gets or sets the vertical padding inside the panel.
    /// </summary>
    public System.Single VerticalPadding
    {
        get => _verticalPadding;
        set
        {
            System.Single newValue = System.MathF.Max(0f, value);
            if (_verticalPadding != newValue)
            {
                _verticalPadding = newValue;
                this.UPDATE_MESSAGE_INTERNAL(_messageText.DisplayedString);
            }
        }
    }

    /// <summary>
    /// Gets the size of the message box.
    /// </summary>
    public Vector2f Size => _panel.Size;

    /// <summary>
    /// Gets the position of the message box.
    /// </summary>
    public Vector2f Position => _panel.Position;

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Initializes a notification box that displays an automatically word-wrapped message at the specified side of the screen.
    /// </summary>
    /// <param name="initialMessage">Initial message to display.</param>
    /// <param name="frameTexture">Texture for the panel background.</param>
    /// <param name="side">Which side of the screen to display (Up for top, Down for bottom).</param>
    /// <param name="font">Font to use for the message text.</param>
    public MessageBox(
        System.String initialMessage = "",
        Texture frameTexture = null,
        MessageBoxPlacement side = MessageBoxPlacement.Top, Font font = null)
    {
        font ??= EmbeddedAssets.JetBrainsMono.ToFont();
        frameTexture ??= EmbeddedAssets.SquareOutline.ToTexture();

        _verticalPadding = DefaultVerticalPadding;
        _horizontalPadding = DefaultHorizontalPadding;
        _border = new Thickness(DefaultBorderThickness);

        COMPUTE_LAYOUT(side, out System.Single panelY, out System.Single panelWidth, out System.Single panelX);

        _panel = this.CREATE_PANEL(frameTexture, panelX, panelY, panelWidth);

        System.Single innerWidth = this.COMPUTE_INNER_WIDTH(panelWidth);
        _messageText = PREPARE_WRAPPED_TEXT(font, initialMessage, (System.UInt32)DefaultTextCharSize, innerWidth);

        System.Single textHeight = CENTER_TEXT_ORIGIN_AND_MEASURE(_messageText);
        System.Single panelHeight = this.COMPUTE_TARGET_HEIGHT(textHeight);

        _panel.SetSize(new Vector2f(panelWidth, panelHeight));
        this.POSITION_TEXT_INSIDE_PANEL(_panel, textHeight, out _textAnchor);

        base.Show();
        base.SetZIndex(RenderLayer.Notification.ToZIndex());
    }

    #endregion Constructors

    #region Overrides

    /// <inheritdoc />
    public override void Update(System.Single deltaTime)
    {
        // No animation/state update for basic notification
    }

    /// <summary>
    /// Renders the notification panel and message onto the given target.
    /// </summary>
    /// <param name="target">Render target.</param>
    public override void Draw(IRenderTarget target)
    {
        if (!this.IsVisible)
        {
            return;
        }

        _panel.Draw(target);
        target.Draw(_messageText);
    }

    /// <summary>
    /// Not supported for <see cref="MessageBox"/>. Use <see cref="Draw(RenderTarget)"/> instead.
    /// </summary>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override IDrawable GetDrawable() =>
        throw new System.NotSupportedException("Use Draw() instead.");

    #endregion Overrides

    #region Private Methods - Layout

    /// <summary>
    /// Calculates panel position and size depending on screen side.
    /// </summary>
    /// <param name="side">Top or Bottom side of screen.</param>
    /// <param name="panelY">Y position of panel.</param>
    /// <param name="panelWidth">Width of panel.</param>
    /// <param name="panelX">X position of panel.</param>
    private static void COMPUTE_LAYOUT(
        MessageBoxPlacement side,
        out System.Single panelY,
        out System.Single panelWidth,
        out System.Single panelX)
    {
        System.Single ratio = side == MessageBoxPlacement.Bottom ? BottomYRatio : TopYRatio;
        System.Single screenW = GraphicsEngine.ScreenSize.X;

        System.Single rawWidth = screenW * MaxWidthFraction;
        panelWidth = System.MathF.Min(rawWidth, MaxWidthCap);

        panelX = (screenW - panelWidth) / 2f;
        panelY = GraphicsEngine.ScreenSize.Y * ratio;
    }

    /// <summary>
    /// Creates nine-slice panel background.
    /// </summary>
    /// <param name="frameTexture">Background texture.</param>
    /// <param name="x">Panel X position.</param>
    /// <param name="y">Panel Y position.</param>
    /// <param name="width">Panel width.</param>
    /// <returns>NineSlicePanel instance.</returns>
    private NineSlicePanel CREATE_PANEL(Texture frameTexture, System.Single x, System.Single y, System.Single width)
    {
        return new NineSlicePanel(frameTexture, _border)
            .SetPosition(new Vector2f(x, y))
            .SetSize(new Vector2f(width, InitialPanelHeight));
    }

    /// <summary>
    /// Calculates inner text width based on panel width and padding.
    /// </summary>
    /// <param name="panelWidth">Panel width.</param>
    /// <returns>Usable width for text rendering.</returns>
    private System.Single COMPUTE_INNER_WIDTH(System.Single panelWidth)
        => System.MathF.Max(MinInnerWidth, panelWidth - (2f * _horizontalPadding));

    /// <summary>
    /// Computes target panel height based on text height and vertical padding.
    /// </summary>
    /// <param name="textHeight">Measured text height.</param>
    /// <returns>Panel height.</returns>
    private System.Single COMPUTE_TARGET_HEIGHT(System.Single textHeight)
    {
        System.Single height = _verticalPadding + textHeight + _verticalPadding;
        return System.MathF.Max(MinPanelHeight, height);
    }

    /// <summary>
    /// Positions the message text centered within the inner panel bounds and computes anchor position.
    /// </summary>
    /// <param name="panel">Panel object.</param>
    /// <param name="textHeight">Measured text height.</param>
    /// <param name="anchorOut">Returns computed anchor position.</param>
    private void POSITION_TEXT_INSIDE_PANEL(NineSlicePanel panel, System.Single textHeight, out Vector2f anchorOut)
    {
        System.Single innerLeft = panel.Position.X + _border.Left + _horizontalPadding;
        System.Single innerRight = panel.Position.X + panel.Size.X - _border.Right - _horizontalPadding;
        System.Single innerCenterX = (innerLeft + innerRight) / 2f;
        System.Single innerTop = panel.Position.Y + _border.Top + _verticalPadding;

        _messageText.Position = new Vector2f(innerCenterX, innerTop + (textHeight * 0.5f));
        anchorOut = _messageText.Position;
    }

    #endregion Private Methods - Layout

    #region Private Methods - Text Processing

    /// <summary>
    /// Prepares SFML Text object with word-wrapped message.
    /// </summary>
    /// <param name="font">Font object.</param>
    /// <param name="message">String to display.</param>
    /// <param name="charSize">Font size.</param>
    /// <param name="innerWidth">Max width for wrapping.</param>
    /// <returns>Configured Text object.</returns>
    private static Text PREPARE_WRAPPED_TEXT(Font font, System.String message, System.UInt32 charSize, System.Single innerWidth)
        => new(font, WRAP_TEXT(font, message, charSize, innerWidth), charSize) { FillColor = Color.Black };

    /// <summary>
    /// Re-centers origin of Text and returns measured height.
    /// </summary>
    /// <param name="text">Text object.</param>
    /// <returns>Measured height of text block.</returns>
    private static System.Single CENTER_TEXT_ORIGIN_AND_MEASURE(Text text)
    {
        FloatRect localBounds = text.GetLocalBounds();
        text.Origin = new Vector2f(
            localBounds.Left + (localBounds.Width / 2f),
            localBounds.Top + (localBounds.Height / 2f));
        return text.GetGlobalBounds().Height;
    }

    /// <summary>
    /// Updates the message text, maintaining the anchor position and applying word wrap.
    /// </summary>
    /// <param name="newMessage">New message to display.</param>
    private void UPDATE_MESSAGE_INTERNAL(System.String newMessage)
    {
        System.Single innerWidth = this.COMPUTE_INNER_WIDTH(_panel.Size.X);
        _messageText.DisplayedString = WRAP_TEXT(_messageText.Font, newMessage, _messageText.CharacterSize, innerWidth);

        // Re-center origin and recalculate layout
        System.Single textHeight = CENTER_TEXT_ORIGIN_AND_MEASURE(_messageText);
        System.Single panelHeight = this.COMPUTE_TARGET_HEIGHT(textHeight);

        _panel.SetSize(new Vector2f(_panel.Size.X, panelHeight));
        this.POSITION_TEXT_INSIDE_PANEL(_panel, textHeight, out _textAnchor);
    }

    /// <summary>
    /// Performs word wrapping on the specified text, splitting into multiple lines so each fits in <paramref name="maxWidth"/>.
    /// Uses a single <see cref="Text"/> instance for measuring, avoiding performance overhead.
    /// </summary>
    /// <param name="font">Font to use for measurement.</param>
    /// <param name="text">Text content to wrap.</param>
    /// <param name="characterSize">Font character size.</param>
    /// <param name="maxWidth">Maximum allowed line width.</param>
    /// <returns>Word-wrapped text.</returns>
    private static System.String WRAP_TEXT(Font font, System.String text, System.UInt32 characterSize, System.Single maxWidth)
    {
        if (System.String.IsNullOrEmpty(text))
        {
            return System.String.Empty;
        }

        System.Text.StringBuilder result = new();
        System.String currentLine = System.String.Empty;
        System.String[] words = text.Split(' ');

        Text measurer = new(font, System.String.Empty, characterSize);

        foreach (System.String word in words)
        {
            System.String testLine = System.String.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";
            measurer.DisplayedString = testLine;

            if (measurer.GetLocalBounds().Width > maxWidth)
            {
                if (!System.String.IsNullOrEmpty(currentLine))
                {
                    _ = result.AppendLine(currentLine);
                    currentLine = word;
                }
                else
                {
                    // Word longer than maxWidth: force wrapping on this word
                    _ = result.AppendLine(word);
                    currentLine = System.String.Empty;
                }
            }
            else
            {
                currentLine = testLine;
            }
        }

        if (!System.String.IsNullOrEmpty(currentLine))
        {
            _ = result.Append(currentLine);
        }

        return result.ToString();
    }

    #endregion Private Methods - Text Processing
}
