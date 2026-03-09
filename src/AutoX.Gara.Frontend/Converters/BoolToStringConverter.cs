// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Microsoft.Maui.Controls;
using System;

namespace AutoX.Gara.Frontend.Converters;

/// <summary>
/// Converts a boolean value to one of two strings split by '|' in the ConverterParameter.
/// Format: "TrueString|FalseString"
/// </summary>
public sealed class BoolToStringConverter : IValueConverter
{
    public Object? Convert(Object? value, Type targetType, Object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not Boolean b || parameter is not String p)
        {
            return null;
        }

        String[] parts = p.Split('|');
        return parts.Length == 2 ? (b ? parts[0] : parts[1]) : null;
    }

    public Object? ConvertBack(Object? value, Type targetType, Object? parameter, System.Globalization.CultureInfo culture)
        => throw new NotImplementedException();
}