// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Shared.Enums;

/// <summary>
/// Các cột cho phép sắp xếp khi truy vấn danh sách nhà cung cấp.
/// </summary>
public enum SupplierSortField : System.Byte
{
    /// <summary>Sắp xếp theo tên nhà cung cấp.</summary>
    [Display(Name = "Tên")]
    Name = 0,

    /// <summary>Sắp xếp theo email.</summary>
    [Display(Name = "Email")]
    Email = 1,

    /// <summary>Sắp xếp theo ngày bắt đầu hợp tác.</summary>
    [Display(Name = "Ngày bắt đầu hợp tác")]
    ContractStartDate = 2,

    /// <summary>Sắp xếp theo ngày kết thúc hợp tác.</summary>
    [Display(Name = "Ngày kết thúc hợp tác")]
    ContractEndDate = 3,

    /// <summary>Sắp xếp theo trạng thái.</summary>
    [Display(Name = "Trạng thái")]
    Status = 4,
}