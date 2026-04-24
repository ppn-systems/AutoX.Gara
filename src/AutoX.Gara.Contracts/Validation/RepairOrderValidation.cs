using System;
namespace AutoX.Gara.Contracts.Validation;
/// <summary>
/// Quy tắc kiểm tra tính hợp lệ của Lệnh sửa chữa.
/// </summary>
public static class RepairOrderValidation
{
    public static bool IsValidDates(DateTime orderDate, DateTime? expectedCompletion) => !expectedCompletion.HasValue || expectedCompletion.Value >= orderDate;
    public static bool IsValidDescription(string? desc) => string.IsNullOrEmpty(desc) || desc.Length <= 1000;
}

