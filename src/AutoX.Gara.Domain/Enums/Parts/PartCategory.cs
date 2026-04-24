// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Domain.Enums.Parts;

/// <summary>
/// Danh m?c ph? t�ng � t�.
/// </summary>
public enum PartCategory : byte
{
    [Display(Name = "Kh�ng x�c d?nh")]
    None = 0,

    // ?? �?ng co & truy?n d?ng
    [Display(Name = "Ph? t�ng d?ng co")]
    Engine = 1,

    [Display(Name = "Ph? t�ng truy?n d?ng")]
    Transmission = 2,

    [Display(Name = "H? th?ng phun nhi�n li?u")]
    FuelInjection = 3,

    [Display(Name = "B? tang �p")]
    Turbocharger = 4,

    [Display(Name = "H? th?ng b�i tron")]
    Lubrication = 5,

    [Display(Name = "H? th?ng l�m m�t")]
    Cooling = 6,

    [Display(Name = "H? th?ng nhi�n li?u")]
    Fuel = 7,

    [Display(Name = "H? th?ng x?")]
    Exhaust = 8,

    [Display(Name = "H? th?ng d�nh l?a")]
    Ignition = 9,

    // ? H? th?ng di?n & di?u khi?n
    [Display(Name = "Ph? t�ng di?n")]
    Electrical = 10,

    [Display(Name = "C?m bi?n v� m�-dun di?u khi?n")]
    SensorsAndModules = 11,

    [Display(Name = "H? th?ng ch?ng b� c?ng phanh")]
    ABS = 12,

    [Display(Name = "H? th?ng ?n d?nh di?n t?")]
    ESC = 13,

    [Display(Name = "H? th?ng chi?u s�ng")]
    Lighting = 14,

    // ?? H? th?ng an to�n
    [Display(Name = "Ph? t�ng phanh")]
    Brake = 15,

    [Display(Name = "H? th?ng an to�n")]
    Safety = 16,

    [Display(Name = "T�i kh� v� thi?t b? an to�n")]
    Airbags = 17,

    [Display(Name = "H? th?ng kh�a v� an ninh")]
    SecurityAndLocking = 18,

    // ?? Khung g?m & treo
    [Display(Name = "H? th?ng treo")]
    Suspension = 19,

    [Display(Name = "H? th?ng l�i")]
    Steering = 20,

    [Display(Name = "B�nh xe v� l?p")]
    WheelAndTire = 21,

    // ?? N?i th?t & ti?n nghi
    [Display(Name = "H? th?ng di?u h�a")]
    AirConditioning = 22,

    [Display(Name = "N?i th?t xe")]
    Interior = 23,

    [Display(Name = "H? th?ng gi?i tr�")]
    Entertainment = 24,

    [Display(Name = "H? th?ng d?nh v?")]
    Navigation = 25,

    [Display(Name = "H? th?ng su?i gh?")]
    SeatHeating = 26,

    [Display(Name = "H? th?ng l�m m�t gh?")]
    SeatCooling = 27,

    // ?? Ngo?i th?t & ph? ki?n
    [Display(Name = "Ph? t�ng th�n xe")]
    Body = 28,

    [Display(Name = "Guong v� k�nh")]
    MirrorsAndGlass = 29,

    [Display(Name = "Ph? ki?n ngo?i th?t")]
    ExteriorAccessories = 30,

    [Display(Name = "Ph? ki?n n?i th?t")]
    InteriorAccessories = 31,

    // ?? C�ng ngh? h? tr? l�i xe
    [Display(Name = "H? th?ng di?u khi?n h�nh tr�nh")]
    CruiseControl = 32,

    [Display(Name = "Camera v� c?m bi?n d? xe")]
    ParkingAssist = 33,

    [Display(Name = "H? th?ng kh?i d?ng t? xa")]
    RemoteStart = 34,

    // ?? B?o tr� & b?o du?ng
    [Display(Name = "Ph? t�ng b?o du?ng")]
    Maintenance = 35,

    [Display(Name = "H? th?ng ch?ng ?n")]
    SoundDampening = 36,

    // ?? H? th?ng nhi�n li?u ti�n ti?n (EV & Hybrid)
    [Display(Name = "Pin v� m�-dun di?n")]
    BatteryAndModules = 37,

    [Display(Name = "B? s?c v� hệ thống qu?n l� pin")]
    ChargingSystem = 38,

    // ?? H? th?ng di?u hu?ng & vi?n th�ng
    [Display(Name = "H? th?ng vi?n th�ng & Internet")]
    Telematics = 39,

    [Display(Name = "M�n h�nh hi?n th? HUD")]
    HUD = 40,

    // ?? H? th?ng kh� d?ng h?c
    [Display(Name = "C�nh gi� v� b? khu?ch t�n")]
    Aerodynamics = 41,

    // ?? H? th?ng c�ch �m & c�ch nhi?t
    [Display(Name = "C�ch �m & ch?ng rung")]
    SoundProofing = 42,

    [Display(Name = "K�nh ch?ng UV v� c�ch nhi?t")]
    UVGlass = 43,

    // ?? Ph? ki?n chuy�n d?ng
    [Display(Name = "Gi� n�c v� h?p ch?a d?")]
    RoofRack = 44,

    [Display(Name = "B? m�c k�o xe")]
    TowHitch = 45,

    [Display(Name = "B? l?c kh�ng kh� v� nhi�n li?u")]
    Filter = 46,

    // ? Kh�c
    [Display(Name = "Ph? t�ng kh�c")]
    Other = 255
}
