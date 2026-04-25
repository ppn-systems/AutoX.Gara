using System;
namespace AutoX.Gara.Contracts.Validation;
/// <summary>
/// Quy tắc kiểm tra tính hợp lệ của thông tin Nhân viên.
/// </summary>
public static class EmployeeValidation
{
    public static bool IsValidName(string name) => !string.IsNullOrWhiteSpace(name) && name.Length >= 2 && name.Length <= 100;
    public static bool IsValidDates(DateTime startDate, DateTime? endDate) => !endDate.HasValue || endDate.Value > startDate;
    public static bool IsValidDateOfBirth(DateTime? dob)
    {
        if (!dob.HasValue || dob.Value == default)
        {
            return true;
        }
        var today = DateTime.Today;
        return dob.Value < today && dob.Value > today.AddYears(-100);
    }
}


