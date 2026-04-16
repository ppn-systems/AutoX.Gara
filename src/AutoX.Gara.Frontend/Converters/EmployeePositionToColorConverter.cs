using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Employees;

using Microsoft.Maui.Controls;

using Microsoft.Maui.Graphics;

namespace AutoX.Gara.Frontend.Converters;

public sealed class EmployeePositionToColorConverter : IValueConverter

{
    public System.Object Convert(System.Object? value, System.Type targetType, System.Object? parameter, System.Globalization.CultureInfo culture)

    {
        if (value is null)

        {
            return Color.FromArgb("#6B6B80");

        }

        if (value is not Position pos)

        {
            return Color.FromArgb("#6B6B80");

        }

        // Tailwind-ish palette tuned for dark background. "One position -> one color".

        return pos switch

        {
            Position.None => Color.FromArgb("#6B6B80"),

            Position.Apprentice => Color.FromArgb("#38BDF8"),

            Position.CarWasher => Color.FromArgb("#2DD4BF"),

            Position.AutoElectrician => Color.FromArgb("#60A5FA"),

            Position.UnderCarMechanic => Color.FromArgb("#34D399"),

            Position.BodyworkMechanic => Color.FromArgb("#F472B6"),

            Position.Technician => Color.FromArgb("#22C55E"),

            Position.Receptionist => Color.FromArgb("#FBBF24"),

            Position.Advisor => Color.FromArgb("#FB923C"),

            Position.Support => Color.FromArgb("#A78BFA"),

            Position.Accountant => Color.FromArgb("#C084FC"),

            Position.Manager => Color.FromArgb("#818CF8"),

            Position.MaintenanceStaff => Color.FromArgb("#4ADE80"),

            Position.InventoryCoordinator => Color.FromArgb("#22D3EE"),

            Position.WarehouseSupervisor => Color.FromArgb("#06B6D4"),

            Position.Painter => Color.FromArgb("#E879F9"),

            Position.DiagnosticSpecialist => Color.FromArgb("#0EA5E9"),

            Position.EngineSpecialist => Color.FromArgb("#16A34A"),

            Position.TransmissionSpecialist => Color.FromArgb("#2563EB"),

            Position.ACSpecialist => Color.FromArgb("#14B8A6"),

            Position.Grinder => Color.FromArgb("#F97316"),

            Position.InsuranceStaff => Color.FromArgb("#93C5FD"),

            Position.PartsConsultant => Color.FromArgb("#FACC15"),

            Position.VehicleDeliveryStaff => Color.FromArgb("#FB7185"),

            Position.CleaningStaff => Color.FromArgb("#A3E635"),

            Position.Security => Color.FromArgb("#EF4444"),

            Position.MarketingStaff => Color.FromArgb("#EC4899"),

            Position.CustomerService => Color.FromArgb("#F59E0B"),

            Position.TechnicalDirector => Color.FromArgb("#6366F1"),

            Position.ServiceDirector => Color.FromArgb("#8B5CF6"),

            Position.ExecutiveDirector => Color.FromArgb("#1D4ED8"),

            Position.ElectronicsAndProgrammingTechnician => Color.FromArgb("#06B6D4"),

            Position.QualityControlSpecialist => Color.FromArgb("#10B981"),

            Position.PartsOrderingStaff => Color.FromArgb("#F97316"),

            Position.WarrantySpecialist => Color.FromArgb("#84CC16"),

            Position.Cashier => Color.FromArgb("#EAB308"),

            Position.ShiftSupervisor => Color.FromArgb("#7C3AED"),

            Position.TestDriver => Color.FromArgb("#94A3B8"),

            Position.TireSpecialist => Color.FromArgb("#3B82F6"),

            Position.HydraulicTechnician => Color.FromArgb("#0EA5E9"),

            _ => Color.FromArgb("#C4C4D4")

        };

    }

    public System.Object ConvertBack(System.Object? value, System.Type targetType, System.Object? parameter, System.Globalization.CultureInfo culture)

        => throw new System.NotSupportedException();
}