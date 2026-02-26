// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Graphics.Engine;
using Nalix.Graphics.Entities;
using Nalix.Graphics.UI.Controls;
using SFML.Graphics;
using SFML.System;

namespace AutoX.Gara.Launcher.Scenes.MainMenuView;

/// <summary>
/// Represents a view containing the main menu buttons for the application.
/// Manages the layout, rendering, and interaction of login, new game, server info, and change account buttons.
/// </summary>
public class ButtonView : RenderObject
{
    #region Const

    private const System.Single ButtonWidth = 380f;
    private const System.Single VerticalSpacing = 20f;
    private const System.Single HorizontalCenterDivisor = 2f;
    private const System.Single VerticalCenterDivisor = 1.65f;

    #endregion Const

    #region Fields

    private readonly Button _exit;
    private readonly Button _login;
    private readonly Button _register;

    private readonly Button[] _buttons;

    #endregion Fields

    #region Events

    public event System.Action ExitRequested;
    public event System.Action LoginRequested;
    public event System.Action RegisterRequested;

    #endregion Events

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ButtonView"/> class.
    /// Creates and configures all buttons, wires event handlers, and sets up the layout.
    /// </summary>
    public ButtonView()
    {
        _exit = new Button("EXIT");
        _login = new Button("LOGIN");
        _register = new Button("REGISTER");

        _buttons = [_login, _register, _exit];

        this.WIRE_HANDLERS();
        this.REGISTER_BUTTONS();
        this.LAYOUT_BUTTONS();
    }

    #endregion Constructor

    #region Overrides

    /// <inheritdoc/>
    public override void Update(System.Single dt)
    {
        if (!base.IsVisible)
        {
            return;
        }

        foreach (Button b in _buttons)
        {
            b.Update(dt);
        }
    }

    /// <inheritdoc/>
    public override void Draw(IRenderTarget target)
    {
        foreach (Button b in _buttons)
        {
            b.Draw(target);
        }
    }

    /// <inheritdoc/>
    public override void OnBeforeDestroy()
    {
        // Clear all event subscribers
        this.ExitRequested = null;
        this.LoginRequested = null;
        this.RegisterRequested = null;

        base.OnBeforeDestroy();
    }

    /// <inheritdoc/>
    protected override IDrawable GetDrawable() => throw new System.NotSupportedException();

    #endregion Overrides

    #region Private Methods

    private void REGISTER_BUTTONS()
    {
        foreach (Button b in _buttons)
        {
            b.Size = new Vector2f(ButtonWidth, b.Size.Y);
            b.FontSize = 17;
        }
    }

    private void WIRE_HANDLERS()
    {
        _exit.RegisterClickHandler(() => ExitRequested?.Invoke());
        _login.RegisterClickHandler(() => LoginRequested?.Invoke());
        _register.RegisterClickHandler(() => RegisterRequested?.Invoke());
    }

    private void LAYOUT_BUTTONS()
    {
        System.Single total = 0f;
        foreach (Button b in _buttons)
        {
            if (!b.IsVisible)
            {
                continue;
            }
            total += b.GlobalBounds.Height + VerticalSpacing;
        }

        total -= VerticalSpacing;

        System.Single y = (GraphicsEngine.ScreenSize.Y - total) / VerticalCenterDivisor;

        foreach (Button b in _buttons)
        {
            if (!b.IsVisible)
            {
                continue;
            }
            FloatRect r = b.GlobalBounds;
            System.Single x = (GraphicsEngine.ScreenSize.X - r.Width) / HorizontalCenterDivisor;

            b.Position = new Vector2f(x, y);
            y += r.Height + VerticalSpacing;
        }
    }

    #endregion Private Methods
}