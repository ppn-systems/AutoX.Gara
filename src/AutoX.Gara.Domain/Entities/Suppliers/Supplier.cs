// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Inventory;
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoX.Gara.Domain.Entities.Suppliers;

/// <summary>
/// Lớp đại diện cho nhà cung cấp.
/// </summary>
[Table(nameof(Supplier))]
public class Supplier
{
    #region Fields

    private System.DateTime? _contractEndDate;

    #endregion

    #region Identification Properties

    /// <summary>
    /// Mã nhà cung cấp (Unique identifier).
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public System.Int32 Id { get; set; }

    /// <summary>
    /// Tên nhà cung cấp.
    /// </summary>
    [Required(ErrorMessage = "Supplier name is required.")]
    [MaxLength(100, ErrorMessage = "Supplier name must not exceed 100 characters.")]
    public System.String Name { get; set; } = System.String.Empty;

    #endregion

    #region Contact Information Properties

    /// <summary>
    /// Email của nhà cung cấp.
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [MaxLength(100, ErrorMessage = "Email must not exceed 100 characters.")]
    public System.String Email { get; set; } = System.String.Empty;

    /// <summary>
    /// Địa chỉ của nhà cung cấp.
    /// </summary>
    [MaxLength(255, ErrorMessage = "Address must not exceed 255 characters.")]
    public System.String Address { get; set; } = System.String.Empty;

    /// <summary>
    /// Danh sách số điện thoại của nhà cung cấp (Quan hệ 1-N với `SupplierPhone`).
    /// </summary>
    public virtual ICollection<SupplierContactPhone> PhoneNumbers { get; set; } = [];

    #endregion

    #region Contract Details Properties

    /// <summary>
    /// Ngày bắt đầu hợp tác với nhà cung cấp.
    /// </summary>
    public System.DateTime ContractStartDate { get; set; } = System.DateTime.UtcNow;

    /// <summary>
    /// Ngày kết thúc hợp tác (nếu có).
    /// </summary>
    public System.DateTime? ContractEndDate
    {
        get => _contractEndDate;
        set
        {
            if (value.HasValue && value < ContractStartDate)
            {
                throw new System.ArgumentException("Contract end date cannot be earlier than start date.");
            }

            _contractEndDate = value;
        }
    }

    /// <summary>
    /// Ghi chú về nhà cung cấp.
    /// </summary>
    public System.String Notes { get; set; } = System.String.Empty;

    #endregion

    #region Financial Information Properties

    /// <summary>
    /// Tài khoản ngân hàng để thanh toán.
    /// </summary>
    [MaxLength(20, ErrorMessage = "Bank account must not exceed 50 characters.")]
    public System.String BankAccount { get; set; } = System.String.Empty;

    /// <summary>
    /// Mã số thuế của nhà cung cấp.
    /// </summary>
    [MaxLength(13, ErrorMessage = "Tax code must not exceed 20 characters.")]
    public System.String TaxCode { get; set; } = System.String.Empty;

    /// <summary>
    /// Điều khoản thanh toán.
    /// </summary>
    public PaymentTerms PaymentTerms { get; set; } = PaymentTerms.None;

    #endregion

    #region Status and Relationships Properties

    /// <summary>
    /// Trạng thái của nhà cung cấp (Hoạt động, Ngừng hợp tác,...).
    /// </summary>
    public SupplierStatus Status { get; set; } = SupplierStatus.Active;

    /// <summary>
    /// Những loại phụ tùng cung cấp.
    /// </summary>
    public virtual ICollection<Part> Parts { get; set; } = [];

    #endregion
}