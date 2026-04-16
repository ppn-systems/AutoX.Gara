using AutoX.Gara.Domain.Abstractions;
using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Domain.Entities.Invoices;
using AutoX.Gara.Domain.Enums.Repairs;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoX.Gara.Domain.Entities.Repairs;

/// <summary>
/// Lop dai dien cho cong viec sua chua.
/// </summary>
[Table(nameof(RepairTask))]
public class RepairTask : AuditEntity<int>
{
    #region Fields

    private DateTime? _startDate;
    private DateTime? _completionDate;

    #endregion

    #region Identification Properties

    /// <summary>
    /// Nhan vien thuc hien cong viec sua chua.
    /// </summary>
    public int EmployeeId { get; set; }

    /// <summary>
    /// Cac dich vu su dung.
    /// </summary>
    public int ServiceItemId { get; set; }

    /// <summary>
    /// Id don sua chua lien quan.
    /// </summary>
    public int RepairOrderId { get; set; }

    /// <summary>
    /// Thong tin nhan vien lien quan (Navigation Property).
    /// </summary>
    [ForeignKey(nameof(EmployeeId))]
    public virtual Employee Employee { get; set; } = null!;

    /// <summary>
    /// Thong tin dich vu lien quan (Navigation Property).
    /// </summary>
    [ForeignKey(nameof(ServiceItemId))]
    public virtual ServiceItem ServiceItem { get; set; } = null!;

    /// <summary>
    /// Thong tin don sua chua lien quan (Navigation Property).
    /// </summary>
    [ForeignKey(nameof(RepairOrderId))]
    public virtual RepairOrder RepairOrder { get; set; } = null!;

    #endregion

    #region Task Details Properties

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Trang thai cong viec sua chua.
    /// </summary>
    public RepairOrderStatus Status { get; set; } = RepairOrderStatus.Pending;

    /// <summary>
    /// Ngay bat dau cong viec.
    /// </summary>
    public DateTime? StartDate
    {
        get => _startDate;
        set
        {
            if (value.HasValue && value > DateTime.UtcNow)
            {
                throw new ArgumentException("Start date cannot be in the future.");
            }

            _startDate = value;
        }
    }

    /// <summary>
    /// Thoi gian uoc tinh de hoan thanh cong viec (tinh bang gio).
    /// </summary>
    [Range(0, 1000, ErrorMessage = "Estimated duration must be between 0 and 1000 hours.")]
    public double EstimatedDuration { get; set; } = 1.0;

    /// <summary>
    /// Ngay hoan thanh cong viec sua chua (neu da xong).
    /// </summary>
    public DateTime? CompletionDate
    {
        get => _completionDate;
        set
        {
            if (value.HasValue && StartDate.HasValue && value < StartDate)
            {
                throw new ArgumentException("Completion date cannot be earlier than start date.");
            }

            _completionDate = value;
        }
    }

    /// <summary>
    /// Cong viec da hoan thanh chua.
    /// </summary>
    public bool IsCompleted => Status == RepairOrderStatus.Completed;

    #endregion
}