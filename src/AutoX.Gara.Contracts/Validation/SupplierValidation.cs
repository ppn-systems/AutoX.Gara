using System;
namespace AutoX.Gara.Contracts.Validation;
/// <summary>
/// Quy tắc kiểm tra tính hợp lệ của Nhà cung cấp.
/// </summary>
public static class SupplierValidation
{
    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name) && name.Length >= 2 && name.Length <= 200;
    public static bool IsValidTaxCode(string? taxCode)
    {
        if (string.IsNullOrEmpty(taxCode))
        {
            return true; // Optional for some suppliers
        }
        return taxCode.Length is >= 10 and <= 14;
    }
    public static bool IsValidDates(DateTime contractStart, DateTime? contractEnd) => !contractEnd.HasValue || contractEnd.Value >= contractStart;
}

