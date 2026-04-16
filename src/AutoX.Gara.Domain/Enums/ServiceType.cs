using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Domain.Enums;

/// <summary>
/// Enum d?i di?n cho c�c lo?i d?ch v? trong gara � t�.
/// </summary>
public enum ServiceType : byte
{
    [Display(Name = "Kh�ng x�c d?nh")]
    None = 0,

    // ?? **B?o tr� & b?o du?ng**
    [Display(Name = "B?o du?ng d?nh k?")]
    Maintenance = 1,

    [Display(Name = "Ki?m tra xe")]
    Inspection = 2,

    [Display(Name = "Thay d?u & b? l?c")]
    OilChange = 3,

    [Display(Name = "D?ch v? l?p xe (Thay, v�, c�n b?ng)")]
    TireService = 4,

    [Display(Name = "C�n ch?nh g�c d?t b�nh xe (Alignment)")]
    WheelAlignment = 5,

    [Display(Name = "D?ch v? di?u h�a kh�ng kh�")]
    ACService = 6,

    // ?? **S?a ch?a chung**
    [Display(Name = "D?ch v? s?a ch?a")]
    Repair = 10,

    [Display(Name = "S?a ch?a d?ng co")]
    EngineRepair = 11,

    [Display(Name = "S?a ch?a h?p s? & truy?n d?ng")]
    TransmissionRepair = 12,

    [Display(Name = "S?a ch?a h? th?ng phanh")]
    BrakeRepair = 13,

    [Display(Name = "S?a ch?a h? th?ng l�i & treo")]
    SuspensionRepair = 14,

    [Display(Name = "S?a ch?a h? th?ng nhi�n li?u")]
    FuelSystemRepair = 15,

    [Display(Name = "D?ch v? di?n & ?c quy")]
    ElectricalService = 16,

    [Display(Name = "S?a ch?a h? th?ng d�nh l?a")]
    IgnitionRepair = 17,

    // ?? **L�m d?p & ph?c h?i xe**
    [Display(Name = "R?a xe & cham s�c n?i th?t")]
    CarWashAndDetailing = 20,

    [Display(Name = "Son & l�m d?p xe")]
    Painting = 21,

    [Display(Name = "Ph?c h?i d�n pha & k�nh xe")]
    HeadlightRestoration = 22,

    [Display(Name = "D�n phim c�ch nhi?t & b?o v? son")]
    WindowTintingAndPPF = 23,

    [Display(Name = "D?ch v? ph? ceramic & nano coating")]
    CeramicCoating = 24,

    // ?? **D?ch v? an to�n & ki?m d?nh**
    [Display(Name = "D?ch v? ki?m d?nh xe")]
    VehicleInspection = 30,

    [Display(Name = "Ki?m tra & l?p d?t camera h�nh tr�nh")]
    DashcamInstallation = 31,

    [Display(Name = "L?p d?t & s?a ch?a h? th?ng c?m bi?n h? tr? l�i")]
    ParkingSensorAndADAS = 32,

    // ?? **D?ch v? kh?n c?p**
    [Display(Name = "D?ch v? c?u h? xe kh?n c?p")]
    EmergencyRoadsideAssistance = 40,

    [Display(Name = "D?ch v? k�o xe")]
    TowingService = 41,

    [Display(Name = "H? tr? kh?i d?ng xe (Nh?y b�nh)")]
    JumpStartService = 42,

    [Display(Name = "H? tr? m? kh�a xe")]
    LockoutAssistance = 43,

    [Display(Name = "Cung c?p nhi�n li?u kh?n c?p")]
    EmergencyFuelDelivery = 44,

    [Display(Name = "Kh�c")]
    Other = 255
}