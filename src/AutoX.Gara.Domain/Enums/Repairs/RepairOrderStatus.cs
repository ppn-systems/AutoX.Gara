using System;
using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Domain.Enums.Repairs;

/// <summary>
/// Enum d?i di?n cho c�c tr?ng th�i c?a don s?a ch?a.
/// </summary>
public enum RepairOrderStatus
{
    [Display(Name = "Kh�ng x�c d?nh")]
    None = 0,

    [Display(Name = "Ch? x�c nh?n")]
    Pending = 1,

    [Display(Name = "�ang ki?m tra xe")]
    Inspecting = 2,  // ?? Giai do?n ki?m tra ban d?u

    [Display(Name = "Ch? b�o gi�")]
    QuotationPending = 3,  // ?? Ch? kh�ch duy?t b�o gi�

    [Display(Name = "Kh�ch h�ng t? ch?i s?a ch?a")]
    RejectedByCustomer = 4,  // ? Kh�ch t? ch?i sau khi b�o gi�

    [Display(Name = "�ang ch? ph? t�ng")]
    WaitingForParts = 5,

    [Display(Name = "�ang s?a ch?a")]
    InProgress = 6,

    [Display(Name = "Ch? ki?m tra sau s?a ch?a")]
    PostRepairInspection = 7,  // ? Ki?m tra l?n cu?i tru?c khi b�n giao

    [Display(Name = "Ho�n th�nh (chua thanh to�n)")]
    Completed = 8,

    [Display(Name = "�� thanh to�n")]
    Paid = 9,

    [Display(Name = "B? t? ch?i b?o hi?m")]
    InsuranceRejected = 10,  // ?? B?o hi?m kh�ng duy?t

    [Display(Name = "�� h?y")]
    Canceled = 11
}
