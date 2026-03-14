// Copyright (c) 2026 PPN Corporation. All rights reserved.

#if WINDOWS
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
#endif
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;

namespace AutoX.Gara.Frontend.Helpers;

internal static class PickerFlyoutHelper
{
    public static Boolean TryShow(VisualElement? anchor, String title, IReadOnlyList<String> options, Action<Int32> onSelected)
    {
#if WINDOWS
        try
        {
            if (anchor?.Handler?.PlatformView is not Microsoft.UI.Xaml.FrameworkElement fe)
            {
                return false;
            }

            var flyout = new Microsoft.UI.Xaml.Controls.MenuFlyout
            {
                Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft
            };

            flyout.Items.Add(new Microsoft.UI.Xaml.Controls.MenuFlyoutItem { Text = title, IsEnabled = false });
            flyout.Items.Add(new Microsoft.UI.Xaml.Controls.MenuFlyoutSeparator());

            for (Int32 i = 0; i < options.Count; i++)
            {
                Int32 idx = i;
                flyout.Items.Add(new Microsoft.UI.Xaml.Controls.MenuFlyoutItem
                {
                    Text = options[i],
                    Command = new Command(() => onSelected(idx))
                });
            }

            flyout.ShowAt(fe);
            return true;
        }
        catch
        {
            return false;
        }
#else
        return false;
#endif
    }
}
