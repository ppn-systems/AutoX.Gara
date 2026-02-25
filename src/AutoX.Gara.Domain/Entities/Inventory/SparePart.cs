// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Parts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoX.Gara.Domain.Entities.Inventory;

/// <summary>
/// Lớp đại diện cho phụ tùng.
/// </summary>
[Table(nameof(SparePart))]
public class SparePart
{
    #region Fields

    private System.Decimal _sellingPrice;
    private System.Int32 _inventoryQuantity;

    #endregion

    #region Identification Properties

    /// <summary>
    /// Mã phụ tùng.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public System.Int32 Id { get; set; }

    /// <summary>
    /// Id nhà cung cấp của phụ tùng.
    /// </summary>
    public System.Int32 SupplierId { get; set; }

    /// <summary>
    /// Thông tin nhà cung cấp (Navigation Property).
    /// </summary>
    [ForeignKey(nameof(SupplierId))]
    public virtual Supplier Supplier { get; set; }

    #endregion

    #region Basic Properties

    /// <summary>
    /// Loại phụ tùng.
    /// </summary>
    public PartCategory PartCategory { get; set; }

    /// <summary>
    /// Tên phụ tùng.
    /// </summary>
    [Required, StringLength(100, ErrorMessage = "Part name must not exceed 100 characters.")]
    public System.String PartName { get; set; } = System.String.Empty;

    #endregion

    #region Price and Quantity Properties

    /// <summary>
    /// Giá nhập phụ tùng.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, System.Double.MaxValue, ErrorMessage = "Purchase price must be greater than 0.")]
    public System.Decimal PurchasePrice { get; set; }

    /// <summary>
    /// Giá bán của phụ tùng (luôn lớn hơn hoặc bằng giá nhập).
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, System.Double.MaxValue, ErrorMessage = "Selling price must be greater than 0.")]
    public System.Decimal SellingPrice
    {
        get => _sellingPrice;
        set
        {
            if (value < PurchasePrice)
            {
                throw new System.InvalidOperationException("Selling price cannot be lower than purchase price.");
            }

            _sellingPrice = value;
        }
    }

    /// <summary>
    /// Số lượng tồn kho của phụ tùng.
    /// </summary>
    [Range(0, System.Int32.MaxValue, ErrorMessage = "Inventory quantity must be a non-negative integer.")]
    public System.Int32 InventoryQuantity
    {
        get => _inventoryQuantity;
        set
        {
            if (value < 0)
            {
                throw new System.ArgumentException("Inventory quantity cannot be negative.");
            }

            _inventoryQuantity = value;
        }
    }

    #endregion

    #region Status Properties

    /// <summary>
    /// Đánh dấu phụ tùng không còn bán.
    /// </summary>
    [Required]
    public System.Boolean IsDiscontinued { get; set; } = false;

    #endregion
}