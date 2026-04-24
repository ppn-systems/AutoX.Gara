using AutoX.Gara.Domain.Abstractions;
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
public class RepairOrder : AuditEntity<int>
{
    #region Identification Properties
    /// <summary>
    /// Id chủ xe.
    /// </summary>
    public int CustomerId { get; set; }
    /// <summary>
    /// Mã xe liên quan đến đơn sửa chữa.
    /// </summary>
    public int? VehicleId { get; set; }
    /// <summary>
    /// Hóa đơn liên quan đến đơn sửa chữa (nếu đã có).
    /// </summary>
    public int? InvoiceId { get; set; }
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
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Ngày hoàn thành lệnh sửa chữa.
    /// </summary>
    public DateTime? ExpectedCompletionDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public RepairOrderPriority Priority { get; set; } = RepairOrderPriority.Normal;
    public string Description { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
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
    public decimal TotalRepairCost =>
        Tasks.Sum(t => t.ServiceItem?.UnitPrice ?? 0) +
        Parts.Sum(p => (p.SparePart?.SellingPrice ?? 0) * p.Quantity);
    /// <summary>
    /// Xác định xem lệnh sửa chữa đã hoàn thành hay chưa.
    /// </summary>
    [NotMapped]
    public bool IsCompleted => Status == RepairOrderStatus.Completed;
    #endregion
}
