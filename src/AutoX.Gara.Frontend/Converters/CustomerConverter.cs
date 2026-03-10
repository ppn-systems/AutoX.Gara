// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Customers;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Globalization;

namespace AutoX.Gara.Frontend.Converters;

// ═══════════════════════════════════════════════════════════════════════════
// 1. CustomerTypeToColorConverter  — màu badge Loại KH
// ═══════════════════════════════════════════════════════════════════════════
public sealed class CustomerTypeToColorConverter : IValueConverter
{
    public Object? Convert(Object? value, Type targetType, Object? parameter, CultureInfo culture)
        => value is CustomerType t ? t switch
        {
            CustomerType.Individual => Color.FromArgb("#2563EB"), // Xanh dương
            CustomerType.Business => Color.FromArgb("#7C3AED"), // Tím
            CustomerType.Government => Color.FromArgb("#0891B2"), // Xanh ngọc
            CustomerType.Fleet => Color.FromArgb("#059669"), // Xanh lá
            CustomerType.InsuranceCompany => Color.FromArgb("#D97706"), // Cam
            CustomerType.VIP => Color.FromArgb("#DC2626"), // Đỏ (VIP nổi bật)
            CustomerType.Potential => Color.FromArgb("#65A30D"), // Vàng xanh
            CustomerType.Supplier => Color.FromArgb("#9333EA"), // Tím nhạt
            CustomerType.NonProfit => Color.FromArgb("#EA580C"), // Cam đậm
            CustomerType.Dealer => Color.FromArgb("#0D9488"), // Xanh đậm
            CustomerType.Other => Color.FromArgb("#6B7280"), // Xám
            _ => Color.FromArgb("#9CA3AF")  // None
        } : Color.FromArgb("#9CA3AF");

    public Object? ConvertBack(Object? value, Type targetType, Object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

// ═══════════════════════════════════════════════════════════════════════════
// 2. CustomerTypeToLabelConverter  — nhãn ngắn badge Loại KH
// ═══════════════════════════════════════════════════════════════════════════
public sealed class CustomerTypeToLabelConverter : IValueConverter
{
    public Object? Convert(Object? value, Type targetType, Object? parameter, CultureInfo culture)
        => value is CustomerType t ? t switch
        {
            CustomerType.Individual => "Cá nhân",
            CustomerType.Business => "DN",
            CustomerType.Government => "Chính phủ",
            CustomerType.Fleet => "Fleet",
            CustomerType.InsuranceCompany => "BH",
            CustomerType.VIP => "VIP",
            CustomerType.Potential => "Tiềm năng",
            CustomerType.Supplier => "NCC",
            CustomerType.NonProfit => "NPO",
            CustomerType.Dealer => "Đại lý",
            CustomerType.Other => "Khác",
            _ => String.Empty
        } : String.Empty;

    public Object? ConvertBack(Object? value, Type targetType, Object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

// ═══════════════════════════════════════════════════════════════════════════
// 3. MembershipToColorConverter  — màu badge Hạng thành viên
// ═══════════════════════════════════════════════════════════════════════════
public sealed class MembershipToColorConverter : IValueConverter
{
    public Object? Convert(Object? value, Type targetType, Object? parameter, CultureInfo culture)
        => value is MembershipLevel m ? m switch
        {
            MembershipLevel.Trial => Color.FromArgb("#6B7280"), // Xám — dùng thử
            MembershipLevel.Standard => Color.FromArgb("#92400E"), // Nâu — thường
            MembershipLevel.Silver => Color.FromArgb("#64748B"), // Xám bạc
            MembershipLevel.Gold => Color.FromArgb("#B45309"), // Vàng
            MembershipLevel.Platinum => Color.FromArgb("#0891B2"), // Xanh ngọc
            MembershipLevel.Diamond => Color.FromArgb("#7C3AED"), // Tím kim cương
            _ => Color.FromArgb("#D1D5DB")  // None
        } : Color.FromArgb("#D1D5DB");

    public Object? ConvertBack(Object? value, Type targetType, Object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

// ═══════════════════════════════════════════════════════════════════════════
// 4. MembershipToLabelConverter  — nhãn ngắn badge Hạng thành viên
// ═══════════════════════════════════════════════════════════════════════════
public sealed class MembershipToLabelConverter : IValueConverter
{
    public Object? Convert(Object? value, Type targetType, Object? parameter, CultureInfo culture)
        => value is MembershipLevel m ? m switch
        {
            MembershipLevel.Trial => "Trial",
            MembershipLevel.Standard => "Standard",
            MembershipLevel.Silver => "Silver",
            MembershipLevel.Gold => "Gold",
            MembershipLevel.Platinum => "Platinum",
            MembershipLevel.Diamond => "Diamond",
            _ => String.Empty
        } : String.Empty;

    public Object? ConvertBack(Object? value, Type targetType, Object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

// ═══════════════════════════════════════════════════════════════════════════
// 5. InitialsConverter  — "Nguyễn Văn Phúc" → "NP"
// ═══════════════════════════════════════════════════════════════════════════
public sealed class InitialsConverter : IValueConverter
{
    public Object? Convert(Object? value, Type targetType, Object? parameter, CultureInfo culture)
    {
        if (value is not String name || String.IsNullOrWhiteSpace(name))
        {
            return "?";
        }

        String[] parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2
            ? $"{Char.ToUpperInvariant(parts[0][0])}{Char.ToUpperInvariant(parts[^1][0])}"
            : name.Length >= 2
            ? $"{Char.ToUpperInvariant(name[0])}{Char.ToUpperInvariant(name[1])}"
            : name[..1].ToUpperInvariant();
    }

    public Object? ConvertBack(Object? value, Type targetType, Object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

// ═══════════════════════════════════════════════════════════════════════════
// 6. InitialsBackgroundConverter  — màu avatar deterministic theo tên
// ═══════════════════════════════════════════════════════════════════════════
public sealed class InitialsBackgroundConverter : IValueConverter
{
    private static readonly Color[] Palette =
    [
        Color.FromArgb("#2563EB"),
        Color.FromArgb("#7C3AED"),
        Color.FromArgb("#DB2777"),
        Color.FromArgb("#D97706"),
        Color.FromArgb("#059669"),
        Color.FromArgb("#DC2626"),
        Color.FromArgb("#0891B2"),
        Color.FromArgb("#65A30D"),
        Color.FromArgb("#9333EA"),
        Color.FromArgb("#EA580C"),
    ];

    public Object? Convert(Object? value, Type targetType, Object? parameter, CultureInfo culture)
    {
        if (value is not String name || String.IsNullOrWhiteSpace(name))
        {
            return Palette[0];
        }

        Int32 hash = 0;
        foreach (Char c in name)
        {
            hash += c;
        }

        return Palette[Math.Abs(hash) % Palette.Length];
    }

    public Object? ConvertBack(Object? value, Type targetType, Object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

// ═══════════════════════════════════════════════════════════════════════════
// 7. DateTimeToShortStringConverter  — DateTime → "10/03/2026"
// ═══════════════════════════════════════════════════════════════════════════
public sealed class DateTimeToShortStringConverter : IValueConverter
{
    public Object? Convert(Object? value, Type targetType, Object? parameter, CultureInfo culture)
    {
        if (value is DateTime dt && dt != default)
        {
            return dt.ToLocalTime().ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        }

        return value is DateTimeOffset dto ? dto.LocalDateTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : String.Empty;
    }

    public Object? ConvertBack(Object? value, Type targetType, Object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

// ═══════════════════════════════════════════════════════════════════════════
// 8. StringNotEmptyConverter  — string? → bool
// ═══════════════════════════════════════════════════════════════════════════
public sealed class StringNotEmptyConverter : IValueConverter
{
    public Object? Convert(Object? value, Type targetType, Object? parameter, CultureInfo culture)
        => value is String s && !String.IsNullOrWhiteSpace(s);

    public Object? ConvertBack(Object? value, Type targetType, Object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

// ═══════════════════════════════════════════════════════════════════════════
// 9. InverseBoolConverter  — bool → !bool
// ═══════════════════════════════════════════════════════════════════════════
public sealed class InverseBoolConverter : IValueConverter
{
    public static readonly InverseBoolConverter Instance = new();

    public Object? Convert(Object? value, Type targetType, Object? parameter, CultureInfo culture)
        => value is Boolean b ? !b : value;

    public Object? ConvertBack(Object? value, Type targetType, Object? parameter, CultureInfo culture)
        => value is Boolean b ? !b : value;
}