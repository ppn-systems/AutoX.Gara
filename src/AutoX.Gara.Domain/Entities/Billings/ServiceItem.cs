using System;
using AutoX.Gara.Domain.Abstractions;
using AutoX.Gara.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoX.Gara.Domain.Entities.Billings;

/// <summary>
/// Lop dai dien cho mot dich vu.
/// </summary>
[Table(nameof(ServiceItem))]
public class ServiceItem : AuditEntity<int>
{
    #region Service Details Properties

    /// <summary>
    /// Mo ta cua dich vu.
    /// </summary>
    [Required(ErrorMessage = "Description is required.")]
    [MaxLength(255, ErrorMessage = "Description must not exceed 255 characters.")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Loai dich vu (Sua chua, Bao duong,...).
    /// </summary>
    public ServiceType Type { get; set; } = ServiceType.None;

    /// <summary>
    /// Don gia cua dich vu.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 9999999.99, ErrorMessage = "Unit price must be between 0.01 and 9,999,999.99.")]
    public decimal UnitPrice { get; set; }

    #endregion
}