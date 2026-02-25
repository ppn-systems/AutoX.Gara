// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Domain.Enums.Cars;

/// <summary>
/// Enum định nghĩa các màu xe phổ biến và đặc biệt.
/// </summary>
public enum CarColor : System.Byte
{
    [Display(Name = "Không xác định")]
    None = 0,

    // Màu cơ bản
    [Display(Name = "Đen")]
    Black = 1,

    [Display(Name = "Trắng")]
    White = 2,

    [Display(Name = "Xám")]
    Gray = 3,

    [Display(Name = "Bạc")]
    Silver = 4,

    // Màu phổ biến
    [Display(Name = "Đỏ")]
    Red = 5,

    [Display(Name = "Xanh dương")]
    Blue = 6,

    [Display(Name = "Xanh lá")]
    Green = 7,

    [Display(Name = "Vàng")]
    Yellow = 8,

    [Display(Name = "Nâu")]
    Brown = 9,

    [Display(Name = "Cam")]
    Orange = 10,

    [Display(Name = "Tím")]
    Purple = 11,

    [Display(Name = "Hồng")]
    Pink = 12,

    [Display(Name = "Xanh ngọc")]
    Cyan = 13,

    [Display(Name = "Khác")]
    Other = 255
}