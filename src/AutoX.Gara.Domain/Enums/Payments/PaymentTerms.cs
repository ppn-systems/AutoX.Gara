using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Domain.Enums.Payments;

/// <summary>
/// Enum d?i di?n cho c�c di?u kho?n thanh to�n.
/// </summary>
public enum PaymentTerms : byte
{
    [Display(Name = "Kh�ng x�c d?nh")]
    None = 0,

    [Display(Name = "Thanh to�n ngay khi nh?n h�ng")]
    DueOnReceipt = 1,

    [Display(Name = "Thanh to�n trong 7 ng�y")]
    Net7 = 2,

    [Display(Name = "Thanh to�n trong 15 ng�y")]
    Net15 = 3,

    [Display(Name = "Thanh to�n trong 30 ng�y")]
    Net30 = 4,

    [Display(Name = "Th?a thu?n ri�ng")]
    Custom = 255
}