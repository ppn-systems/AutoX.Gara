using System;
using AutoX.Gara.Domain.Enums.Customers;

namespace AutoX.Gara.Shared.Validation;

/// <summary>
/// Quy tắc kiểm tra tính hợp lệ của thông tin Khách hàng.
/// Dùng chung cho cả Client (UI) và Server (Application).
/// </summary>
public static class CustomerValidation
{
    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name) && name.Length >= 2 && name.Length <= 100;

    public static bool IsValidTaxCode(string? taxCode, CustomerType? type)
    {
        if (type == CustomerType.Business)
        {
            return !string.IsNullOrWhiteSpace(taxCode) && taxCode.Length >= 10 && taxCode.Length <= 14;
        }
        return true;
    }

    public static bool IsValidDateOfBirth(DateTime? dob)
    {
        if (!dob.HasValue || dob.Value == default) return true;
        
        var today = DateTime.Today;
        if (dob.Value > today) return false;
        if (dob.Value < today.AddYears(-120)) return false;
        
        return true;
    }

    public static bool IsValidNotes(string? notes)
    {
        if (string.IsNullOrEmpty(notes)) return true;
        return notes.Length <= 500;
    }
}
