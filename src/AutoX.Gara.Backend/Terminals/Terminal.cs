// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Framework.Injection.DI;

namespace AutoX.Gara.Backend.Terminals;

/// <summary>
/// Powerful console tab manager supporting color highlight, arrow navigation, and status bar.
/// </summary>
public sealed class Terminal : SingletonBase<Terminal>
{
    #region Fields

    private volatile System.Boolean _running;
    private readonly System.Threading.ManualResetEvent _quitEvent = new(false);
    private readonly System.Collections.Generic.List<TabDescriptor> _tabs = [];

    private System.Int32 _currentTabIndex;
    private System.Int32 _consoleWidth = System.Console.WindowWidth;
    private System.Int32 _consoleHeight = System.Console.WindowHeight;

    #endregion Fields

    #region APIs

    /// <summary>
    /// Registers a new tab with the terminal. The tab will be displayed in the tab bar and can be switched to using its hotkey or arrow keys.
    /// </summary>
    public void Register(TabDescriptor tab)
    {
        System.ArgumentNullException.ThrowIfNull(tab);

        _tabs.Add(tab);
        if (_tabs.Count == 1)
        {
            _currentTabIndex = 0;
        }
    }

    /// <summary>
    /// Runs the terminal, starting the input loop and rendering the UI until Exit is called.
    /// </summary>
    public void Run(System.String quitMessage = null)
    {
        _running = true;
        System.Console.CursorVisible = false;
        System.Console.Clear();
        var inputThread = new System.Threading.Thread(INPUT_LOOP) { IsBackground = true };
        inputThread.Start();

        while (_running)
        {
            if (_consoleWidth != System.Console.WindowWidth || _consoleHeight != System.Console.WindowHeight)
            {
                _consoleWidth = System.Console.WindowWidth;
                _consoleHeight = System.Console.WindowHeight;
                System.Console.Clear();
            }
            RENDER_UI();
            System.Threading.Thread.Sleep(80);
        }
        inputThread.Join();

        System.Console.Clear();
        if (!System.String.IsNullOrWhiteSpace(quitMessage))
        {
            System.Console.WriteLine(quitMessage);
        }
    }

    /// <summary>
    /// Exits the terminal, stopping the input loop and allowing the Run method to return.
    /// </summary>
    public void Exit() => Dispose();

    /// <inheritdoc/>
    public new void Dispose()
    {
        _running = false;
        _quitEvent.Set();
    }

    #endregion APIs

    #region Private Methods

    private void RENDER_UI()
    {
        System.Console.SetCursorPosition(0, 0);
        System.Console.ForegroundColor = System.ConsoleColor.Cyan;
        System.Console.WriteLine(new System.String('═', _consoleWidth));

        System.Console.ResetColor();
        System.Console.Write("Tabs: ");
        for (System.Int32 i = 0; i < _tabs.Count; i++)
        {
            if (i == _currentTabIndex)
            {
                System.Console.BackgroundColor = System.ConsoleColor.DarkBlue;
                System.Console.ForegroundColor = System.ConsoleColor.Yellow;
            }
            else
            {
                System.Console.ResetColor();
            }
            System.Console.Write($" [Ctrl+{KEY_LABEL(_tabs[i].Hotkey)}:{_tabs[i].DisplayName}] ");
            System.Console.ResetColor();
        }
        System.Console.WriteLine();
        System.Console.WriteLine(new System.String('═', _consoleWidth));

        System.Console.ResetColor();
        if (_tabs.Count > 0)
        {
            var tab = _tabs[_currentTabIndex];
            var lines = tab.ContentProvider?.Invoke() ?? System.Array.Empty<System.String>();
            System.Int32 availableRows = _consoleHeight - 6;
            for (System.Int32 i = 0; i < System.Math.Min(lines.Count, availableRows); i++)
            {
                System.Console.WriteLine(lines[i]);
            }
        }

        System.Console.SetCursorPosition(0, _consoleHeight - 2);
        System.Console.BackgroundColor = System.ConsoleColor.Gray;
        System.Console.ForegroundColor = System.ConsoleColor.Black;
        const System.String shortcuts = "Shortcuts: Ctrl+[tab hotkey]/←/→: Switch | Ctrl+C: Exit";
        System.Console.Write(shortcuts.PadRight(_consoleWidth - 1));
        System.Console.ResetColor();
    }

    private void INPUT_LOOP()
    {
        System.Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            Dispose();
        };
        while (_running)
        {
            if (System.Console.KeyAvailable)
            {
                var keyInfo = System.Console.ReadKey(true);
                System.Boolean handled = false;

                if (keyInfo.Modifiers.HasFlag(System.ConsoleModifiers.Control))
                {
                    for (System.Int32 i = 0; i < _tabs.Count; i++)
                    {
                        if (keyInfo.Key == _tabs[i].Hotkey) { _currentTabIndex = i; handled = true; break; }
                    }
                }
                else if (keyInfo.Key == System.ConsoleKey.LeftArrow)
                {
                    _currentTabIndex = (_currentTabIndex - 1 + _tabs.Count) % _tabs.Count; handled = true;
                }
                else if (keyInfo.Key == System.ConsoleKey.RightArrow)
                {
                    _currentTabIndex = (_currentTabIndex + 1) % _tabs.Count; handled = true;
                }

                if (handled && System.OperatingSystem.IsWindows())
                {
                    System.Console.Beep(900, 40);
                }
            }
            System.Threading.Thread.Sleep(12);
        }
    }

    private static System.String KEY_LABEL(System.ConsoleKey key)
        => key is >= System.ConsoleKey.D0 and <= System.ConsoleKey.D9 ? ((System.Int32)key - (System.Int32)System.ConsoleKey.D0).ToString() : key.ToString();

    #endregion Private Methods
}