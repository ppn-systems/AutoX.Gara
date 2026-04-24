// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Domain.Enums.Employees;

public enum SalaryType
{
    [Display(Name = "Kh�ng x�c d?nh")]
    None = 0,

    [Display(Name = "Theo th�ng")]
    Monthly = 1,

    [Display(Name = "Theo ng�y")]
    Daily = 2,

    [Display(Name = "Theo gi?")]
    Hourly = 3
}
