using System;
using AutoX.Gara.Domain.Abstractions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoX.Gara.Domain.Entities.Suppliers;

/// <summary>
/// Lop dai dien cho so dien thoai cua nha cung cap.
/// </summary>
[Table(nameof(SupplierContactPhone))]
public class SupplierContactPhone : AuditEntity<int>
{
    #region Identification Properties

    /// <summary>
    /// Ma nha cung cap lien ket voi so dien thoai nay.
    /// </summary>
    [Required]
    public int SupplierId { get; set; }

    /// <summary>
    /// Thong tin nha cung cap lien quan (Navigation Property).
    /// </summary>
    [ForeignKey(nameof(SupplierId))]
    public virtual Supplier Supplier { get; set; } = null!;

    #endregion

    #region Contact Properties

    /// <summary>
    /// So dien thoai cua nha cung cap (10-12 chu so).
    /// </summary>
    [Required]
    [MaxLength(12)]
    [RegularExpression(@"^\d{10,12}$", ErrorMessage = "The phone number must be between 10-12 digits.")]
    public string PhoneNumber { get; set; } = string.Empty;

    #endregion
}
