using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Employees;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoX.Gara.Domain.Entities.Identity;

/// <summary>
/// Lớp đại diện cho nhân viên.
/// </summary>
[Table(nameof(Employee))]
public class Employee
{
    #region Fields

    private System.String _name;
    private System.String _email;
    private System.String _address;
    private System.String _phoneNumber;

    private System.DateTime? _dateOfBirth;
    private System.DateTime? _endDate;
    private System.DateTime _startDate = System.DateTime.UtcNow;

    #endregion

    #region Identification Properties

    /// <summary>
    /// Mã nhân viên.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public System.Int32 Id { get; set; }

    /// <summary>
    /// Tên nhân viên.
    /// </summary>
    [Required(ErrorMessage = "Employee name is required.")]
    [MaxLength(50)]
    public System.String Name
    {
        get => _name;
        set => _name = value?.Trim() ?? System.String.Empty;
    }

    #endregion

    #region Personal Information Properties

    /// <summary>
    /// Giới tính.
    /// </summary>
    public Gender Gender { get; set; } = Gender.None;

    /// <summary>
    /// Ngày sinh.
    /// </summary>
    public System.DateTime? DateOfBirth
    {
        get => _dateOfBirth;
        set
        {
            if (value.HasValue && value > System.DateTime.UtcNow)
            {
                throw new System.ArgumentException("Date of birth cannot be in the future.");
            }

            _dateOfBirth = value;
        }
    }

    #endregion

    #region Contact Information Properties

    /// <summary>
    /// Địa chỉ nhân viên.
    /// </summary>
    [MaxLength(200)]
    public System.String Address
    {
        get => _address;
        set => _address = value?.Trim() ?? System.String.Empty;
    }

    /// <summary>
    /// Số điện thoại nhân viên.
    /// </summary>
    [MaxLength(14)]
    [RegularExpression(@"^\d{10,14}$", ErrorMessage = "Phone number must be 10-14 digits.")]
    public System.String PhoneNumber
    {
        get => _phoneNumber;
        set => _phoneNumber = value?.Trim() ?? System.String.Empty;
    }

    /// <summary>
    /// Email nhân viên.
    /// </summary>
    [MaxLength(50)]
    [EmailAddress]
    public System.String Email
    {
        get => _email;
        set => _email = value?.Trim() ?? System.String.Empty;
    }

    #endregion

    #region Employment Details Properties

    /// <summary>
    /// Chức vụ.
    /// </summary>
    public Position Position { get; set; } = Position.None;

    /// <summary>
    /// Ngày bắt đầu làm việc.
    /// </summary>
    public System.DateTime StartDate
    {
        get => _startDate;
        set
        {
            if (value > (EndDate ?? System.DateTime.MaxValue))
            {
                throw new System.ArgumentException("Start date cannot be later than end date.");
            }

            _startDate = value;
            UpdateStatus();
        }
    }

    /// <summary>
    /// Ngày kết thúc hợp đồng.
    /// </summary>
    public System.DateTime? EndDate
    {
        get => _endDate;
        set
        {
            _endDate = value;
            UpdateStatus();
        }
    }

    /// <summary>
    /// Trạng thái công việc.
    /// </summary>
    public EmploymentStatus Status { get; set; } = EmploymentStatus.None;

    #endregion

    #region Methods

    /// <summary>
    /// Cập nhật trạng thái công việc.
    /// </summary>
    public void UpdateStatus()
    {
        Status = EndDate < System.DateTime.UtcNow
            ? EmploymentStatus.Inactive
            : StartDate > System.DateTime.UtcNow ? EmploymentStatus.Pending : EmploymentStatus.Active;
    }

    #endregion
}