// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Domain.Enums.Employees;

public enum SalaryType
{
    [Display(Name = "Không xác định")]
    None = 0,

    [Display(Name = "Theo tháng")]
    Monthly = 1,

    [Display(Name = "Theo ngày")]
    Daily = 2,

    [Display(Name = "Theo giờ")]
    Hourly = 3
}