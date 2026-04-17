using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Domain.Enums.Cars;

/// <summary>
/// Enum d?nh nghia c�c m�u xe ph? bi?n v� d?c bi?t.
/// </summary>
public enum CarColor : byte
{
    [Display(Name = "Kh�ng x�c d?nh")]
    None = 0,

    // M�u co b?n
    [Display(Name = "�en")]
    Black = 1,

    [Display(Name = "Tr?ng")]
    White = 2,

    [Display(Name = "X�m")]
    Gray = 3,

    [Display(Name = "B?c")]
    Silver = 4,

    // M�u ph? bi?n
    [Display(Name = "�?")]
    Red = 5,

    [Display(Name = "Xanh duong")]
    Blue = 6,

    [Display(Name = "Xanh l�")]
    Green = 7,

    [Display(Name = "V�ng")]
    Yellow = 8,

    [Display(Name = "N�u")]
    Brown = 9,

    [Display(Name = "Cam")]
    Orange = 10,

    [Display(Name = "T�m")]
    Purple = 11,

    [Display(Name = "H?ng")]
    Pink = 12,

    [Display(Name = "Xanh ng?c")]
    Cyan = 13,

    [Display(Name = "Kh�c")]
    Other = 255
}
