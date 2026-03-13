// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoX.Gara.Domain.Entities.Billings;

/// <summary>
/// Lớp đại diện cho một dịch vụ trong hóa đơn.
/// </summary>
[Table(nameof(ServiceItem))]
public class ServiceItem
{
    #region Fields

    // Hiện tại không có private fields, để lại region này cho tính nhất quán.

    #endregion

    #region Identification Properties

    /// <summary>
    /// Mã dịch vụ (Unique Identifier).
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public System.Int32 Id { get; set; }

    #endregion

    #region Service Details Properties

    /// <summary>
    /// Mô tả của dịch vụ.
    /// </summary>
    [Required(ErrorMessage = "Description is required.")]
    [MaxLength(255, ErrorMessage = "Description must not exceed 255 characters.")]
    public System.String Description { get; set; } = System.String.Empty;

    /// <summary>
    /// Loại dịch vụ (Sửa chữa, Bảo dưỡng,...).
    /// </summary>
    public ServiceType Type { get; set; } = ServiceType.None;

    /// <summary>
    /// Đơn giá của dịch vụ.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 9999999.99, ErrorMessage = "Unit price must be between 0.01 and 9,999,999.99.")]
    public System.Decimal UnitPrice { get; set; }

    #endregion
}