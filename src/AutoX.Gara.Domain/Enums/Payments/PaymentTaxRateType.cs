// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Domain.Enums.Payments;

public enum TaxRateType : System.Byte
{
    [Display(Name = "Không áp dụng thuế VAT (0%)")]
    None = 0,

    [Display(Name = "Thuế VAT 5%")]
    VAT5 = 5,

    [Display(Name = "Thuế VAT 8% (hỗ trợ kinh tế)")]
    VAT8 = 8,

    [Display(Name = "Thuế VAT 10% (mặc định)")]
    VAT10 = 10
}