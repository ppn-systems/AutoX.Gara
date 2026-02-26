// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Framework.Injection.DI;
using Nalix.Graphics.Abstractions;

namespace Nalix.Graphics.Internal;

/// <summary>
/// Manages the focus state of <see cref="IFocusable"/> objects within the UI.
/// </summary>
internal sealed class FocusManager : SingletonBase<FocusManager>
{
    #region Fields

    private IFocusable _focused;

    #endregion Fields

    #region Constructors

    public FocusManager() => _focused = null;

    #endregion Constructors

    #region APIs

    /// <summary>
    /// Requests focus for the specified <see cref="IFocusable"/> target.
    /// If the target is already focused, no action is taken.
    /// When focus changes, the previously focused object receives <see cref="IFocusable.OnFocusLost"/>,
    /// and the new target receives <see cref="IFocusable.OnFocusGained"/>.
    /// </summary>
    /// <param name="target">The object to gain focus.</param>
    public void RequestFocus(IFocusable target)
    {
        if (ReferenceEquals(_focused, target))
        {
            return;
        }

        _focused?.OnFocusLost();
        _focused = target;
        _focused?.OnFocusGained();
    }

    /// <summary>
    /// Clears focus from the specified <see cref="IFocusable"/> target if it is currently focused.
    /// The target receives <see cref="IFocusable.OnFocusLost"/>, and no object will be focused afterwards.
    /// </summary>
    /// <param name="target">The object to lose focus.</param>
    public void ClearFocus(IFocusable target)
    {
        if (ReferenceEquals(_focused, target))
        {
            _focused?.OnFocusLost();
            _focused = null;
        }
    }

    #endregion APIs
}
