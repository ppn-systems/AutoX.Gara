using AutoX.Gara.Domain.Abstractions;
using AutoX.Gara.Domain.Enums.Employees;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AutoX.Gara.Domain.Entities.Identity;
/// <summary>
/// Lop dai dien cho thong tin luong cua nhan vien.
/// </summary>
[Table(nameof(EmployeeSalary))]
public class EmployeeSalary : AuditEntity<int>
{
    // Fields  
    private decimal _salary;
    private DateTime _effectiveFrom = DateTime.UtcNow;
    private DateTime? _effectiveTo;
    private string _note = string.Empty;
    // Identification Properties
    [Required]
    public int EmployeeId { get; set; }
    [ForeignKey(nameof(EmployeeId))]
    public virtual Employee Employee { get; set; } = default!;
    // Salary Information Properties
    /// <summary>
    /// Muc luong cho mot don vi (thang/ngay/gio).
    /// </summary>
    [Required(ErrorMessage = "Salary is required.")]
    [Range(0, double.MaxValue, ErrorMessage = "Salary must be >= 0.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Salary
    {
        get => _salary;
        set
        {
            if (value < 0)
            {
                throw new ArgumentException("Salary must be >= 0.");
            }
            _salary = value;
        }
    }
    /// <summary>
    /// Loai luong (thang/ngay/gio).
    /// </summary>
    [Required]
    public SalaryType SalaryType { get; set; } = SalaryType.Monthly;
    /// <summary>
    /// So don vi (gio/ngay/thang) ap dung trong dot nay.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Salary unit must be >= 0.")]
    public decimal SalaryUnit { get; set; } = 1;
    /// <summary>
    /// Tong luong thuc te.
    /// </summary>
    [NotMapped]
    public decimal TotalSalary => SalaryType == SalaryType.Monthly ? Salary : Salary * SalaryUnit;
    /// <summary>
    /// Ngay bat dau hieu luc.
    /// </summary>
    [Required(ErrorMessage = "Effective date is required.")]
    public DateTime EffectiveFrom
    {
        get => _effectiveFrom;
        set
        {
            if (value > (_effectiveTo ?? DateTime.MaxValue))
            {
                throw new ArgumentException("EffectiveFrom cannot be later than EffectiveTo.");
            }
            _effectiveFrom = value;
        }
    }
    /// <summary>
    /// Ngay ket thuc hieu luc.
    /// </summary>
    public DateTime? EffectiveTo
    {
        get => _effectiveTo;
        set
        {
            if (value.HasValue && value < EffectiveFrom)
            {
                throw new ArgumentException("EffectiveTo cannot be earlier than EffectiveFrom.");
            }
            _effectiveTo = value;
        }
    }
    /// <summary>
    /// Ghi chu ve muc luong.
    /// </summary>
    [MaxLength(200)]
    public string Note
    {
        get => _note;
        set => _note = value?.Trim() ?? string.Empty;
    }
}
