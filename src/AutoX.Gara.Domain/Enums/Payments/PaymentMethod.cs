// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Domain.Enums.Payments;

/// <summary>
/// Xác định các phương thức thanh toán có thể sử dụng.
/// </summary>
public enum PaymentMethod : System.Byte
{
    [Display(Name = "Không có phương thức thanh toán")]
    None = 0,

    [Display(Name = "Tiền mặt")]
    Cash = 1,

    [Display(Name = "Chuyển khoản ngân hàng")]
    BankTransfer = 2,

    [Display(Name = "Thẻ tín dụng")]
    CreditCard = 3,

    [Display(Name = "Ví điện tử Momo")]
    Momo = 4,

    [Display(Name = "Ví điện tử ZaloPay")]
    ZaloPay = 5,

    [Display(Name = "VNPay - Cổng thanh toán")]
    VNPay = 6,

    [Display(Name = "PayPal - Thanh toán quốc tế")]
    PayPal = 7,

    [Display(Name = "Khác")]
    Other = 255
}