using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Domain.Enums;

/// <summary>
/// X�c d?nh lo?i gi?m gi� �p d?ng tr�n h�a don.
/// </summary>
public enum DiscountType : byte
{
    /// <summary>
    /// Kh�ng �p d?ng gi?m gi�.
    /// </summary>
    [Display(Name = "Kh�ng �p d?ng gi?m gi�")]
    None = 0,

    /// <summary>
    /// Gi?m gi� theo ph?n tram (%) tr�n t?ng h�a don.
    /// V� d?: 10% s? gi?m 10% tr�n t?ng s? ti?n.
    /// </summary>
    [Display(Name = "Gi?m theo ph?n tram")]
    Percentage = 1,

    /// <summary>
    /// Gi?m gi� theo m?t s? ti?n c? d?nh.
    /// V� d?: Gi?m tr?c ti?p 50,000 VN� tr�n t?ng h�a don.
    /// </summary>
    [Display(Name = "Gi?m theo s? ti?n c? d?nh")]
    Amount = 2,

    /// <summary>
    /// Gi?m gi� theo chuong tr�nh khuy?n m�i d?c bi?t.
    /// - V� d?: Gi?m gi� ng�y l?, s? ki?n, flash sale.
    /// </summary>
    [Display(Name = "Gi?m gi� theo chuong tr�nh khuy?n m�i")]
    Promotional = 3,

    /// <summary>
    /// Gi?m gi� theo m� gi?m gi� ho?c voucher.
    /// - V� d?: Nh?p m� "DISCOUNT50" d? du?c gi?m 50,000 VN�.
    /// </summary>
    [Display(Name = "Gi?m gi� theo m� gi?m gi�")]
    Coupon = 4
}