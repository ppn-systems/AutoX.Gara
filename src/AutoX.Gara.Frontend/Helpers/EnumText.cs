// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace AutoX.Gara.Frontend.Helpers;

public static class EnumText
{
    public static string Get<TEnum>(TEnum value) where TEnum : struct, System.Enum
    {
        MemberInfo? member = typeof(TEnum).GetMember(value.ToString()).FirstOrDefault();
        if (member is null)
        {
            return value.ToString();
        }

        // Prefer DisplayAttribute (supports localization via resource types if you use it)
        DisplayAttribute? display = member.GetCustomAttribute<DisplayAttribute>();
        if (display is not null)
        {
            string? name = display.GetName();
            if (!string.IsNullOrWhiteSpace(name))
            {
                return name!;
            }
        }

        // Fallback to DescriptionAttribute
        DescriptionAttribute? desc = member.GetCustomAttribute<DescriptionAttribute>();
        if (desc is not null && !string.IsNullOrWhiteSpace(desc.Description))
        {
            return desc.Description;
        }

        return value.ToString();
    }

    public static string[] GetNames<TEnum>() where TEnum : struct, System.Enum
        => System.Enum.GetValues<TEnum>().Select(Get).ToArray();
}

