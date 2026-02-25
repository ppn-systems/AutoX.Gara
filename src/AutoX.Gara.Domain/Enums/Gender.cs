// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Domain.Enums;

/// <summary>
/// Đại diện cho giới tính của nhân viên.
/// </summary>
public enum Gender : System.Byte
{
    /// <summary>
    /// Giới tính không xác định hoặc không cung cấp.
    /// </summary>
    [Display(Name = "Không xác định")]
    None = 0,

    /// <summary>
    /// Giới tính nam.
    /// </summary>
    [Display(Name = "Nam")]
    Male = 1,

    /// <summary>
    /// Giới tính nữ.
    /// </summary>
    [Display(Name = "Nữ")]
    Female = 2
}