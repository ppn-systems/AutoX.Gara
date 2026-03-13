// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Shared.Enums;

/// <summary>
/// Các cột cho phép sắp xếp khi truy vấn danh sách nhân viên.
/// </summary>
public enum EmployeeSortField : System.Byte
{
    [Display(Name = "Tên")]
    Name = 0,

    [Display(Name = "Email")]
    Email = 1,

    [Display(Name = "Chức vụ")]
    Position = 2,

    [Display(Name = "Trạng thái")]
    Status = 3,

    [Display(Name = "Ngày bắt đầu")]
    StartDate = 4,

    [Display(Name = "Giới tính")]
    Gender = 5,
}