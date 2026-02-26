// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Graphics.Assets;
using Nalix.Graphics.Enums;
using Nalix.Graphics.Extensions;
using Nalix.Graphics.UI.Controls;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Graphics.UI.Dialogs;

/// <summary>
/// Notification box with a single action button using reusable Button class.
/// Supports click visual effects and automatically closes when the button is clicked.
/// </summary>
public sealed class MessageBoxAction : MessageBox
{
    #region Constants

    private const System.Int32 ButtonZIndexOffset = 1;
    private const System.Single DefaultVerticalGap = 12f;
    private const System.Single DefaultButtonWidth = 180f;
    private const System.Single DefaultButtonHeight = 32f;
    private const System.UInt32 DefaultButtonFontSize = 18;
    private const System.Single DefaultButtonExtraOffset = -90f;

    #endregion Constants

    #region Fields

    private readonly Button _actionButton;

    private System.Single _verticalGap;
    private System.Single _buttonExtraOffsetY;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets or sets the message text. Automatically repositions the button when changed.
    /// </summary>
    public new System.String Message
    {
        get => base.Message;
        set
        {
            if (base.Message != value)
            {
                base.Message = value;
                this.UPDATE_BUTTON_LAYOUT();
            }
        }
    }

    /// <summary>
    /// Gets or sets the extra vertical offset for the button after layout.
    /// </summary>
    public System.Single ButtonExtraOffsetY
    {
        get => _buttonExtraOffsetY;
        set
        {
            if (_buttonExtraOffsetY != value)
            {
                _buttonExtraOffsetY = value;
                this.UPDATE_BUTTON_LAYOUT();
            }
        }
    }

    /// <summary>
    /// Gets or sets the vertical gap between message text and the button.
    /// </summary>
    public System.Single VerticalGap
    {
        get => _verticalGap;
        set
        {
            System.Single newValue = System.MathF.Max(0f, value);
            if (_verticalGap != newValue)
            {
                _verticalGap = newValue;
                this.UPDATE_BUTTON_LAYOUT();
            }
        }
    }

    /// <summary>
    /// Gets or sets the button text.
    /// </summary>
    public System.String ButtonText
    {
        get => _actionButton.Text;
        set => _actionButton.Text = value;
    }

    /// <summary>
    /// Gets or sets the button width. Automatically repositions when changed.
    /// </summary>
    public System.Single ButtonWidth
    {
        get => _actionButton.Width;
        set
        {
            if (_actionButton.Width != value)
            {
                _actionButton.Width = value;
                this.UPDATE_BUTTON_LAYOUT();
            }
        }
    }

    /// <summary>
    /// Gets or sets the button height.
    /// </summary>
    public System.Single ButtonHeight
    {
        get => _actionButton.Height;
        set => _actionButton.Height = value;
    }

    /// <summary>
    /// Gets or sets the button font size.
    /// </summary>
    public System.UInt32 ButtonFontSize
    {
        get => _actionButton.FontSize;
        set => _actionButton.FontSize = value;
    }

    /// <summary>
    /// Gets or sets whether the button is enabled.
    /// </summary>
    public System.Boolean ButtonEnabled
    {
        get => _actionButton.IsEnabled;
        set => _actionButton.IsEnabled = value;
    }

    #endregion Properties

    #region Events

    /// <summary>
    /// Raised when the action button is clicked.
    /// </summary>
    public event System.Action ButtonClicked;

    #endregion Events

    #region Constructor

    /// <summary>
    /// Initializes a notification box with an action button under the message.
    /// </summary>
    /// <param name="initialMessage">Initial notification message.</param>
    /// <param name="buttonTexture">Texture for button panel.</param>
    /// <param name="side">Side of the screen to display notification.</param>
    /// <param name="buttonText">Label of action button.</param>
    /// <param name="font">Font for notification and button text.</param>
    public MessageBoxAction(
        System.String initialMessage = "",
        Texture buttonTexture = null,
        MessageBoxPlacement side = MessageBoxPlacement.Bottom,
        System.String buttonText = "OK",
        Font font = null)
        : base(initialMessage, buttonTexture, side, font)
    {
        font ??= EmbeddedAssets.JetBrainsMono.ToFont();
        buttonTexture ??= EmbeddedAssets.SquareOutline.ToTexture();

        _buttonExtraOffsetY = DefaultButtonExtraOffset;
        _verticalGap = DefaultVerticalGap;

        _actionButton = new Button(buttonText, buttonTexture, DefaultButtonWidth, default, font)
        {
            FontSize = DefaultButtonFontSize,
            Size = new Vector2f(DefaultButtonWidth, DefaultButtonHeight)
        };

        _actionButton.RegisterClickHandler(this.ON_BUTTON_PRESSED);

        this.UPDATE_BUTTON_LAYOUT();
        this.SYNC_BUTTON_Z_INDEX();
    }

    #endregion Constructor

    #region Overrides

    /// <summary>
    /// Updates the notification and its button.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update.</param>
    public override void Update(System.Single deltaTime)
    {
        if (!this.IsVisible)
        {
            return;
        }

        base.Update(deltaTime);
        _actionButton.Update(deltaTime);
    }

    /// <summary>
    /// Renders the notification base and action button.
    /// </summary>
    /// <param name="target">Render target.</param>
    public override void Draw(IRenderTarget target)
    {
        if (!this.IsVisible)
        {
            return;
        }

        base.Draw(target);
        _actionButton.Draw(target);
    }

    #endregion Overrides

    #region Private Methods

    /// <summary>
    /// Lays out the action button underneath the notification message.
    /// </summary>
    private void UPDATE_BUTTON_LAYOUT()
    {
        System.Single panelCenterX = this.Position.X + (this.Size.X / 2f);
        System.Single buttonX = panelCenterX - (_actionButton.Width / 2f);
        System.Single buttonY = this.Position.Y + this.Size.Y + _verticalGap + _buttonExtraOffsetY;

        _actionButton.Position = new Vector2f(buttonX, buttonY);
    }

    /// <summary>
    /// Synchronizes the button's Z-index with the message box's Z-index.
    /// </summary>
    private void SYNC_BUTTON_Z_INDEX() => _actionButton.SetZIndex(this.ZIndex + ButtonZIndexOffset);

    /// <summary>
    /// Handles button pressed event: fires external callback and hides this notification.
    /// </summary>
    private void ON_BUTTON_PRESSED()
    {
        this.ButtonClicked?.Invoke();
        base.Hide();
    }

    #endregion Private Methods
}
