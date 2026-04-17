using System;
using AutoX.Gara.Domain.Abstractions;
using AutoX.Gara.Domain.Entities.Inventory;
using AutoX.Gara.Domain.Entities.Invoices;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoX.Gara.Domain.Entities.Repairs;

/// <summary>
/// Bang trung gian giua RepairOrder va Part.
/// </summary>
[Table(nameof(RepairOrderItem))]
public class RepairOrderItem : AuditEntity<int>
{
    #region Foreign Key Properties

    /// <summary>
    /// Khoa ngoai toi Part.
    /// </summary>
    public int PartId { get; set; }

    /// <summary>
    /// Khoa ngoai toi RepairOrder.
    /// </summary>
    public int RepairOrderId { get; set; }

    /// <summary>
    /// Thong tin phu tung lien quan (Navigation Property).
    /// </summary>
    [ForeignKey(nameof(PartId))]
    public virtual Part SparePart { get; set; } = null!;

    /// <summary>
    /// Thong tin don sua chua lien quan (Navigation Property).
    /// </summary>
    [ForeignKey(nameof(RepairOrderId))]
    public virtual RepairOrder RepairOrder { get; set; } = null!;

    #endregion

    #region Additional Properties

    /// <summary>
    /// So luong phu tung su dung trong don sua chua.
    /// </summary>
    public int Quantity { get; set; }

    #endregion
}
