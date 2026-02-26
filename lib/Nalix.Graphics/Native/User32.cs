// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace Nalix.Graphics.Native;

/// <summary>
/// Native Windows MessageBox với styling custom
/// </summary>
public static partial class User32
{
    #region Constants

    // MessageBox types
    private const System.UInt32 MB_OK = 0x00000000;
    private const System.UInt32 MB_ICONERROR = 0x00000010;
    private const System.UInt32 MB_SYSTEMMODAL = 0x00001000;
    private const System.UInt32 MB_TOPMOST = 0x00040000;

    private const System.Boolean SET_LAST_ERROR = false;
    private const System.String DLL_USER32 = "user32.dll";

    private const System.String ENTRYPOINT_USER32_MESSAGE_BOX = "MessageBoxW";
    private const System.String ENTRYPOINT_USER32_SHOW_WINDOW = "ShowWindow";
    private const System.String ENTRYPOINT_USER32_IS_WINDOW_VISIBLE = "IsWindowVisible";

    #endregion Constants

    #region Invoke Declarations

    [System.Runtime.InteropServices.LibraryImport(
        DLL_USER32, SetLastError = true, EntryPoint = ENTRYPOINT_USER32_MESSAGE_BOX,
        StringMarshalling = System.Runtime.InteropServices.StringMarshalling.Utf16)]
    private static partial System.Int32 MESSAGE_BOX(
        System.IntPtr hWnd,
        System.String lpText,
        System.String lpCaption,
        System.UInt32 uType
    );

    [System.Security.SuppressUnmanagedCodeSecurity]
    [System.Runtime.InteropServices.DefaultDllImportSearchPaths(
    System.Runtime.InteropServices.DllImportSearchPath.System32)]
    [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [System.Runtime.InteropServices.LibraryImport(
    DLL_USER32, SetLastError = SET_LAST_ERROR, EntryPoint = ENTRYPOINT_USER32_IS_WINDOW_VISIBLE,
    StringMarshalling = System.Runtime.InteropServices.StringMarshalling.Utf8)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    internal static partial System.Boolean IS_WINDOW_VISIBLE(System.IntPtr hWnd);

    [System.Security.SuppressUnmanagedCodeSecurity]
    [System.Runtime.InteropServices.DefaultDllImportSearchPaths(
    System.Runtime.InteropServices.DllImportSearchPath.System32)]
    [System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [System.Runtime.InteropServices.LibraryImport(
    DLL_USER32, SetLastError = SET_LAST_ERROR, EntryPoint = ENTRYPOINT_USER32_SHOW_WINDOW,
    StringMarshalling = System.Runtime.InteropServices.StringMarshalling.Utf8)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    internal static partial System.Boolean SHOW_WINDOW(System.IntPtr hWnd, System.Int32 nCmdShow);

    #endregion Invoke Declarations

    #region Public Methods

    /// <summary>
    /// Shows a custom error message box
    /// </summary>
    public static void MessageBox(System.String title, System.String message)
    {
        const System.UInt32 flags = MB_OK | MB_ICONERROR | MB_SYSTEMMODAL | MB_TOPMOST;

        MESSAGE_BOX(System.IntPtr.Zero, message, title, flags);
    }

    #endregion Public Methods
}
