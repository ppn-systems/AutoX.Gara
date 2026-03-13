// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Suppliers;
using AutoX.Gara.Domain.Enums.Parts;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoX.Gara.Domain.Entities.Inventory;

/// <summary>
/// Represents a spare or replacement part in the inventory.
/// </summary>
[Table(nameof(Part))]
public class Part
{
    #region Identification Properties

    /// <summary>
    /// Unique identifier for the part.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public System.Int32 Id { get; set; }

    /// <summary>
    /// Supplier identifier for the part.
    /// </summary>
    public System.Int32 SupplierId { get; set; }

    /// <summary>
    /// Navigation property to the supplier entity.
    /// </summary>
    [ForeignKey(nameof(SupplierId))]
    public virtual Supplier Supplier { get; set; }

    /// <summary>
    /// Code or SKU (Stock Keeping Unit) of the part.
    /// </summary>
    [Required]
    [StringLength(12, ErrorMessage = "Part code must not exceed 12 characters.")]
    [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "Part code must contain only letters and numbers.")]
    public System.String PartCode { get; set; } = System.String.Empty;

    /// <summary>
    /// Name of the part.
    /// </summary>
    [Required]
    [StringLength(100, ErrorMessage = "Part name must not exceed 100 characters.")]
    public System.String PartName { get; set; } = System.String.Empty;

    /// <summary>
    /// Manufacturer or brand of the part.
    /// </summary>
    [Required]
    [StringLength(75, ErrorMessage = "Manufacturer must not exceed 75 characters.")]
    public System.String Manufacturer { get; set; } = System.String.Empty;

    #endregion

    #region Categorization

    /// <summary>
    /// The category of the part.
    /// </summary>
    public PartCategory PartCategory { get; set; }

    #endregion

    #region Quantity and Price

    /// <summary>
    /// Inventory quantity of the part.
    /// </summary>
    [Range(0, System.Int32.MaxValue, ErrorMessage = "Inventory quantity must be non-negative.")]
    public System.Int32 InventoryQuantity { get; set; }

    /// <summary>
    /// Purchase price of the part.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, System.Double.MaxValue, ErrorMessage = "Purchase price must be greater than 0.")]
    public System.Decimal PurchasePrice { get; set; }

    /// <summary>
    /// Selling price of the part (must be greater than or equal to purchase price).
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
                throw new InvalidOperationException("Selling price cannot be lower than purchase price.");
            }
            _sellingPrice = value;
        }
    }
    private System.Decimal _sellingPrice;

    /// <summary>
    /// Total value of the part in stock. (InventoryQuantity * PurchasePrice)
    /// </summary>
    [NotMapped]
    public System.Decimal TotalValue => InventoryQuantity * PurchasePrice;

    #endregion

    #region Status and Dates

    /// <summary>
    /// Indicates whether the part is currently in stock.
    /// </summary>
    [NotMapped]
    public System.Boolean IsInStock => InventoryQuantity > 0;

    /// <summary>
    /// Indicates whether the part is defective.
    /// </summary>
    public System.Boolean IsDefective { get; set; } = false;

    /// <summary>
    /// Marks the part as defective.
    /// </summary>
    public void MarkAsDefective() => IsDefective = true;

    /// <summary>
    /// Cancels the defective status.
    /// </summary>
    public void UnmarkAsDefective() => IsDefective = false;

    /// <summary>
    /// Indicates whether the part is discontinued.
    /// </summary>
    public System.Boolean IsDiscontinued { get; set; } = false;

    /// <summary>
    /// The date when the part was added to the inventory.
    /// </summary>
    public DateOnly DateAdded { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// The expiry date of the part, if applicable.
    /// </summary>
    public DateOnly? ExpiryDate
    {
        get => _expiryDate;
        set
        {
            if (value is not null && value < DateAdded)
            {
                throw new ArgumentException("Expiry date cannot be earlier than date added.");
            }
            _expiryDate = value;
        }
    }
    private DateOnly? _expiryDate;

    #endregion

    #region Quantity Change Methods

    /// <summary>
    /// Increase inventory quantity by the specified amount.
    /// </summary>
    /// <param name="amount">Amount to increase.</param>
    /// <exception cref="ArgumentException">If amount is not positive.</exception>
    public void IncreaseQuantity(System.Int32 amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Increase amount must be positive.");
        }

        InventoryQuantity += amount;
    }

    /// <summary>
    /// Decrease inventory quantity by the specified amount.
    /// </summary>
    /// <param name="amount">Amount to decrease.</param>
    /// <exception cref="ArgumentException">If amount is not positive or insufficient stock.</exception>
    public void DecreaseQuantity(System.Int32 amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Decrease amount must be positive.");
        }

        if (InventoryQuantity < amount)
        {
            throw new ArgumentException("Not enough stock.");
        }

        InventoryQuantity -= amount;
    }

    #endregion
}