// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace Nalix.Graphics.Enums;

/// <summary>
/// Defines Z-order layers used for UI rendering.
/// Higher values are rendered on top of lower ones.
/// </summary>
public enum RenderLayer : System.Int32
{
    Background = 0,

    Notification = 100,
    NotificationButton = 110,

    InputField = 800,

    Spinner = 999,

    Overlay = 1000,
    Banner = 1010,
    Tooltip = 1100,

    /// <summary>
    /// Reserved for components that must always be on top.
    /// </summary>
    Highest = System.Int32.MaxValue - 1
}
