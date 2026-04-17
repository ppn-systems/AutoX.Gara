using System;
using AutoX.Gara.Domain.Enums.Repairs;

namespace AutoX.Gara.Shared.Validation;

/// <summary>
/// Quy tắc kiểm tra tính hợp lệ của Lệnh sửa chữa.
/// </summary>
public static class RepairOrderValidation
{
    public static bool IsValidDates(DateTime orderDate, DateTime? expectedCompletion)
    {
        if (!expectedCompletion.HasValue) return true;
        return expectedCompletion.Value >= orderDate;
    }

    public static bool IsValidDescription(string? desc)
    {
        if (string.IsNullOrEmpty(desc)) return true;
        return desc.Length <= 1000;
    }
}
