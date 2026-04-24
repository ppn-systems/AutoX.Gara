// Copyright (c) 2026 PPN Corporation. All rights reserved.
using System.ComponentModel.DataAnnotations;
namespace AutoX.Gara.Domain.Enums.Payments;
public enum TaxRateType : byte
{
    [Display(Name = "Kh�ng �p d?ng thu? VAT (0%)")]
    None = 0,
    [Display(Name = "Thu? VAT 5%")]
    VAT5 = 5,
    [Display(Name = "Thu? VAT 8% (h? tr? kinh t?)")]
    VAT8 = 8,
    [Display(Name = "Thu? VAT 10% (m?c d?nh)")]
    VAT10 = 10
}
