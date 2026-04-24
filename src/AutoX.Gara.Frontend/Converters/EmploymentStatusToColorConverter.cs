// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Employees;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
namespace AutoX.Gara.Frontend.Converters;
public sealed class EmploymentStatusToColorConverter : IValueConverter
{
    public System.Object Convert(System.Object? value, System.Type targetType, System.Object? parameter, System.Globalization.CultureInfo culture)
    {
        return value is null
            ? Color.FromArgb("#6B6B80")
            : value is not EmploymentStatus st
            ? Color.FromArgb("#6B6B80")
            : st switch
            {
                EmploymentStatus.None => Color.FromArgb("#6B6B80"),
                EmploymentStatus.Active => Color.FromArgb("#22C55E"),
                EmploymentStatus.Inactive => Color.FromArgb("#94A3B8"),
                EmploymentStatus.OnLeave => Color.FromArgb("#F59E0B"),
                EmploymentStatus.Terminated => Color.FromArgb("#EF4444"),
                EmploymentStatus.Pending => Color.FromArgb("#60A5FA"),
                _ => Color.FromArgb("#C4C4D4")
            };
    }
    public System.Object ConvertBack(System.Object? value, System.Type targetType, System.Object? parameter, System.Globalization.CultureInfo culture)
        => throw new System.NotSupportedException();
}
