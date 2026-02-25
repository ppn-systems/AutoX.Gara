// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Billing;
using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Domain.Enums.Repairs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AutoX.Gara.Domain.Entities.Repairs;

/// <summary>
/// Lớp đại diện cho đơn sửa chữa.
/// </summary>
[Table(nameof(RepairOrder))]
public class RepairOrder
{
    #region Fields

    // Hiện tại không có private fields, để lại region này cho tính nhất quán.

    #endregion

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
    /// Id hóa đơn.
    /// </summary>
    public System.Int32 InvoiceId { get; set; }

    /// <summary>
    /// Mã xe liên quan đến đơn sửa chữa.
    /// </summary>
    public System.Int32 VehicleId { get; set; }

    /// <summary>
    /// Thông tin chủ xe (Navigation Property).
    /// </summary>
    [ForeignKey(nameof(CustomerId))]
    public virtual Customer Customer { get; set; }

    /// <summary>
    /// Thông tin hóa đơn liên quan (Navigation Property).
    /// </summary>
    [ForeignKey(nameof(InvoiceId))]
    public virtual Invoice Invoice { get; set; }

    /// <summary>
    /// Thông tin xe liên quan (Navigation Property).
    /// </summary>
    [ForeignKey(nameof(VehicleId))]
    public virtual Vehicle Vehicle { get; set; }

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
    public virtual ICollection<RepairTask> RepairTaskList { get; set; } = [];

    /// <summary>
    /// Danh sách phụ tùng thay thế liên quan.
    /// </summary>
    public virtual ICollection<RepairOrderItem> RepairOrderItems { get; set; } = [];

    /// <summary>
    /// Tổng chi phí sửa chữa.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public System.Decimal TotalRepairCost =>
        (RepairTaskList?.Sum(task => task.ServiceItem.UnitPrice) ?? 0) +
        (RepairOrderItems?.Sum(sp => sp.SparePart.SellingPrice * sp.Quantity) ?? 0);

    /// <summary>
    /// Xác định xem lệnh sửa chữa đã hoàn thành hay chưa.
    /// </summary>
    [NotMapped]
    public System.Boolean IsCompleted => Status == RepairOrderStatus.Completed;

    #endregion
}