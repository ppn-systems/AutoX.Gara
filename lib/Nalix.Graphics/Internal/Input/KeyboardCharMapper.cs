// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Framework.Injection.DI;
using Nalix.Graphics.Input;
using SFML.Window;

namespace Nalix.Graphics.Internal.Input;

/// <inheritdoc/>
public class KeyboardCharMapper : SingletonBase<KeyboardCharMapper>
{
    #region Fields

    private readonly System.Collections.Generic.Dictionary<Keyboard.Key, (System.Char normal, System.Char shift)> _map = new()
    {
        // row digits
        [Keyboard.Key.Num0] = ('0', ')'),
        [Keyboard.Key.Num1] = ('1', '!'),
        [Keyboard.Key.Num2] = ('2', '@'),
        [Keyboard.Key.Num3] = ('3', '#'),
        [Keyboard.Key.Num4] = ('4', '$'),
        [Keyboard.Key.Num5] = ('5', '%'),
        [Keyboard.Key.Num6] = ('6', '^'),
        [Keyboard.Key.Num7] = ('7', '&'),
        [Keyboard.Key.Num8] = ('8', '*'),
        [Keyboard.Key.Num9] = ('9', '('),

        // punctuation
        [Keyboard.Key.Hyphen] = ('-', '_'),
        [Keyboard.Key.Equal] = ('=', '+'),
        [Keyboard.Key.LBracket] = ('[', '{'),
        [Keyboard.Key.RBracket] = (']', '}'),
        [Keyboard.Key.Backslash] = ('\\', '|'),
        [Keyboard.Key.Semicolon] = (';', ':'),
        [Keyboard.Key.Apostrophe] = ('\'', '"'),
        [Keyboard.Key.Comma] = (',', '<'),
        [Keyboard.Key.Period] = ('.', '>'),
        [Keyboard.Key.Slash] = ('/', '?'),
        [Keyboard.Key.Space] = (' ', ' ')
    };

    #endregion Fields

    #region APIs

    /// <inheritdoc/>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public System.Boolean TryMapKeyToChar(out System.Char c, System.Boolean shift)
    {
        c = '\0';

        // A..Z
        for (Keyboard.Key k = Keyboard.Key.A; k <= Keyboard.Key.Z; k++)
        {
            if (KeyboardManager.Instance.IsKeyPressed(k))
            {
                c = (System.Char)((shift ? 'A' : 'a') + (k - Keyboard.Key.A));
                return true;
            }
        }

        // numpad 0..9
        for (Keyboard.Key k = Keyboard.Key.Numpad0; k <= Keyboard.Key.Numpad9; k++)
        {
            if (KeyboardManager.Instance.IsKeyPressed(k))
            {
                c = (System.Char)('0' + (k - Keyboard.Key.Numpad0));
                return true;
            }
        }

        // row digits + punctuation
        foreach (var kv in _map)
        {
            if (KeyboardManager.Instance.IsKeyPressed(kv.Key))
            {
                c = shift ? kv.Value.shift : kv.Value.normal;
                return true;
            }
        }

        return false;
    }

    #endregion APIs
}
