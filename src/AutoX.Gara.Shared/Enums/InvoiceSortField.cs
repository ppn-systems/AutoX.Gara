using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace AutoX.Gara.Shared.Enums;

/// <summary>
/// Cac cot duoc phep sap xep trong truy van danh sach hoa don.
/// </summary>
public enum InvoiceSortField : byte
{
    /// <summary>Sap xep theo ngay lap (mac dinh).</summary>
    InvoiceDate = 0,

    /// <summary>Sap xep theo so hoa don.</summary>
    InvoiceNumber = 1,

    /// <summary>Sap xep theo tong tien.</summary>
    TotalAmount = 2,

    /// <summary>Sap xep theo con no.</summary>
    BalanceDue = 3,

    /// <summary>Sap xep theo trang thai thanh toan.</summary>
    PaymentStatus = 4,
}
