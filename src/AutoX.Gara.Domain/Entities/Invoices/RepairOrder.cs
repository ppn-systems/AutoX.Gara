// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Domain.Entities.Repairs;
using AutoX.Gara.Domain.Enums.Repairs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AutoX.Gara.Domain.Entities.Invoices;

/// <summary>
/// Lớp đại diện cho đơn sửa chữa.
/// </summary>
[Table(nameof(RepairOrder))]
public class RepairOrder
{
    #region Identification Properties

    /// <summary>
    /// Mã đơn sửa chữa.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public System.Int32 Id { get; set; }

    /// <summary>
    /// Id chủ xe.
    /// </summary>
    public System.Int32 CustomerId { get; set; }

    /// <summary>
    /// Mã xe liên quan đến đơn sửa chữa.
    /// </summary>
    public System.Int32? VehicleId { get; set; }

    /// <summary>
    /// Hóa đơn liên quan đến đơn sửa chữa (nếu đã có).
    /// </summary>
    public System.Int32? InvoiceId { get; set; }

    /// <summary>
    /// Thông tin hóa đơn liên quan (Navigation Property).
    /// </summary>
    [ForeignKey(nameof(InvoiceId))]
    public virtual Invoice Invoice { get; set; } = null!;

    #endregion

    #region Order Details Properties

    /// <summary>
    /// Ngày tạo lệnh sửa chữa.
    /// </summary>
    [Required]
    public System.DateTime OrderDate { get; set; } = System.DateTime.UtcNow;

    /// <summary>
    /// Ngày hoàn thành lệnh sửa chữa.
    /// </summary>
    public System.DateTime? CompletionDate { get; set; }

    /// <summary>
    /// Trạng thái của lệnh sửa chữa.
    /// </summary>
    [Required]
    public RepairOrderStatus Status { get; set; } = RepairOrderStatus.None;

    /// <summary>
    /// Danh sách công việc sửa chữa liên quan.
    /// </summary>
    public virtual ICollection<RepairTask> Tasks { get; set; } = [];

    /// <summary>
    /// Danh sách phụ tùng thay thế liên quan.
    /// </summary>
    public virtual ICollection<RepairOrderItem> Parts { get; set; } = [];

    /// <summary>
    /// Tổng chi phí sửa chữa.
    /// </summary>
    [NotMapped]
    [Column(TypeName = "decimal(18,2)")]
    public System.Decimal TotalRepairCost =>
        Tasks.Sum(t => t.ServiceItem.UnitPrice) +
        Parts.Sum(p => p.SparePart.SellingPrice * p.Quantity);

    /// <summary>
    /// Xác định xem lệnh sửa chữa đã hoàn thành hay chưa.
    /// </summary>
    [NotMapped]
    public System.Boolean IsCompleted => Status == RepairOrderStatus.Completed;

    #endregion
}
