using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Domain.Enums.Customers;

/// <summary>
/// Enum d?i di?n cho lo?i kh�ch h�ng trong hệ thống.
/// </summary>
public enum CustomerType : byte
{
    [Display(Name = "Kh�ng x�c d?nh")]
    None = 0,

    [Display(Name = "Kh�ch h�ng c� nh�n")]
    Individual = 1,

    [Display(Name = "Doanh nghi?p")]
    Business = 2,

    [Display(Name = "Co quan ch�nh ph?")]
    Government = 3,

    [Display(Name = "Kh�ch h�ng s? h?u nhi?u xe")]
    Fleet = 4,

    [Display(Name = "C�ng ty b?o hi?m")]
    InsuranceCompany = 5,

    [Display(Name = "Kh�ch h�ng VIP")]
    VIP = 6,

    [Display(Name = "Kh�ch h�ng ti?m nang")]
    Potential = 7,

    [Display(Name = "Nh� cung c?p")]
    Supplier = 8,

    [Display(Name = "T? ch?c phi l?i nhu?n")]
    NonProfit = 9,

    [Display(Name = "�?i l�")]
    Dealer = 10,

    [Display(Name = "Lo?i kh�ch h�ng kh�c")]
    Other = 255
}
