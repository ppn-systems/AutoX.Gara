// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Common.Diagnostics;
using Nalix.Framework.Configuration;
using Nalix.Graphics.Engine;
using Nalix.Graphics.Input;
using Nalix.Graphics.Native;
using Nalix.Graphics.UI.Indicators;
using Nalix.Logging.Extensions;
using SFML.Graphics;

namespace Nalix.Graphics.Sandbox;

/// <summary>
/// Program entry point for Ascendance Sandbox application.
/// </summary>
public static class Program
{
    #region Public Methods

    /// <summary>
    /// Main entry point of the application.
    /// </summary>
    public static void Main()
    {
        // Setup logging
        NLogixFx.MinimumLevel = LogLevel.Debug;
        System.Threading.CancellationTokenSource cts = new();

        //Themes.ToggleTheme();
        ConfigurationManager.Instance.Get<GraphicsConfig>().ScreenWidth = 1280;
        ConfigurationManager.Instance.Get<GraphicsConfig>().ScreenHeight = 720;

        if (!GraphicsEngine.Instance.IsDebugMode && System.OperatingSystem.IsWindows())
        {
            Kernel32.Hide();
        }

        // Create and set application icon
        Image icon = GetImageFromBase64(Program.IconBase64);
        GraphicsEngine.Instance.FrameUpdate += Program.OnFrameUpdate;

        GraphicsEngine.Instance.SetIcon(icon);
        GraphicsEngine.Instance.Launch();

        cts.Cancel();

        System.Console.WriteLine("Press Enter to exit...");
        System.Console.ReadLine();
    }

    #endregion Public Methods

    #region Private Class

    private static class Debug
    {
        public static System.Boolean IsEnabled;

        public static readonly DebugOverlay Overlay;

        static Debug()
        {
            IsEnabled = false;
            Overlay = new DebugOverlay();
        }
    }

    #endregion Private Class

    #region Private Methods

    private static void OnFrameUpdate(System.Single deltaTime)
    {
        // Command line debug mode with F10
        if (KeyboardManager.Instance.IsKeyPressed(SFML.Window.Keyboard.Key.F10))
        {
            if (System.OperatingSystem.IsWindows())
            {
                if (Kernel32.IsConsoleVisible())
                {
                    Kernel32.Hide();
                }
                else
                {
                    Kernel32.Show();
                }
            }
        }

        // Toggle debug mode with F12
        if (KeyboardManager.Instance.IsKeyPressed(SFML.Window.Keyboard.Key.F12))
        {
            Debug.IsEnabled = !Debug.IsEnabled;

            if (!GraphicsEngine.Instance.IsDebugMode)
            {
                GraphicsEngine.Instance.DebugMode();
                GraphicsEngine.Instance.FrameRender -= OnFrameRender;
            }
            else
            {
                GraphicsEngine.Instance.DebugMode();
                GraphicsEngine.Instance.FrameRender += OnFrameRender;
            }
        }

        // Exit application with Escape
        if (KeyboardManager.Instance.IsKeyPressed(SFML.Window.Keyboard.Key.Escape))
        {
            GraphicsEngine.Instance.Shutdown();
            System.Environment.Exit(0);
        }
    }

    private static Image GetImageFromBase64(System.String base64)
    {
        System.Byte[] bytes = System.Convert.FromBase64String(base64);
        using System.IO.MemoryStream ms = new(bytes);
        return new Image(ms);
    }

    private static void OnFrameRender(IRenderTarget target) => Debug.Overlay.Draw(target);

    #endregion Private Methods

    #region Base64

    private const System.String IconBase64 =
        F9B4 + A3F9 + A28C +
        B7C2 + BAC5 + C91D +
        C5E7 + DFA4 + E8C1 +
        A3A1 + F4D7 + B74B +
        A920 + C5F1 + B3B6 +
        D0C9 + C1E8 + E8B2 +
        D47A + FD41 + E6F2 + A6A7;

    private const System.String F9B4 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAMAAACdt4HsAAACT1BMVEVHcExKSkkoKCc9Pj0sLS0HCQ0sLSt6enoICxAcHh4XGBcVFhYICxDb3N3Nzs////4lJiYNER";
    private const System.String A3F9 = "x4eXkMDxH29vYLDA0bHBwREhMICw+oqau2t7gHCg1rbXH4+PgGCQ15fH+IiowqLzyOkJN9fn4VFxgNERYLDhJkZmcPEA8eICQYGh0XGBoKDRLGx8gFChN9f4Pn5+jS";
    private const System.String A28C = "0tTz8/P9/fwJCw9XWVoSFyMZHiwbITNMTVAGCAo/Q02srKwcIjNcXmAOEBMJDBAKCwwMDQ6TlZr5+fni4uP7+/vW1tdRU1YMDxQQExgeIi1RU1ZoaWq9vr4UFReGho";
    private const System.String B7C2 = "ZwcnZucHMHCQuusLEMDRAUGSh0dnh4eHiur692d3mFhogSFh8eIiqlpaRwcnWNjo4PExyLjY4LDQ6TlJafn56rrKyur66ys7Oam5wfJDIrLzPBwcF4en0wNDuIiYyg";
    private const System.String BAC5 = "oqNDRk4gIyltbnBgYWRhYmQfJTQdJDQqLjlVWF11d3uOkJJ9f4JlaGmAgoV1eHoeIy/Cw8OIioy9vr67vLw3O0aZmpsnLDilpaYfJDOXmZpYWV06PUBlZ20qLz4NDx";
    private const System.String C91D = "CBg4czOEUnLT3///8DBQcaITIdJDQGBwkFChYfJjgMDhAJCwwIDRgJDx4iJzUQFSEYHizu7u7BwsMiKDl/gYXr6+seIzAcIS67vL6XmZxvcHOcnqAwNUN1d3w0NjgS";
    private const System.String C5E7 = "FyXx8fEQExQYGRuhoqRJTlsWGydnaW9iZGhXWmFPU13Q0NFdX2SBhIirra9RVmQhIyWxsrQ5Oz0vMTKFh4nIycqInUrpAAAAu3RSTlMAAQIDCvMEAvkOEhXo/v/+B/";
    private const System.String DFA4 = "4HT/56Czjf/f7Z/v7w/vz+/g0dl68iRSIwJsj+/f7+/v7+wAn31foS+/4p8UJcz1WL/v7+/v7+cGmJFhS+QBKmy5r+ZfJeP26LybtYHZU52VejjhZINaHZt0DT9jGM";
    private const System.String E8C1 = "6dt6bapPmOdM3vq60X/y5qzu7NSlkXtyjLH46G7rybjW9Oj////////////////////////////////////////////////////+gGE+DAAABJtJREFUWMPtlfVTI0";
    private const System.String A3A1 = "kYhmeSiUvHfWM4CRAI7hLcbd3d3U/23J1MJiEJRCEKSXBf2P3DbqBq77YqgSv2qq7qqvLMTzNd79s9X38CQWnSpPnPgf+VmANBHMKH6zllCA35MD1ZBBEzdbfu5ulo";
    private const System.String F4D7 = "hzQgEBAOQ8zv6mzIOp3V3f1Zdz7nUHqiSJQ5ml2qvyApP1pRUWepu3mm5/0owpQDI5bJyGNkZkkkTO7kbPnRSoulsuLmt1UQGaZQcBdeBkwhkvfX0xiM3C4+f7i60I";
    private const System.String B74B = "nrXagFp7LuaR6FUlXGw8X47rx99odFBDpC5zf/ymyUM5VWB3dywWWxeW0WtPL753erqnp5AwNXaq9eqz2bQaGkTBTdqLC5kTSOQ5VTHU7urMvm9eIGlc8qnj6vf3Tv";
    private const System.String A920 = "8nfX+27f7us7cTZFVpF1oiY9tRrDMHs8HmcyrQ6ncwG17eG1Pft68Dd1UDoS8BQUvP38q1pekgMRpguViZlddrbicWqh1RmZn9/c3Jyvwc+A/jK0Ht54Ld2QBdhsj+";
    private const System.String C5F1 = "fLaxlJBmS6kGTXxtyxWMzAVsSpVKt3VbBLrDUcsVQ+ePn6iEkgm5PKBLGA7PqV5B9gNEjsCTZwt7ezTGPubSXVunIEsN6y2wXAqEDrXkSLAQDt4WAAGEakJ5KuARbR";
    private const System.String B3B6 = "xHJsxg/Uia2t8Jipg1ToKAKCZa8m0gJAFP3jZ58BHAHuuRY2KJ67fzXFFSCjJPOy0dRhtmNa4J8pdM5uAFYERVEtKNlEf1e0CATsNqNM2woCwRu1yZlPzhu2Y2rg79";
    private const System.String D0C9 = "jZDhnASLzQOVEM2CvzK+sGUKBBaxalwB/2g/agWyD94ZvkJEDITXJsXAaMbn8MjK1u40lQ4wZtsViJEbCWUe/Koge0FxWAWMDkn7t/kZcUQg7SIMe2WKB1dZUdeLMz";
    private const System.String C1E8 = "rnQ6F41GFv7mCc5bLJrlEAvIFGGT0Q1YwRvnk+4QhuF8iblGYFxKJBJxPBHxKmoB/nlNJKLx2mxoZE3lbgsqigzAaPRoPxpI1TiE1eYQHjzMbt9NZCXXFQBsjQVPZK";
    private const System.String E8B2 = "9lGq1Z05rcPkXUYzQJpOp7xBT6frkZmwPsxDsDp6YVSKcmJnb1romONfxKQqFouERgCH5xsSxFGQpJmL1oJLRbCfZxkkTpiIwElnCDiakpV7mmYylcMBeKqtQFxR7t";
    private const System.String D47A = "5YxUldykNJsxO67Hqpny4Wyqc3ZqAn+mp6dd5RUPlpcWFVFVkc/3Rq0+dT5lK+DjBjgYRtJnZXWWFjrxVjA9vbDgWuDqH75YWwqpitZ9Pt+np04e56Vq27niUiYJq1";
    private const System.String FD41 = "ZKsptzdeImpnXXYJY7OVl+Ibvrpx8XoyrVuk/18eCx4zkp5wtHR+eXkuTZwtxMBkLLF5KskwuTXIeDefphf/6tx4qXd+68Gjp3LKeHzNunhUN04aUGsQ6BEDrEydeT";
    private const System.String E6F2 = "HLicKs86k0mmIPWPhwbPnfykHoHKiPs2UkZuP23vbCIahOReamykKks7xTQE/9bz5NGTnN7ejBzeAX0cZvw9dxBdPl+oz+YziHuOcE59Dg+uKjtwthII8F/LMJFOEz";
    private const System.String A6A7 = "fzGe+WRGX/pE6qK4guziMT3hvLh4YAEQlQmjRp0vzv+BNqkXxRJ+EFgQAAAABJRU5ErkJggg==";

    #endregion Base64
}