using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Domain.Enums.Payments;

/// <summary>
/// X�c d?nh c�c phuong th?c thanh to�n c� th? sử dụng.
/// </summary>
public enum PaymentMethod : byte
{
    [Display(Name = "Kh�ng c� phuong th?c thanh to�n")]
    None = 0,

    [Display(Name = "Ti?n m?t")]
    Cash = 1,

    [Display(Name = "Chuy?n kho?n ng�n h�ng")]
    BankTransfer = 2,

    [Display(Name = "Th? t�n d?ng")]
    CreditCard = 3,

    [Display(Name = "V� di?n t? Momo")]
    Momo = 4,

    [Display(Name = "V� di?n t? ZaloPay")]
    ZaloPay = 5,

    [Display(Name = "VNPay - C?ng thanh to�n")]
    VNPay = 6,

    [Display(Name = "PayPal - Thanh to�n qu?c t?")]
    PayPal = 7,

    [Display(Name = "Kh�c")]
    Other = 255
}
