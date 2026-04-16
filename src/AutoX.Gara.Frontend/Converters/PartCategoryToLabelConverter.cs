// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Parts;

using Microsoft.Maui.Controls;

using Microsoft.Maui.Graphics;

using System;

using System.ComponentModel.DataAnnotations;

using System.Globalization;

using System.Linq;

namespace AutoX.Gara.Frontend.Converters;

/// <summary>

/// Converts <see cref="PartCategory"/> enum to its display label (Vietnamese) using DisplayAttribute.

/// </summary>

public sealed class PartCategoryToLabelConverter : IValueConverter

{
    /// <summary>

    /// Converts PartCategory to short display label.

    /// </summary>

    /// <param name="value">The PartCategory value.</param>

    /// <param name="targetType">The target type (not used).</param>

    /// <param name="parameter">Parameter (not used).</param>

    /// <param name="culture">Culture info (not used).</param>

    /// <returns>Display name (label) if available, else ToString() or empty.</returns>

    public Object? Convert(Object? value, Type targetType, Object? parameter, CultureInfo culture)

    {
        if (value is PartCategory cat)

        {
            // L?y DisplayAttribute n?u khai b�o tr�n enum member

            var member = typeof(PartCategory).GetMember(cat.ToString()).FirstOrDefault();

            if (member != null)

            {
                var display = member.GetCustomAttributes(typeof(DisplayAttribute), false)

                                    .OfType<DisplayAttribute>().FirstOrDefault();

                if (display != null)

                {
                    return display.Name;

                }

            }

            return cat.ToString();

        }

        return String.Empty;

    }

    /// <summary>

    /// Not supported. Throws <see cref="NotImplementedException"/>.

    /// </summary>

    public Object? ConvertBack(Object? value, Type targetType, Object? parameter, CultureInfo culture)

        => throw new NotImplementedException();
}

/// <summary>

/// Converts <see cref="PartCategory"/> to a representative color (Color) for colored badge.

/// </summary>

public sealed class PartCategoryToColorConverter : IValueConverter

{
    /// <summary>

    /// Converts PartCategory to Color for display.

    /// </summary>

    /// <param name="value">The PartCategory value.</param>

    /// <param name="targetType">The target type (not used).</param>

    /// <param name="parameter">Parameter (not used).</param>

    /// <param name="culture">Culture info (not used).</param>

    /// <returns>A MAUI Color representing the category.</returns>

    public Object? Convert(Object? value, Type targetType, Object? parameter, CultureInfo culture)

    {
        if (value is PartCategory cat)

        {
            // B?n n�n t? quy d?nh m�u cho t?ng lo?i, v� d?:

            return cat switch

            {
                PartCategory.Engine => Color.FromArgb("#2563EB"), // Xanh duong

                PartCategory.Transmission => Color.FromArgb("#7C3AED"), // T�m

                PartCategory.FuelInjection => Color.FromArgb("#0891B2"), // Xanh ng?c

                PartCategory.Turbocharger => Color.FromArgb("#D97706"), // Cam

                PartCategory.Lubrication => Color.FromArgb("#059669"), // Xanh l�

                PartCategory.Cooling => Color.FromArgb("#0D9488"), // Xanh th?m

                PartCategory.Fuel => Color.FromArgb("#DC2626"), // �?

                PartCategory.Exhaust => Color.FromArgb("#7C3AED"), // T�m

                PartCategory.Ignition => Color.FromArgb("#F59E42"), // V�ng cam

                PartCategory.Electrical => Color.FromArgb("#9155FD"), // T�m l?

                PartCategory.SensorsAndModules => Color.FromArgb("#1DA1F2"), // Xanh Twitter

                PartCategory.ABS => Color.FromArgb("#FFC300"), // V�ng

                PartCategory.ESC => Color.FromArgb("#33B249"), // Xanh l� s�ng

                PartCategory.Lighting => Color.FromArgb("#FF5733"), // �? cam

                PartCategory.Brake => Color.FromArgb("#C70039"), // �? s?m

                PartCategory.Safety => Color.FromArgb("#22D3EE"), // Xanh cyan

                PartCategory.Airbags => Color.FromArgb("#FBBF24"), // V�ng

                PartCategory.SecurityAndLocking => Color.FromArgb("#6366F1"), // Xanh t�m

                PartCategory.Suspension => Color.FromArgb("#34D399"), // Xanh l� c�y

                PartCategory.Steering => Color.FromArgb("#F87171"), // H?ng nh?t

                PartCategory.WheelAndTire => Color.FromArgb("#A3A3A3"), // X�m

                PartCategory.AirConditioning => Color.FromArgb("#60A5FA"), // Xanh nu?c bi?n

                PartCategory.Interior => Color.FromArgb("#EAB308"), // V�ng n�u

                PartCategory.Entertainment => Color.FromArgb("#EA580C"), // Cam d?m

                PartCategory.Navigation => Color.FromArgb("#818CF8"), // Xanh t�m nh?t

                PartCategory.SeatHeating => Color.FromArgb("#EF4444"), // �? nh?t

                PartCategory.SeatCooling => Color.FromArgb("#14B8A6"), // Xanh ng?c l?c b?o

                PartCategory.Body => Color.FromArgb("#78716C"), // N�u x�m

                PartCategory.MirrorsAndGlass => Color.FromArgb("#F0E68C"), // V�ng nh?t

                PartCategory.ExteriorAccessories => Color.FromArgb("#E879F9"), // H?ng

                PartCategory.InteriorAccessories => Color.FromArgb("#FDE68A"), // V�ng d?m

                PartCategory.CruiseControl => Color.FromArgb("#A21CAF"), // T�m n?i b?t

                PartCategory.ParkingAssist => Color.FromArgb("#2DD4BF"), // Xanh l?c b?o

                PartCategory.RemoteStart => Color.FromArgb("#FB7185"), // H?ng neon

                PartCategory.Maintenance => Color.FromArgb("#A3E635"), // L� non

                PartCategory.SoundDampening => Color.FromArgb("#0EA5E9"), // Xanh bi?n

                PartCategory.BatteryAndModules => Color.FromArgb("#ECECEC"), // X�m tr?ng

                PartCategory.ChargingSystem => Color.FromArgb("#F59E42"), // V�ng cam

                PartCategory.Telematics => Color.FromArgb("#6366F1"), // T�m than

                PartCategory.HUD => Color.FromArgb("#38BDF8"), // Xanh nh?t

                PartCategory.Aerodynamics => Color.FromArgb("#2D3748"),

                PartCategory.SoundProofing => Color.FromArgb("#CBD5E1"),

                PartCategory.UVGlass => Color.FromArgb("#FBB6CE"),

                PartCategory.RoofRack => Color.FromArgb("#B45309"),

                PartCategory.TowHitch => Color.FromArgb("#64748B"),

                PartCategory.None => Color.FromArgb("#9CA3AF"),

                PartCategory.Other => Color.FromArgb("#6B7280"),

                _ => Color.FromArgb("#D1D5DB") // m�u d? ph�ng

            };

        }

        return Color.FromArgb("#D1D5DB"); // fallback color

    }

    /// <summary>

    /// Not supported. Throws <see cref="NotImplementedException"/>.

    /// </summary>

    public Object? ConvertBack(Object? value, Type targetType, Object? parameter, CultureInfo culture)

        => throw new NotImplementedException();
}