// Copyright (c) 2026 PPN Corporation. All rights reserved.
using Microsoft.Maui.Controls;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
namespace AutoX.Gara.Frontend.Converters;
/// <summary>
/// Converts enum values to localized display text using <see cref="EnumText"/>.
/// Optional ConverterParameter: prefix string (e.g. "PTTT: ").
/// </summary>
public sealed class EnumTextConverter : IValueConverter
{
    public System.Object Convert(System.Object? value, System.Type targetType, System.Object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is null)
        {
            return string.Empty;
        }
        string prefix = parameter as string ?? string.Empty;
        if (value is System.Enum e)
        {
            // Same logic as Helpers.EnumText.Get<TEnum>, but works with runtime enum types.
            System.Type enumType = e.GetType();
            MemberInfo? member = enumType.GetMember(e.ToString()).FirstOrDefault();
            if (member is null)
            {
                return prefix + e.ToString();
            }
            DisplayAttribute? display = member.GetCustomAttribute<DisplayAttribute>();
            if (display is not null)
            {
                string? name = display.GetName();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    return prefix + name;
                }
            }
            DescriptionAttribute? desc = member.GetCustomAttribute<DescriptionAttribute>();
            return desc is not null && !string.IsNullOrWhiteSpace(desc.Description) ? prefix + desc.Description : prefix + e.ToString();
        }
        return prefix + value.ToString();
    }
    public System.Object ConvertBack(System.Object? value, System.Type targetType, System.Object? parameter, System.Globalization.CultureInfo culture)
        => throw new System.NotSupportedException();
}
