using System;
using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Domain.Enums.Transactions;

/// <summary>
/// Tr?ng th�i c?a m?t giao d?ch t�i ch�nh.
/// </summary>
public enum TransactionStatus
{
    /// <summary>
    /// Giao d?ch dang ch? x? l�.
    /// - H? th?ng chua ho�n t?t vi?c x�c nh?n ho?c chua nh?n được ph?n h?i t? c?ng thanh to�n.
    /// </summary>
    [Display(Name = "�ang ch? x? l�")]
    Pending = 1,

    /// <summary>
    /// Giao d?ch d� được x? l� th�nh c�ng.
    /// - Ti?n d� được chuy?n ho?c nh?n d�ng nhu y�u c?u.
    /// </summary>
    [Display(Name = "Ho�n t?t")]
    Completed = 2,

    /// <summary>
    /// Giao d?ch kh�ng th�nh c�ng.
    /// - C� th? do l?i hệ thống, kh�ng d? ti?n, ho?c b? t? ch?i b?i c?ng thanh to�n.
    /// </summary>
    [Display(Name = "Th?t b?i")]
    Failed = 3,

    /// <summary>
    /// Giao d?ch d� b? h?y b?i kh�ch h�ng ho?c hệ thống tru?c khi x? l� xong.
    /// </summary>
    [Display(Name = "�� h?y")]
    Canceled = 4,

    /// <summary>
    /// Giao d?ch d� được ho�n ti?n cho kh�ch h�ng.
    /// - �p d?ng khi c� l?i ho?c kh�ch h�ng y�u c?u ho�n ti?n.
    /// </summary>
    [Display(Name = "�� ho�n ti?n")]
    Refunded = 5,

    /// <summary>
    /// Giao d?ch b? t?m gi? d? ki?m tra th�m.
    /// - C� th? do nghi ng? gian l?n ho?c c?n x�c minh th�m th�ng tin.
    /// </summary>
    [Display(Name = "�ang xem x�t")]
    UnderReview = 6
}
