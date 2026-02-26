// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Graphics.Abstractions;
using Nalix.Graphics.Entities;
using Nalix.Graphics.Enums;
using Nalix.Graphics.Internal.Effects;
using SFML.Graphics;

namespace Nalix.Graphics.Scenes;

/// <summary>
/// Runs a two-phase scene transition via a full-screen overlay: <b>closing</b> (cover) → <b>switch scene</b> → <b>opening</b> (reveal).
/// </summary>
/// <remarks>
/// This instance exists across scenes and automatically destroys itself when complete.
/// The desired effect is rendered using the selected <see cref="ITransitionDrawable"/> strategy.
/// </remarks>
public sealed class SceneTransition : RenderObject, IUpdatable
{
    #region Fields

    private readonly ITransitionDrawable _effect;
    private readonly System.String _nextSceneName;
    private readonly System.Single _durationSeconds;

    private System.Single _elapsed;
    private System.Boolean _hasSwitched;

    #endregion Fields

    #region Construction

    /// <summary>
    /// Initializes a new instance of the <see cref="SceneTransition"/> class.
    /// </summary>
    /// <param name="nextScene">The target scene name to switch to at transition midpoint.</param>
    /// <param name="style">Overlay transition visual style.</param>
    /// <param name="duration">Total transition duration in seconds (minimum 0.1s).</param>
    /// <param name="color">Overlay color (default: black).</param>
    /// <exception cref="System.ArgumentNullException"><paramref name="nextScene"/> is null.</exception>
    /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="duration"/> is not positive.</exception>
    public SceneTransition(System.String nextScene, SceneTransitionEffect style = SceneTransitionEffect.Fade, System.Single duration = 1.0f, Color? color = null)
    {
        _nextSceneName = nextScene ?? throw new System.ArgumentNullException(nameof(nextScene));
        _durationSeconds = System.MathF.Max(0.1f, duration);
        _effect = style switch
        {
            SceneTransitionEffect.Fade => new FadeOverlay(color ?? Color.Black),
            SceneTransitionEffect.WipeVertical => new WipeOverlayVertical(color ?? Color.Black),
            SceneTransitionEffect.ZoomIn => new ZoomOverlay(color ?? Color.Black, modeIn: true),
            SceneTransitionEffect.ZoomOut => new ZoomOverlay(color ?? Color.Black, modeIn: false),
            SceneTransitionEffect.WipeHorizontal => new WipeOverlayHorizontal(color ?? Color.Black),
            SceneTransitionEffect.SlideCoverLeft => new SlideCoverOverlay(color ?? Color.Black, fromLeft: true),
            SceneTransitionEffect.SlideCoverRight => new SlideCoverOverlay(color ?? Color.Black, fromLeft: false),
            _ => new FadeOverlay(color ?? Color.Black)
        };

        // Always render on top, persistent through scene change
        this.IsPersistent = true;
        base.SetZIndex(System.Int32.MaxValue);
    }

    #endregion

    #region Virtual Methods

    /// <summary>
    /// Advances the transition state, switches the scene at the midpoint, and destroys itself on completion.
    /// </summary>
    /// <param name="deltaTime">Elapsed time, in seconds, since the last frame.</param>
    public override void Update(System.Single deltaTime)
    {
        _elapsed += deltaTime;

        System.Single half = _durationSeconds * 0.5f;
        System.Boolean isClosing = _elapsed <= half;

        System.Single localT = isClosing
            ? (_elapsed / half)
            : ((_elapsed - half) / half);

        localT = System.Math.Clamp(localT, 0f, 1f);

        _effect.Update(localT, isClosing);

        if (!_hasSwitched && _elapsed >= half)
        {
            _hasSwitched = true;
            SceneManager.Instance.ScheduleSceneChange(_nextSceneName);
        }

        if (_elapsed >= _durationSeconds)
        {
            base.Destroy();
        }
    }

    /// <summary>
    /// Gets the current overlay drawable for rendering.
    /// </summary>
    /// <returns>
    /// The <see cref="IDrawable"/> containing the visual effect.
    /// </returns>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override IDrawable GetDrawable() => _effect.GetDrawable();

    #endregion Virtual Methods
}
