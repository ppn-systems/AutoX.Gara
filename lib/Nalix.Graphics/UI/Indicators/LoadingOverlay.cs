// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Graphics.Abstractions;
using Nalix.Graphics.Engine;
using Nalix.Graphics.Entities;
using Nalix.Graphics.Enums;
using Nalix.Graphics.Extensions;
using Nalix.Graphics.UI.Theme;
using SFML.Graphics;
using SFML.System;

namespace Nalix.Graphics.UI.Indicators;

/// <summary>
/// Very simple loading overlay, draws a dimmed background.
/// Spinner is handled as a separate object.
/// </summary>
public sealed class LoadingOverlay : RenderObject
{
    #region Constants

    private const System.Byte DefaultOverlayAlpha = 160;

    #endregion Constants

    #region Fields

    private readonly RectangleShape _overlayRect;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets or sets the overlay background color.
    /// </summary>
    public Color OverlayColor
    {
        get => _overlayRect.FillColor;
        set
        {
            System.Byte currentAlpha = _overlayRect.FillColor.A;
            _overlayRect.FillColor = new Color(value.R, value.G, value.B, currentAlpha);
        }
    }

    /// <summary>
    /// Gets or sets the overlay alpha (transparency) value.
    /// </summary>
    public System.Byte OverlayAlpha
    {
        get => _overlayRect.FillColor.A;
        set
        {
            Color current = _overlayRect.FillColor;
            _overlayRect.FillColor = new Color(current.R, current.G, current.B, value);
        }
    }

    /// <summary>
    /// Gets the spinner instance for direct access.
    /// </summary>
    public Spinner SpinnerInstance { get; }

    /// <summary>
    /// Gets or sets the spinner rotation speed in degrees per second.
    /// </summary>
    public System.Single SpinnerRotationSpeed
    {
        get => this.SpinnerInstance.RotationSpeed;
        set => this.SpinnerInstance.RotationSpeed = value;
    }

    /// <summary>
    /// Gets or sets the spinner center position.
    /// </summary>
    public Vector2f SpinnerCenter
    {
        get => this.SpinnerInstance.Center;
        set => this.SpinnerInstance.Center = value;
    }

    /// <summary>
    /// Gets or sets the spinner alpha (opacity).
    /// </summary>
    public System.Byte SpinnerAlpha
    {
        get => this.SpinnerInstance.Alpha;
        set => this.SpinnerInstance.Alpha = value;
    }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadingOverlay"/> class.
    /// </summary>
    public LoadingOverlay()
    {
        _overlayRect = new RectangleShape(new Vector2f(GraphicsEngine.ScreenSize.X, GraphicsEngine.ScreenSize.Y))
        {
            FillColor = new Color(0, 0, 0, DefaultOverlayAlpha),
            Position = default
        };

        base.SetZIndex(RenderLayer.Overlay.ToZIndex());

        this.SpinnerInstance = new Spinner(new Vector2f(GraphicsEngine.ScreenSize.X / 2f, GraphicsEngine.ScreenSize.Y / 2f))
        {
            RotationSpeed = 180f
        };
        this.SpinnerInstance.SetZIndex(System.Int32.MaxValue - 1);
    }

    #endregion Constructor

    #region Overrides

    /// <inheritdoc/>
    public override void Update(System.Single deltaTime)
    {
        // Auto-resize overlay if window size changes
        if (_overlayRect.Size.X != GraphicsEngine.ScreenSize.X || _overlayRect.Size.Y != GraphicsEngine.ScreenSize.Y)
        {
            _overlayRect.Size = (Vector2f)GraphicsEngine.ScreenSize;
        }

        this.SpinnerInstance.Update(deltaTime);
    }

    /// <inheritdoc/>
    public override void Draw(IRenderTarget target)
    {
        if (!this.IsVisible)
        {
            return;
        }

        target.Draw(_overlayRect);
        this.SpinnerInstance.Draw(target);
    }

    /// <inheritdoc/>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override IDrawable GetDrawable() =>
        throw new System.NotSupportedException("Use Draw() instead.");

    #endregion Overrides

    #region Nested Class - Spinner

    /// <summary>
    /// Procedural animated spinner used as a loading indicator.
    /// Can be shown independently or embedded as part of composite UI.
    /// </summary>
    /// <remarks>
    /// This spinner is designed for efficient rendering by precomputing segment shapes and alpha multipliers
    /// to avoid unnecessary allocations during each frame.
    /// </remarks>
    public sealed class Spinner : RenderObject, IUpdatable
    {
        #region Constants

        private const System.Int32 SegmentCount = 12;
        private const System.Single DefaultSpinnerRadius = 32f;
        private const System.Single DefaultSegmentThickness = 7f;
        private const System.Single DegreesToRadians = 0.017453292519943295f;
        private const System.Single DefaultRotationSpeed = 150f;
        private const System.Single FullRotation = 360f;
        private const System.Single MinAlphaMultiplier = 0.2f;
        private const System.Single MaxAlphaMultiplier = 0.8f;
        private const System.Byte MaxAlpha = 255;

        #endregion Constants

        #region Fields

        private Vector2f _center;
        private System.Single _currentAngle = 0f;
        private System.Single _rotationDegreesPerSecond;
        private System.Single _spinnerRadius;
        private System.Single _segmentThickness;
        private Color _spinnerColor;

        private readonly CircleShape[] _segmentShapes = new CircleShape[SegmentCount];
        private readonly System.Single[] _segmentOffsets = new System.Single[SegmentCount];
        private readonly System.Byte[] _segmentAlphaMultipliers = new System.Byte[SegmentCount];

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets the center position of the spinner.
        /// </summary>
        public Vector2f Center
        {
            get => _center;
            set => _center = value;
        }

        /// <summary>
        /// Gets or sets the alpha (opacity) for the entire spinner (0-255).
        /// </summary>
        public System.Byte Alpha { get; set; } = MaxAlpha;

        /// <summary>
        /// Gets or sets the spinner's rotation speed in degrees per second.
        /// </summary>
        public System.Single RotationSpeed
        {
            get => _rotationDegreesPerSecond;
            set => _rotationDegreesPerSecond = value;
        }

        /// <summary>
        /// Gets or sets the spinner radius in pixels.
        /// </summary>
        public System.Single Radius
        {
            get => _spinnerRadius;
            set
            {
                if (_spinnerRadius != value)
                {
                    _spinnerRadius = System.MathF.Max(1f, value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the segment thickness in pixels.
        /// </summary>
        public System.Single SegmentThickness
        {
            get => _segmentThickness;
            set
            {
                System.Single newValue = System.MathF.Max(1f, value);
                if (_segmentThickness != newValue)
                {
                    _segmentThickness = newValue;
                    this.UPDATE_SEGMENT_THICKNESS();
                }
            }
        }

        /// <summary>
        /// Gets or sets the spinner foreground color.
        /// </summary>
        public Color SpinnerColor
        {
            get => _spinnerColor;
            set => _spinnerColor = value;
        }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Spinner"/> class at a specific center point.
        /// </summary>
        /// <param name="center">The center point for the spinner.</param>
        public Spinner(Vector2f center)
        {
            _center = center;
            _rotationDegreesPerSecond = DefaultRotationSpeed;
            _spinnerRadius = DefaultSpinnerRadius;
            _segmentThickness = DefaultSegmentThickness;
            _spinnerColor = Themes.SpinnerForegroundColor;

            this.PRECOMPUTE_SEGMENTS();
            base.SetZIndex(RenderLayer.Spinner.ToZIndex());
        }

        #endregion Constructor

        #region Overrides

        /// <inheritdoc />
        public override void Update(System.Single deltaTime)
        {
            _currentAngle += deltaTime * _rotationDegreesPerSecond;
            if (_currentAngle >= FullRotation)
            {
                _currentAngle -= FullRotation;
            }
        }

        /// <inheritdoc />
        public override void Draw(IRenderTarget target)
        {
            if (!this.IsVisible)
            {
                return;
            }

            for (System.Int32 i = 0; i < SegmentCount; i++)
            {
                System.Single segAngle = _currentAngle + _segmentOffsets[i];
                System.Single angleRad = segAngle * DegreesToRadians;

                System.Single x = _center.X + (System.MathF.Cos(angleRad) * _spinnerRadius);
                System.Single y = _center.Y + (System.MathF.Sin(angleRad) * _spinnerRadius);

                CircleShape segCircle = _segmentShapes[i];
                segCircle.Position = new Vector2f(x, y);

                System.Byte finalAlpha = (System.Byte)(this.Alpha * _segmentAlphaMultipliers[i] / MaxAlpha);
                segCircle.FillColor = new Color(_spinnerColor.R, _spinnerColor.G, _spinnerColor.B, finalAlpha);

                target.Draw(segCircle);
            }
        }

        /// <inheritdoc />
        [return: System.Diagnostics.CodeAnalysis.NotNull]
        protected override IDrawable GetDrawable() =>
            throw new System.NotSupportedException("Use Draw() instead.");

        #endregion Overrides

        #region Private Methods

        /// <summary>
        /// Precomputes static values for segment angle and multipliers to optimize drawing.
        /// </summary>
        private void PRECOMPUTE_SEGMENTS()
        {
            const System.Single anglePerSegment = FullRotation / SegmentCount;

            for (System.Int32 i = 0; i < SegmentCount; i++)
            {
                _segmentOffsets[i] = i * anglePerSegment;

                System.Single progress = (System.Single)i / SegmentCount;
                System.Single alphaMultiplier = MinAlphaMultiplier + (MaxAlphaMultiplier * progress);
                _segmentAlphaMultipliers[i] = (System.Byte)(alphaMultiplier * MaxAlpha);

                System.Single radius = _segmentThickness / 2f;
                _segmentShapes[i] = new CircleShape(radius)
                {
                    Origin = new Vector2f(radius, radius)
                };
            }
        }

        /// <summary>
        /// Updates segment shapes when thickness changes.
        /// </summary>
        private void UPDATE_SEGMENT_THICKNESS()
        {
            System.Single radius = _segmentThickness / 2f;
            for (System.Int32 i = 0; i < SegmentCount; i++)
            {
                _segmentShapes[i].Radius = radius;
                _segmentShapes[i].Origin = new Vector2f(radius, radius);
            }
        }

        #endregion Private Methods
    }

    #endregion Nested Class - Spinner
}
