using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Domain.Enums.Payments;

/// <summary>
/// X�c d?nh tr?ng th�i thanh to�n c?a h�a don.
/// </summary>
public enum PaymentStatus : byte
{
    [Display(Name = "Chua thanh to�n")]
    Unpaid = 0,

    [Display(Name = "�� thanh to�n")]
    Paid = 1,

    [Display(Name = "�ang x? l�")]
    Pending = 2,

    [Display(Name = "Qu� h?n")]
    Overdue = 3,

    [Display(Name = "�� h?y")]
    Canceled = 4,

    [Display(Name = "Thanh to�n m?t ph?n")]
    PartiallyPaid = 5,

    [Display(Name = "�� ho�n ti?n")]
    Refunded = 6
}
