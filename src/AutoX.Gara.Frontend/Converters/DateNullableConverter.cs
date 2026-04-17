// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Microsoft.Maui.Controls;

using System;

using System.Globalization;

namespace AutoX.Gara.Frontend.Converters;

/// <summary>

/// Converts between <see cref="DateTime?"/> (ViewModel) and <see cref="DateTime"/> (DatePicker).

/// Uses <see cref="DateTime.Today"/> as the fallback when the value is null.

/// </summary>

public sealed class DateNullableConverter : IValueConverter

{
    public Object Convert(Object? value, Type targetType, Object? parameter, CultureInfo culture)

        => value is DateTime dt ? dt : DateTime.Today;

    public Object? ConvertBack(Object? value, Type targetType, Object? parameter, CultureInfo culture)

        => value is DateTime dt ? dt : (DateTime?)null;
}
