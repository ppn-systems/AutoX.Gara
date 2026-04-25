using AutoX.Gara.Domain.Enums.Customers;
using System;
namespace AutoX.Gara.Contracts.Validation;
/// <summary>
/// Quy tắc kiểm tra tính hợp lệ của thông tin Khách hàng.
/// Dùng chung cho cả Client (UI) và Server (Application).
/// </summary>
public static class CustomerValidation
{
    public static bool IsValidName(string name) => !string.IsNullOrWhiteSpace(name) && name.Length >= 2 && name.Length <= 100;
    public static bool IsValidTaxCode(string taxCode, CustomerType? type) => type != CustomerType.Business || (!string.IsNullOrWhiteSpace(taxCode) && taxCode.Length >= 10 && taxCode.Length <= 14);
    public static bool IsValidDateOfBirth(DateTime? dob)
    {
        if (!dob.HasValue || dob.Value == default)
        {
            return true;
        }
        var today = DateTime.Today;
        return dob.Value <= today && dob.Value >= today.AddYears(-120);
    }
    public static bool IsValidNotes(string notes) => string.IsNullOrEmpty(notes) || notes.Length <= 500;
}


