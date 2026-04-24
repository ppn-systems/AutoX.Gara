// Copyright (c) 2026 PPN Corporation. All rights reserved.
using System.ComponentModel.DataAnnotations;
namespace AutoX.Gara.Shared.Enums;
/// <summary>
/// C�c c?t cho ph�p s?p x?p khi truy v?n danh s�ch nh� cung c?p.
/// </summary>
public enum SupplierSortField : byte
{
    /// <summary>S?p x?p theo t�n nh� cung c?p.</summary>
    [Display(Name = "T�n")]
    Name = 0,
    /// <summary>S?p x?p theo email.</summary>
    [Display(Name = "Email")]
    Email = 1,
    /// <summary>S?p x?p theo ng�y b?t d?u h?p t�c.</summary>
    [Display(Name = "Ng�y b?t d?u h?p t�c")]
    ContractStartDate = 2,
    /// <summary>S?p x?p theo ng�y k?t th�c h?p t�c.</summary>
    [Display(Name = "Ng�y k?t th�c h?p t�c")]
    ContractEndDate = 3,
    /// <summary>S?p x?p theo tr?ng th�i.</summary>
    [Display(Name = "Tr?ng th�i")]
    Status = 4,
}
