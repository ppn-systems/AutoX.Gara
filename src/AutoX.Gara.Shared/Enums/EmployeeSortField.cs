using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Shared.Enums;

/// <summary>
/// C�c c?t cho ph�p s?p x?p khi truy v?n danh s�ch nh�n vi�n.
/// </summary>
public enum EmployeeSortField : byte
{
    [Display(Name = "T�n")]
    Name = 0,

    [Display(Name = "Email")]
    Email = 1,

    [Display(Name = "Ch?c v?")]
    Position = 2,

    [Display(Name = "Tr?ng th�i")]
    Status = 3,

    [Display(Name = "Ng�y b?t d?u")]
    StartDate = 4,

    [Display(Name = "Gi?i t�nh")]
    Gender = 5,
}
