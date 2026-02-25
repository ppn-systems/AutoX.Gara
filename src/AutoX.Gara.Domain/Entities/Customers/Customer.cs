// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Repairs;
using AutoX.Gara.Domain.Enums.Customers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoX.Gara.Domain.Entities.Customers;

/// <summary>
/// Lớp đại diện cho khách hàng.
/// </summary>
[Table(nameof(Customer))]
public class Customer
{
    #region Fields

    private System.DateTime? _dateOfBirth;

    private System.String _email = System.String.Empty;
    private System.String _address = System.String.Empty;
    private System.String _taxCode = System.String.Empty;
    private System.String _fullName = System.String.Empty;
    private System.String _phoneNumber = System.String.Empty;

    #endregion

    #region Identification Properties

    /// <summary>
    /// Mã khách hàng.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public System.Int32 Id { get; set; }

    /// <summary>
    /// Họ và tên khách hàng.
    /// </summary>
    [Required(ErrorMessage = "Full name is required.")]
    [MaxLength(100, ErrorMessage = "Full name must not exceed 100 characters.")]
    public System.String Name
    {
        get => _fullName;
        set => _fullName = value.Trim() ?? System.String.Empty;
    }

    #endregion

    #region Contact Information Properties

    /// <summary>
    /// Số điện thoại của khách hàng.
    /// </summary>
    [Required(ErrorMessage = "Phone number is required.")]
    [MaxLength(12, ErrorMessage = "Phone number must not exceed 30 characters.")]
    [Phone(ErrorMessage = "Invalid phone number format.")]
    [RegularExpression(@"^\d{10,12}$", ErrorMessage = "Phone number must be 10-12 digits.")]
    public System.String PhoneNumber
    {
        get => _phoneNumber;
        set => _phoneNumber = value?.Trim() ?? System.String.Empty;
    }

    /// <summary>
    /// Email của khách hàng.
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(100, ErrorMessage = "Email must not exceed 100 characters.")]
    public System.String Email
    {
        get => _email;
        set => _email = value?.Trim() ?? System.String.Empty;
    }

    /// <summary>
    /// Địa chỉ của khách hàng.
    /// </summary>
    [MaxLength(255, ErrorMessage = "Address must not exceed 255 characters.")]
    public System.String Address
    {
        get => _address;
        set => _address = value.Trim() ?? System.String.Empty;
    }

    #endregion

    #region Personal Details Properties

    /// <summary>
    /// Sinh nhật của khách hàng.
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

    /// <summary>
    /// Mã số thuế của khách hàng (nếu có).
    /// </summary>
    [MaxLength(13, ErrorMessage = "Tax code must not exceed 20 characters.")]
    public System.String TaxCode
    {
        get => _taxCode;
        set => _taxCode = value.Trim() ?? System.String.Empty;
    }

    #endregion

    #region Membership and Financial Properties

    /// <summary>
    /// Loại khách hàng.
    /// </summary>
    public CustomerType Type { get; set; } = CustomerType.Individual;

    /// <summary>
    /// Cấp độ thành viên.
    /// </summary>
    public MembershipLevel Membership { get; set; } = MembershipLevel.Standard;

    /// <summary>
    /// Công nợ của khách hàng.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, System.Double.MaxValue, ErrorMessage = "Debt cannot be negative.")]
    public System.Decimal Debt { get; set; } = 0;

    #endregion

    #region Audit Properties

    /// <summary>
    /// Người đã tạo khách hàng trong hệ thống
    /// </summary>
    public System.DateTime CreatedAt { get; set; } = System.DateTime.UtcNow;

    /// <summary>
    /// Người gần nhất chỉnh sửa thông tin khách hàng.
    /// </summary>
    public System.DateTime UpdatedAt { get; set; } = System.DateTime.UtcNow;

    #endregion

    #region Related Entities Properties

    /// <summary>
    /// Danh sách xe của khách hàng.
    /// </summary>
    public virtual ICollection<Vehicle> Vehicles { get; set; } = [];

    /// <summary>
    /// Lịch sử sửa chữa của khách hàng.
    /// </summary>
    public virtual ICollection<RepairOrder> RepairOrders { get; set; } = [];

    #endregion
}