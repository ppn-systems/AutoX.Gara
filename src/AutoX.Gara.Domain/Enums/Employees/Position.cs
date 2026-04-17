using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Domain.Enums.Employees;

/// <summary>
/// �?i di?n cho c�c v? tr� c�ng vi?c trong hệ thống qu?n l� gara � t�.
/// </summary>
public enum Position : byte
{
    [Display(Name = "Kh�ng x�c d?nh")]
    None = 0,

    [Display(Name = "Nh�n vi�n h?c vi?c")]
    Apprentice = 1,

    [Display(Name = "Th? r?a xe")]
    CarWasher = 2,

    [Display(Name = "Th? di?n � t�")]
    AutoElectrician = 3,

    [Display(Name = "Th? m�y g?m")]
    UnderCarMechanic = 4,

    [Display(Name = "Th? d?ng")]
    BodyworkMechanic = 5,

    [Display(Name = "K? thu?t vi�n s?a ch?a chung")]
    Technician = 6,

    [Display(Name = "Nh�n vi�n ti?p nh?n xe")]
    Receptionist = 7,

    [Display(Name = "Nh�n vi�n tu v?n d?ch v?")]
    Advisor = 8,

    [Display(Name = "Nh�n vi�n h? tr? k? thu?t")]
    Support = 9,

    [Display(Name = "Nh�n vi�n k? to�n")]
    Accountant = 10,

    [Display(Name = "Qu?n l� gara")]
    Manager = 11,

    [Display(Name = "Nh�n vi�n b?o tr� thi?t b?")]
    MaintenanceStaff = 12,

    [Display(Name = "�i?u ph?i vi�n kho")]
    InventoryCoordinator = 13,

    [Display(Name = "Gi�m s�t kho")]
    WarehouseSupervisor = 14,

    [Display(Name = "Th? son xe")]
    Painter = 15,

    [Display(Name = "Chuy�n vi�n ch?n do�n l?i xe")]
    DiagnosticSpecialist = 16,

    [Display(Name = "Chuy�n vi�n s?a ch?a d?ng co")]
    EngineSpecialist = 17,

    [Display(Name = "Chuy�n vi�n s?a ch?a h?p s?")]
    TransmissionSpecialist = 18,

    [Display(Name = "Chuy�n vi�n s?a ch?a di?u h�a � t�")]
    ACSpecialist = 19,

    [Display(Name = "Th? m�i b? m?t xe")]
    Grinder = 20,

    [Display(Name = "Nh�n vi�n b?o hi?m xe")]
    InsuranceStaff = 21,

    [Display(Name = "Nh�n vi�n tu v?n ph? t�ng")]
    PartsConsultant = 22,

    [Display(Name = "Nh�n vi�n giao nh?n xe")]
    VehicleDeliveryStaff = 23,

    [Display(Name = "Nh�n vi�n v? sinh gara")]
    CleaningStaff = 24,

    [Display(Name = "Nh�n vi�n b?o v?")]
    Security = 25,

    [Display(Name = "Nh�n vi�n marketing")]
    MarketingStaff = 26,

    [Display(Name = "Nh�n vi�n cham s�c kh�ch h�ng")]
    CustomerService = 27,

    [Display(Name = "Gi�m d?c k? thu?t")]
    TechnicalDirector = 28,

    [Display(Name = "Gi�m d?c d?ch v?")]
    ServiceDirector = 29,

    [Display(Name = "Gi�m d?c di?u h�nh")]
    ExecutiveDirector = 30,

    [Display(Name = "K? thu?t vi�n di?n t? v� l?p tr�nh � t�")]
    ElectronicsAndProgrammingTechnician = 31,

    [Display(Name = "Chuy�n vi�n ki?m tra ch?t lu?ng xe")]
    QualityControlSpecialist = 32,

    [Display(Name = "Nh�n vi�n d?t h�ng ph? t�ng")]
    PartsOrderingStaff = 33,

    [Display(Name = "Chuy�n vi�n b?o h�nh xe")]
    WarrantySpecialist = 34,

    [Display(Name = "Nh�n vi�n thu ng�n")]
    Cashier = 35,

    [Display(Name = "Tru?ng ca l�m vi?c")]
    ShiftSupervisor = 36,

    [Display(Name = "L�i th? xe sau s?a ch?a")]
    TestDriver = 37,

    [Display(Name = "Chuy�n vi�n l?p xe")]
    TireSpecialist = 38,

    [Display(Name = "K? thu?t vi�n hệ thống th?y l?c")]
    HydraulicTechnician = 39
}
