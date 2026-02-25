// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Domain.Enums.Employees;

/// <summary>
/// Trạng thái làm việc của nhân viên.
/// </summary>
public enum EmploymentStatus : System.Byte
{
    [Display(Name = "Không xác định")]
    None = 0,

    /// <summary>
    /// Nhân viên đang làm việc.
    /// </summary>
    [Display(Name = "Đang làm việc")]
    Active = 1,

    /// <summary>
    /// Nhân viên đã nghỉ việc.
    /// </summary>
    [Display(Name = "Đã nghỉ việc")]
    Inactive = 2,

    /// <summary>
    /// Nhân viên đang nghỉ phép.
    /// </summary>
    [Display(Name = "Đang nghỉ phép")]
    OnLeave = 3,

    /// <summary>
    /// Nhân viên bị chấm dứt hợp đồng.
    /// </summary>
    [Display(Name = "Bị sa thải")]
    Terminated = 4,

    /// <summary>
    /// Nhân viên đã được tuyển dụng nhưng chưa bắt đầu làm việc.
    /// </summary>
    [Display(Name = "Chờ bắt đầu")]
    Pending = 5,
}