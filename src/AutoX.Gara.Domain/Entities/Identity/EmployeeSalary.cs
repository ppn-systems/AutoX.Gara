// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Employees;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoX.Gara.Domain.Entities.Identity;

/// <summary>
/// Lớp đại diện cho thông tin lương của nhân viên theo từng đợt với nhiều loại.
/// </summary>
[Table(nameof(EmployeeSalary))]
public class EmployeeSalary
{
    // Fields  
    private System.Decimal _salary;
    private System.DateTime _effectiveFrom = System.DateTime.UtcNow;
    private System.DateTime? _effectiveTo;
    private System.String _note = System.String.Empty;

    // Identification Properties
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public System.Int32 Id { get; protected set; }

    [Required]
    public System.Int32 EmployeeId { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = default!;

    // Salary Information Properties

    /// <summary>
    /// Mức lương cho một đơn vị (tháng/ngày/giờ).
    /// </summary>
    [Required(ErrorMessage = "Salary is required.")]
    [Range(0, System.Double.MaxValue, ErrorMessage = "Salary must be >= 0.")]
    [Column(TypeName = "decimal(18,2)")]
    public System.Decimal Salary
    {
        get => _salary;
        set
        {
            if (value < 0)
            {
                throw new System.ArgumentException("Salary must be >= 0.");
            }

            _salary = value;
        }
    }

    /// <summary>
    /// Loại lương (tháng/ngày/giờ).
    /// </summary>
    [Required]
    public SalaryType SalaryType { get; set; } = SalaryType.Monthly;

    /// <summary>
    /// Số đơn vị (giờ/ngày/tháng) áp dụng trong đợt này.
    /// </summary>
    [Range(0, System.Double.MaxValue, ErrorMessage = "Salary unit must be >= 0.")]
    public System.Decimal SalaryUnit { get; set; } = 1;

    /// <summary>
    /// Tổng lương thực tế (áp dụng cho Daily/Hourly).
    /// </summary>
    [NotMapped]
    public System.Decimal TotalSalary => SalaryType == SalaryType.Monthly ? Salary : Salary * SalaryUnit;

    /// <summary>
    /// Ngày bắt đầu hiệu lực mức lương này.
    /// </summary>
    [Required(ErrorMessage = "Effective date is required.")]
    public System.DateTime EffectiveFrom
    {
        get => _effectiveFrom;
        set
        {
            if (value > (_effectiveTo ?? System.DateTime.MaxValue))
            {
                throw new System.ArgumentException("EffectiveFrom cannot be later than EffectiveTo.");
            }

            _effectiveFrom = value;
        }
    }

    /// <summary>
    /// Ngày kết thúc hiệu lực (null nếu vẫn đang áp dụng).
    /// </summary>
    public System.DateTime? EffectiveTo
    {
        get => _effectiveTo;
        set
        {
            if (value.HasValue && value < EffectiveFrom)
            {
                throw new System.ArgumentException("EffectiveTo cannot be earlier than EffectiveFrom.");
            }

            _effectiveTo = value;
        }
    }

    /// <summary>
    /// Ghi chú về mức lương (phụ cấp, thưởng...)
    /// </summary>
    [MaxLength(200)]
    public System.String Note
    {
        get => _note;
        set => _note = value?.Trim() ?? System.String.Empty;
    }
}
