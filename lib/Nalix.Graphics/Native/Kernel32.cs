// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace Nalix.Graphics.Native;

/// <summary>
/// Provides native helpers for controlling the Windows console window,
/// including show/hide and visibility checks by using relevant Windows APIs.
/// </summary>
[System.Security.SecuritySafeCritical]
[System.Diagnostics.DebuggerNonUserCode]
[System.Runtime.InteropServices.BestFitMapping(false)]
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public static partial class Kernel32
{
    #region Constants

    private const System.Int32 SW_HIDE = 0;
    private const System.Int32 SW_SHOW = 5;

    private const System.Boolean SET_LAST_ERROR = false;

    private const System.String DLL_KERNEL32 = "kernel32.dll";
    private const System.String ENTRYPOINT_KERNEL32_GET_CONSOLE_WINDOW = "GetConsoleWindow";

    #endregion Constants

    #region Invoke Declarations

    [System.Security.SuppressUnmanagedCodeSecurity]
    [System.Runtime.InteropServices.DefaultDllImportSearchPaths(
        System.Runtime.InteropServices.DllImportSearchPath.System32)]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [System.Runtime.InteropServices.LibraryImport(
        DLL_KERNEL32, SetLastError = SET_LAST_ERROR, EntryPoint = ENTRYPOINT_KERNEL32_GET_CONSOLE_WINDOW,
        StringMarshalling = System.Runtime.InteropServices.StringMarshalling.Utf8)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull(nameof(GET_CONSOLE_WINDOW))]
    private static partial System.IntPtr GET_CONSOLE_WINDOW();

    #endregion Invoke Declarations

    #region APIs

    /// <summary>
    /// Hides the console window belonging to the current process, if one exists.
    /// </summary>
    /// <remarks>
    /// This method has no effect if the process does not own a console window.
    /// </remarks>
    [System.Runtime.CompilerServices.SkipLocalsInit]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void Hide()
    {
        System.IntPtr handle = Kernel32.GET_CONSOLE_WINDOW();

        if (handle != System.IntPtr.Zero)
        {
            User32.SHOW_WINDOW(handle, SW_HIDE);
        }
    }

    /// <summary>
    /// Shows the console window belonging to the current process, if one exists.
    /// </summary>
    /// <remarks>
    /// This method has no effect if the process does not own a console window.
    /// </remarks>
    [System.Runtime.CompilerServices.SkipLocalsInit]
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void Show()
    {
        System.IntPtr handle = Kernel32.GET_CONSOLE_WINDOW();

        if (handle != System.IntPtr.Zero)
        {
            User32.SHOW_WINDOW(handle, SW_SHOW);
        }
    }

    /// <summary>
    /// Determines whether the console window for the current process is visible.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the console window exists and is visible; otherwise, <c>false</c>.
    /// </returns>
    public static System.Boolean IsConsoleVisible()
    {
        System.IntPtr handle = Kernel32.GET_CONSOLE_WINDOW();
        return handle != System.IntPtr.Zero && User32.IS_WINDOW_VISIBLE(handle);
    }

    #endregion APIs
}
