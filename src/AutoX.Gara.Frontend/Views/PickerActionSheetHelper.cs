// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoX.Gara.Frontend.Views;

internal static class PickerActionSheetHelper
{
    public static async Task ShowAsync(VisualElement? anchor, String title, IReadOnlyList<String> options, Action<Int32> applyIndex, String cancelLabel = "Hủy")
    {
        if (options is null || options.Count == 0)
        {
            return;
        }

#if WINDOWS
        if (PickerFlyoutHelper.TryShow(anchor, title, options, applyIndex))
        {
            return;
        }
#endif

        Page? page = Application.Current?.Windows[0].Page;
        if (page is null)
        {
            return;
        }

        String pick = await page.DisplayActionSheetAsync(title, cancelLabel, null, (String[])options).ConfigureAwait(false);
        if (String.IsNullOrWhiteSpace(pick) || String.Equals(pick, cancelLabel, StringComparison.Ordinal))
        {
            return;
        }

        Int32 idx = Array.IndexOf((String[])options, pick);
        if (idx >= 0)
        {
            applyIndex(idx);
        }
    }
}
