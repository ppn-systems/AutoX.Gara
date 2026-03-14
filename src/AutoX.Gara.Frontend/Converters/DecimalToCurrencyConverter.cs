// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Globalization;

namespace AutoX.Gara.Frontend.Converters;

/// <summary>
/// Converts a decimal value to currency string formatted according to the current culture.
/// </summary>
public sealed class DecimalToCurrencyConverter : IValueConverter
{
    /// <summary>
    /// Converts a decimal value to a currency string.
    /// </summary>
    /// <param name="value">The decimal value.</param>
    /// <param name="targetType">The target binding type (unused).</param>
    /// <param name="parameter">Optional parameter (unused).</param>
    /// <param name="culture">Culture info for formatting.</param>
    /// <returns>A string formatted as currency if value is decimal; otherwise, an empty string.</returns>
    public Object? Convert(Object? value, Type targetType, Object? parameter, CultureInfo culture)
        => value is Decimal d
            ? d.ToString("C", culture ?? CultureInfo.CurrentCulture)
            : String.Empty;

    /// <summary>
    /// Not supported; throws <see cref="NotImplementedException"/>.
    /// </summary>
    public Object? ConvertBack(Object? value, Type targetType, Object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts <see cref="DateOnly"/> or <see cref="DateTime"/> to a formatted date string "dd/MM/yyyy".
/// </summary>
public sealed class DateOnlyToStringConverter : IValueConverter
{
    /// <summary>
    /// Converts a <see cref="DateOnly"/> or <see cref="DateTime"/> to string.
    /// </summary>
    /// <param name="value">The date value.</param>
    /// <param name="targetType">The target binding type (unused).</param>
    /// <param name="parameter">Optional parameter (unused).</param>
    /// <param name="culture">Culture info for formatting.</param>
    /// <returns>Date string in "dd/MM/yyyy" format, or empty string for invalid values.</returns>
    public Object? Convert(Object? value, Type targetType, Object? parameter, CultureInfo culture)
    {
        return value is DateOnly dateOnly
            ? dateOnly.ToString("dd/MM/yyyy", culture ?? CultureInfo.InvariantCulture)
            : value is DateTime dateTime ? dateTime.ToString("dd/MM/yyyy", culture ?? CultureInfo.InvariantCulture) : String.Empty;
    }

    /// <summary>
    /// Not supported; throws <see cref="NotImplementedException"/>.
    /// </summary>
    public Object? ConvertBack(Object? value, Type targetType, Object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts a boolean value to color: red for <c>true</c> (defective), green for <c>false</c>.
/// </summary>
public sealed class BoolToDefectiveColorConverter : IValueConverter
{
    /// <summary>
    /// Converts a bool value to a color.
    /// </summary>
    /// <param name="value">The bool value indicates defective state.</param>
    /// <param name="targetType">The target type (unused).</param>
    /// <param name="parameter">Optional parameter (unused).</param>
    /// <param name="culture">Culture info (unused).</param>
    /// <returns>Red color for <c>true</c>, green color for <c>false</c>; or gray if invalid.</returns>
    public Object? Convert(Object? value, Type targetType, Object? parameter, CultureInfo culture)
        => value is Boolean b
            ? (b ? Colors.Red : Colors.Green)
            : Colors.Gray; // fallback color for invalid input

    /// <summary>
    /// Not supported; throws <see cref="NotImplementedException"/>.
    /// </summary>
    public Object? ConvertBack(Object? value, Type targetType, Object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
