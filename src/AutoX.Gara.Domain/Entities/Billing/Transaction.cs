// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Domain.Enums.Transactions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoX.Gara.Domain.Entities.Billing;

/// <summary>
/// Đại diện cho một giao dịch tài chính, bao gồm các thông tin về số tiền, phương thức thanh toán và trạng thái.
/// </summary>
[Table(nameof(Transaction))]
public class Transaction
{
    #region Fields

    // Hiện tại không có private fields, để lại region này cho tính nhất quán.

    #endregion

    #region Identification Properties

    /// <summary>
    /// Mã giao dịch duy nhất trong hệ thống.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public System.Int32 Id { get; set; }

    /// <summary>
    /// Mã hóa đơn liên quan đến giao dịch.
    /// </summary>
    public System.Int32 InvoiceId { get; set; }

    [ForeignKey(nameof(InvoiceId))]
    public virtual Invoice Invoice { get; set; } = null!;

    #endregion

    #region Transaction Details Properties

    /// <summary>
    /// Loại giao dịch
    /// - <see cref="TransactionType.Revenue"/>: Giao dịch thu tiền
    /// - <see cref="TransactionType.Expense"/>: Giao dịch chi tiền
    /// - <see cref="TransactionType.DebtPayment"/>: Giao dịch trả nợ
    /// - <see cref="TransactionType.RepairCost"/>: Chi phí sửa chữa.
    /// </summary>
    public TransactionType Type { get; set; }

    /// <summary>
    /// Phương thức thanh toán của giao dịch
    /// - Ví dụ: Tiền mặt, chuyển khoản, thẻ tín dụng, ví điện tử.
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.None;

    /// <summary>
    /// Trạng thái của giao dịch.
    /// - <see cref="TransactionStatus.Pending"/>: Đang chờ xử lý
    /// - <see cref="TransactionStatus.Completed"/>: Đã hoàn thành
    /// - <see cref="TransactionStatus.Failed"/>: Thất bại.
    /// </summary>
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

    /// <summary>
    /// Số tiền liên quan đến giao dịch.
    /// - Giá trị phải lớn hơn 0.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 999999999.99, ErrorMessage = "Transaction amount must be between 0.01 and 999,999,999.99.")]
    public System.Decimal Amount { get; set; }

    /// <summary>
    /// Mô tả chi tiết về giao dịch (tùy chọn)
    /// - Không được vượt quá 255 ký tự.
    /// </summary>
    [StringLength(255, ErrorMessage = "Description must not exceed 255 characters.")]
    public System.String Description { get; set; } = System.String.Empty;

    /// <summary>
    /// Ngày thực hiện giao dịch
    /// - Mặc định là thời điểm tạo giao dịch.
    /// </summary>
    [Column(TypeName = "TEXT")]
    public System.DateTime TransactionDate { get; set; } = System.DateTime.UtcNow;

    #endregion

    #region Audit Properties

    /// <summary>
    /// Người đã tạo giao dịch trong hệ thống
    /// </summary>
    public System.Int32 CreatedBy { get; set; }

    /// <summary>
    /// Người gần nhất chỉnh sửa giao dịch
    /// </summary>
    public System.Int32? ModifiedBy { get; set; }

    /// <summary>
    /// Ngày chỉnh sửa gần nhất.
    /// </summary>
    [Column(TypeName = "TEXT")]
    public System.DateTime? UpdatedAt { get; set; }

    #endregion

    #region Status Properties

    /// <summary>
    /// Đánh dấu nếu giao dịch đã bị hoàn tiền hoặc đảo ngược.
    /// </summary>
    public System.Boolean IsReversed { get; set; } = false;

    #endregion
}