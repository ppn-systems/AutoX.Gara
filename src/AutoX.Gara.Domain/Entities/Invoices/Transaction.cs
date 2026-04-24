using AutoX.Gara.Domain.Abstractions;
using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Domain.Enums.Transactions;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AutoX.Gara.Domain.Entities.Invoices;
/// <summary>
/// Dai dien cho mot giao dich tai chinh, bao gom cac thong tin ve so tien, phuong thuc thanh toan va trang thai.
/// </summary>
[Table(nameof(Transaction))]
public class Transaction : AuditEntity<int>
{
    #region Identification Properties
    /// <summary>
    /// Ma hoa don lien quan den giao dich.
    /// </summary>
    public int InvoiceId { get; set; }
    [ForeignKey(nameof(InvoiceId))]
    public virtual Invoice Invoice { get; set; } = null!;
    #endregion
    #region Transaction Details Properties
    /// <summary>
    /// Loai giao dich
    /// </summary>
    public TransactionType Type { get; set; }
    /// <summary>
    /// Phuong thuc thanh toan cua giao dich
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.None;
    /// <summary>
    /// Trang thai cua giao dich.
    /// </summary>
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    /// <summary>
    /// So tien lien quan den giao dich.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 999999999.99, ErrorMessage = "Transaction amount must be between 0.01 and 999,999,999.99.")]
    public decimal Amount { get; set; }
    /// <summary>
    /// Mo ta chi tiet ve giao dich (tuy chon)
    /// </summary>
    [StringLength(255, ErrorMessage = "Description must not exceed 255 characters.")]
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// Ngay thuc hien giao dich
    /// </summary>
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    #endregion
    #region Status Properties
    /// <summary>
    /// Danh dau neu giao dich da bi hoan tien hoac dao nguoc.
    /// </summary>
    public bool IsReversed { get; set; } = false;
    #endregion
}
