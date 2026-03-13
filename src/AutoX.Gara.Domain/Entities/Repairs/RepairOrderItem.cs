// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Inventory;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoX.Gara.Domain.Entities.Repairs;

/// <summary>
/// Bảng trung gian giữa RepairOrder và SparePart.
/// </summary>
[Table(nameof(RepairOrderItem))]
public class RepairOrderItem
{
    #region Fields

    // Hiện tại không có private fields, để lại region này cho tính nhất quán.

    #endregion

    #region Foreign Key Properties

    /// <summary>
    /// Primary key property
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public System.Int32 Id { get; set; }

    /// <summary>
    /// Khóa ngoại tới SparePart.
    /// </summary>
    public System.Int32 PartId { get; set; }

    /// <summary>
    /// Khóa ngoại tới RepairOrder.
    /// </summary>
    public System.Int32 RepairOrderId { get; set; }

    /// <summary>
    /// Thông tin phụ tùng liên quan (Navigation Property).
    /// </summary>
    [ForeignKey(nameof(PartId))]
    public Part SparePart { get; set; } = null!;

    /// <summary>
    /// Thông tin đơn sửa chữa liên quan (Navigation Property).
    /// </summary>
    [ForeignKey(nameof(RepairOrderId))]
    public RepairOrder RepairOrder { get; set; } = null!;

    #endregion

    #region Additional Properties

    /// <summary>
    /// Số lượng phụ tùng sử dụng trong đơn sửa chữa.
    /// </summary>
    public System.Int32 Quantity { get; set; }

    #endregion
}