// Copyright (c) 2026 PPN Corporation. All rights reserved.
using System.ComponentModel.DataAnnotations;
namespace AutoX.Gara.Domain.Enums.Customers;
/// <summary>
/// Enum d?i di?n cho c?p d? th�nh vi�n trong hệ thống.
/// </summary>
public enum MembershipLevel : byte
{
    [Display(Name = "Kh�ng x�c d?nh / Chua dang k�")]
    None = 0,
    [Display(Name = "Kh�ch d�ng th?")]
    Trial = 1,
    [Display(Name = "Kh�ch thu?ng")]
    Standard = 2,
    [Display(Name = "Th�nh vi�n b?c")]
    Silver = 3,
    [Display(Name = "Th�nh vi�n v�ng")]
    Gold = 4,
    [Display(Name = "Th�nh vi�n b?ch kim")]
    Platinum = 5,
    [Display(Name = "Th�nh vi�n kim cuong")]
    Diamond = 6
}
