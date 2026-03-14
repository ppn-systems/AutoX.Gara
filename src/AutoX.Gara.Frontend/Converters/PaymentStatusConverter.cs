// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Payments;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Globalization;

namespace AutoX.Gara.Frontend.Converters;

/// <summary>
/// Returns badge colors for <see cref="PaymentStatus"/>.
/// ConverterParameter: "bg" | "stroke" | "text".
/// </summary>
public sealed class PaymentStatusBadgeColorConverter : IValueConverter
{
    public Object? Convert(Object? value, Type targetType, Object? parameter, CultureInfo culture)
    {
        if (value is not PaymentStatus s)
        {
            return Color.FromArgb("#6B7280");
        }

        String mode = parameter as String ?? "bg";

        // Palette tuned for dark UI.
        return (s, mode) switch
        {
            (PaymentStatus.Paid, "bg") => Color.FromArgb("#052E2B"),
            (PaymentStatus.Paid, "stroke") => Color.FromArgb("#10B981"),
            (PaymentStatus.Paid, "text") => Color.FromArgb("#D1FAE5"),

            (PaymentStatus.PartiallyPaid, "bg") => Color.FromArgb("#3A2508"),
            (PaymentStatus.PartiallyPaid, "stroke") => Color.FromArgb("#F59E0B"),
            (PaymentStatus.PartiallyPaid, "text") => Color.FromArgb("#FEF3C7"),

            (PaymentStatus.Pending, "bg") => Color.FromArgb("#1F2937"),
            (PaymentStatus.Pending, "stroke") => Color.FromArgb("#60A5FA"),
            (PaymentStatus.Pending, "text") => Color.FromArgb("#DBEAFE"),

            (PaymentStatus.Overdue, "bg") => Color.FromArgb("#3B0B0B"),
            (PaymentStatus.Overdue, "stroke") => Color.FromArgb("#EF4444"),
            (PaymentStatus.Overdue, "text") => Color.FromArgb("#FEE2E2"),

            (PaymentStatus.Canceled, "bg") => Color.FromArgb("#2A2A38"),
            (PaymentStatus.Canceled, "stroke") => Color.FromArgb("#9CA3AF"),
            (PaymentStatus.Canceled, "text") => Color.FromArgb("#E5E7EB"),

            (PaymentStatus.Refunded, "bg") => Color.FromArgb("#2E1065"),
            (PaymentStatus.Refunded, "stroke") => Color.FromArgb("#A78BFA"),
            (PaymentStatus.Refunded, "text") => Color.FromArgb("#EDE9FE"),

            // Unpaid (default)
            (_, "bg") => Color.FromArgb("#0E0E16"),
            (_, "stroke") => Color.FromArgb("#2A2A38"),
            (_, "text") => Color.FromArgb("#F1F1F5"),
            _ => Color.FromArgb("#0E0E16"),
        };
    }

    public Object? ConvertBack(Object? value, Type targetType, Object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

