using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Domain.Enums;

/// <summary>
/// Enum d?i di?n cho tr?ng th�i c?a nh� cung c?p.
/// </summary>
public enum SupplierStatus : byte
{
    /// <summary>
    /// Chua x�c d?nh tr?ng th�i.
    /// </summary>
    [Display(Name = "Kh�ng x�c d?nh")]
    None = 0,

    /// <summary>
    /// �ang h?p t�c.
    /// </summary>
    [Display(Name = "�ang h?p t�c")]
    Active = 1,

    /// <summary>
    /// Ng?ng h?p t�c.
    /// </summary>
    [Display(Name = "Ng?ng h?p t�c")]
    Inactive = 2,

    /// <summary>
    /// �?i t�c ti?m nang.
    /// </summary>
    [Display(Name = "�?i t�c ti?m nang")]
    Potential = 3,

    /// <summary>
    /// T?m d?ng h?p t�c (do vi ph?m di?u kho?n, ch? xem x�t l?i).
    /// </summary>
    [Display(Name = "T?m d?ng h?p t�c")]
    Suspended = 4,

    /// <summary>
    /// Nh� cung c?p m?i, dang trong qu� tr�nh xem x�t h?p t�c.
    /// </summary>
    [Display(Name = "�ang xem x�t h?p t�c")]
    UnderReview = 5,

    /// <summary>
    /// �� k� h?p d?ng nhung chua b?t d?u cung c?p s?n ph?m/d?ch v?.
    /// </summary>
    [Display(Name = "�� k� h?p d?ng, ch? k�ch ho?t")]
    ContractSigned = 6,

    /// <summary>
    /// �� b? lo?i kh?i danh s�ch h?p t�c vinh vi?n.
    /// </summary>
    [Display(Name = "B? lo?i kh?i hệ thống")]
    Blacklisted = 7
}
