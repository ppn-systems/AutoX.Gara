using AutoX.Gara.Domain.Entities.Billing;
using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Domain.Enums.Repairs;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoX.Gara.Domain.Entities.Repairs;

/// <summary>
/// Lớp đại diện cho công việc sửa chữa.
/// </summary>
[Table(nameof(RepairTask))]
public class RepairTask
{
    #region Fields

    private DateTime? _startDate;
    private DateTime? _completionDate;

    #endregion

    #region Identification Properties

    /// <summary>
    /// Mã công việc sửa chữa.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Int32 Id { get; set; }

    /// <summary>
    /// Nhân viên thực hiện công việc sửa chữa.
    /// </summary>
    public Int32 EmployeeId { get; set; }

    /// <summary>
    /// Các dịch vụ sử dụng.
    /// </summary>
    public Int32 ServiceItemId { get; set; }

    /// <summary>
    /// Id đơn sửa chữa liên quan.
    /// </summary>
    public Int32 RepairOrderId { get; set; }

    /// <summary>
    /// Thông tin nhân viên thực hiện (Navigation Property).
    /// </summary>
    [ForeignKey(nameof(EmployeeId))]
    public virtual Employee Employee { get; set; }

    /// <summary>
    /// Thông tin dịch vụ liên quan (Navigation Property).
    /// </summary>
    [ForeignKey(nameof(ServiceItemId))]
    public virtual ServiceItem ServiceItem { get; set; }

    /// <summary>
    /// Thông tin đơn sửa chữa liên quan (Navigation Property).
    /// </summary>
    [ForeignKey(nameof(RepairOrderId))]
    public virtual RepairOrder RepairOrder { get; set; }

    #endregion

    #region Task Details Properties

    /// <summary>
    /// Trạng thái công việc sửa chữa.
    /// </summary>
    public RepairOrderStatus Status { get; set; } = RepairOrderStatus.Pending;

    /// <summary>
    /// Ngày bắt đầu công việc.
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
    /// Thời gian ước tính để hoàn thành công việc (tính bằng giờ).
    /// </summary>
    [Range(0, 1000, ErrorMessage = "Estimated duration must be between 0 and 1000 hours.")]
    public Double EstimatedDuration { get; set; } = 1.0;

    /// <summary>
    /// Ngày hoàn thành công việc sửa chữa (nếu đã xong).
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
    /// Công việc đã hoàn thành chưa.
    /// </summary>
    public Boolean IsCompleted => Status == RepairOrderStatus.Completed;

    #endregion
}