// Copyright (c) 2026 PPN Corporation. All rights reserved.
using System.ComponentModel.DataAnnotations;
namespace AutoX.Gara.Domain.Enums.Cars;
public enum CarType : byte
{
    [Display(Name = "Kh�ng x�c d?nh")]
    None = 0,
    [Display(Name = "Sedan - Xe du l?ch")]
    Sedan = 1,
    [Display(Name = "SUV - Xe th? thao da d?ng")]
    SUV = 2,
    [Display(Name = "Hatchback - Xe c? nh?")]
    Hatchback = 3,
    [Display(Name = "Coupe - Xe th? thao")]
    Coupe = 4,
    [Display(Name = "Convertible - Xe mui tr?n")]
    Convertible = 5,
    [Display(Name = "Pickup - Xe b�n t?i")]
    Pickup = 6,
    [Display(Name = "Minivan - Xe gia d�nh")]
    Minivan = 7,
    [Display(Name = "Truck - Xe t?i")]
    Truck = 8,
    [Display(Name = "Bus - Xe bu�t")]
    Bus = 9,
    [Display(Name = "Motorcycle - Xe m�y")]
    Motorcycle = 10,
    [Display(Name = "Other - Lo?i kh�c")]
    Other = 255
}
