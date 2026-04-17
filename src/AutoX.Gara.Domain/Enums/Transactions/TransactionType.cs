using System;
using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Domain.Enums.Transactions;

/// <summary>
/// X�c d?nh c�c lo?i giao d?ch t�i ch�nh trong hệ thống.
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Giao d?ch thu ti?n t? kh�ch h�ng ho?c c�c ngu?n kh�c.
    /// - V� d?: Thanh to�n h�a don d?ch v?, b�n ph? t�ng.
    /// </summary>
    [Display(Name = "Thu ti?n")]
    Revenue = 1,

    /// <summary>
    /// Giao d?ch chi ti?n cho c�c kho?n chi ph�.
    /// - V� d?: Mua v?t tu, tr? luong nh�n vi�n.
    /// </summary>
    [Display(Name = "Chi ti?n")]
    Expense = 2,

    /// <summary>
    /// Giao d?ch tr? n?, thanh to�n c�c kho?n vay ho?c c�ng n?.
    /// - V� d?: Thanh to�n c�ng n? nh� cung c?p.
    /// </summary>
    [Display(Name = "Thanh to�n c�ng n?")]
    DebtPayment = 3,

    /// <summary>
    /// Chi ph� s?a ch?a, b?o tr� phuong ti?n ho?c thi?t b?.
    /// - V� d?: Chi ph� thay th? linh ki?n, s?a ch?a xe.
    /// </summary>
    [Display(Name = "Chi ph� s?a ch?a")]
    RepairCost = 4,

    /// <summary>
    /// Giao d?ch t?m ?ng ti?n cho nh�n vi�n ho?c c�c kho?n chi chua ho�n t?t.
    /// </summary>
    [Display(Name = "T?m ?ng")]
    AdvancePayment = 5,

    /// <summary>
    /// Giao d?ch ho�n ti?n cho kh�ch h�ng.
    /// - V� d?: Ho�n ti?n do l?i d?ch v?, ch�nh s�ch b?o h�nh.
    /// </summary>
    [Display(Name = "Ho�n ti?n")]
    Refund = 6,

    /// <summary>
    /// Giao d?ch chuy?n ti?n gi?a c�c t�i kho?n n?i b?.
    /// - V� d?: Chuy?n ti?n t? qu? ti?n m?t sang t�i kho?n ng�n h�ng.
    /// </summary>
    [Display(Name = "Chuy?n kho?n n?i b?")]
    InternalTransfer = 7,

    /// <summary>
    /// Thu ti?n d?t c?c t? kh�ch h�ng.
    /// - V� d?: Kh�ch h�ng d?t c?c cho d?ch v? l?n ho?c mua h�ng tru?c.
    /// </summary>
    [Display(Name = "Ti?n d?t c?c")]
    Deposit = 8
}
