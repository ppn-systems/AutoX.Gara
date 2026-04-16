using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Domain.Enums.Cars;

/// <summary>
/// Enum d?nh nghia c�c h�ng xe.
/// </summary>
public enum CarBrand : byte
{
    [Display(Name = "Kh�ng x�c d?nh")]
    None = 0,

    [Display(Name = "Audi")]
    Audi = 1,

    [Display(Name = "Bentley")]
    Bentley = 8,

    [Display(Name = "BMW")]
    BMW = 9,

    [Display(Name = "BYD")]
    BYD = 10,

    [Display(Name = "Bugatti")]
    Bugatti = 12,

    [Display(Name = "Buick")]
    Buick = 13,

    [Display(Name = "Cadillac")]
    Cadillac = 16,

    [Display(Name = "Chevrolet")]
    Chevrolet = 19,

    [Display(Name = "Ford")]
    Ford = 33,

    [Display(Name = "Ferrari")]
    Ferrari = 35,

    [Display(Name = "Honda")]
    Honda = 46,

    [Display(Name = "Hyundai")]
    Hyundai = 48,

    [Display(Name = "Jaguar")]
    Jaguar = 53,

    [Display(Name = "Jeep")]
    Jeep = 55,

    [Display(Name = "KIA")]
    KIA = 58,

    [Display(Name = "Lamborghini")]
    Lamborghini = 61,

    [Display(Name = "Land Rover")]
    LandRover = 64,

    [Display(Name = "Lexus")]
    Lexus = 66,

    [Display(Name = "Mazda")]
    Mazda = 72,

    [Display(Name = "McLaren")]
    McLaren = 73,

    [Display(Name = "Mercedes-Benz")]
    MercedesBenz = 74,

    [Display(Name = "Mitsubishi")]
    Mitsubishi = 77,

    [Display(Name = "Nissan")]
    Nissan = 82,

    [Display(Name = "Porsche")]
    Porsche = 87,

    [Display(Name = "Rolls-Royce")]
    RollsRoyce = 97,

    [Display(Name = "Subaru")]
    Subaru = 105,

    [Display(Name = "Suzuki")]
    Suzuki = 106,

    [Display(Name = "Tesla")]
    Tesla = 110,

    [Display(Name = "Toyota")]
    Toyota = 111,

    [Display(Name = "VinFast")]
    VinFast = 112,

    [Display(Name = "Volvo")]
    Volvo = 115,

    [Display(Name = "Volkswagen")]
    Volkswagen = 116,

    [Display(Name = "Kh�c")]
    Other = 255
}