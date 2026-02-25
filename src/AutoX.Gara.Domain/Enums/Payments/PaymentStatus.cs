// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Domain.Enums.Payments;

/// <summary>
/// Xác định trạng thái thanh toán của hóa đơn.
/// </summary>
public enum PaymentStatus : System.Byte
{
    [Display(Name = "Chưa thanh toán")]
    Unpaid = 0,

    [Display(Name = "Đã thanh toán")]
    Paid = 1,

    [Display(Name = "Đang xử lý")]
    Pending = 2,

    [Display(Name = "Quá hạn")]
    Overdue = 3,

    [Display(Name = "Đã hủy")]
    Canceled = 4,

    [Display(Name = "Thanh toán một phần")]
    PartiallyPaid = 5,

    [Display(Name = "Đã hoàn tiền")]
    Refunded = 6
}