using AutoX.Gara.Domain.Abstractions;
using AutoX.Gara.Domain.Entities.Invoices;
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Domain.Enums.Transactions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AutoX.Gara.Domain.Entities.Billings;

/// <summary>
/// Lớp đại diện cho hóa đơn gara ô tô.
/// </summary>
[Table(nameof(Invoice))]
public class Invoice : AuditEntity<int>
{
    #region Fields

    private decimal _discount;
    private string _invoiceNumber = string.Empty;

    #endregion

    #region Identification Properties

    /// <summary>
    /// Id chủ xe.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Số hóa đơn (mã duy nhất).
    /// </summary>
    [Required(ErrorMessage = "Invoice number is required.")]
    [MaxLength(30, ErrorMessage = "Invoice number must not exceed 30 characters.")]
    public string InvoiceNumber
    {
        get => _invoiceNumber;
        set => _invoiceNumber = value?.Trim() ?? string.Empty;
    }

    #endregion

    #region Payment Details Properties

    /// <summary>
    /// Trạng thái thanh toán của hóa đơn.
    /// </summary>
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

    /// <summary>
    /// Tỷ lệ thuế (5% hoặc 10%).
    /// </summary>
    public TaxRateType TaxRate { get; set; } = TaxRateType.VAT10;

    /// <summary>
    /// Loại giảm giá (phần trăm hoặc số tiền).
    /// </summary>
    public DiscountType DiscountType { get; set; } = DiscountType.None;

    /// <summary>
    /// Giá trị giảm giá.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Discount must be a positive value.")]
    public decimal Discount
    {
        get => _discount;
        set
        {
            if (DiscountType == DiscountType.Percentage && (value < 0 || value > 100))
            {
                throw new ArgumentException("Discount percentage must be between 0 and 100.");
            }

            _discount = value;
        }
    }

    #endregion

    #region Related Entities Properties

    /// <summary>
    /// Danh sách cho đơn sửa chữa.
    /// </summary>
    public virtual ICollection<RepairOrder> RepairOrders { get; set; } = [];

    /// <summary>
    /// Danh sách các giao dịch thanh toán.
    /// </summary>
    public virtual ICollection<Transaction> Transactions { get; set; } = [];

    #endregion

    #region Calculated Properties

    public DateTime InvoiceDate 
    { 
        get => CreatedAt; 
        set => CreatedAt = value; 
    }

    /// <summary>
    /// Tính tổng tiền trước thuế và giảm giá.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; private set; }

    /// <summary>
    /// Số tiền giảm giá thực tế.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; private set; }

    /// <summary>
    /// Số tiền thuế thực tế.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; private set; }

    /// <summary>
    /// Tổng số tiền cần thanh toán sau thuế và giảm giá.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// Số tiền còn nợ.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal BalanceDue { get; private set; }

    /// <summary>
    /// Tiền dịch vụ
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ServiceSubtotal { get; private set; }

    /// <summary>
    /// Tiền phụ tùng
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal PartsSubtotal { get; private set; }

    /// <summary>
    /// Ghi chú của hóa đơn.
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    public bool IsFullyPaid => BalanceDue <= 0;

    #endregion

    #region Methods

    /// <summary>
    /// Số tiền khách đã thanh toán.
    /// </summary>
    public decimal AmountPaid() =>
        Transactions?
            .Where(t => t.Type == TransactionType.Revenue && !t.IsReversed)
            .Sum(t => t.Amount) ?? 0;

    /// <summary>
    /// Tính toán lại các giá trị tài chính của hóa đơn.
    /// </summary>
    public void Recalculate()
    {
        // Tiền dịch vụ
        ServiceSubtotal = RepairOrders?
            .Sum(o => o.Tasks?.Sum(task => task.ServiceItem.UnitPrice) ?? 0) ?? 0;

        // Tiền phụ tùng
        PartsSubtotal = RepairOrders?
            .Sum(o => o.Parts?
                .Sum(sp => sp.SparePart.SellingPrice * sp.Quantity) ?? 0
            ) ?? 0;

        // Subtotal = tổng chi phí sửa chữa (dịch vụ + phụ tùng) của tất cả RepairOrder
        Subtotal = ServiceSubtotal + PartsSubtotal;

        // Tính giảm giá
        DiscountAmount = DiscountType == DiscountType.Percentage
            ? Subtotal * Discount / 100
            : Discount;

        // Tính thuế
        TaxAmount = (Subtotal - DiscountAmount) * ((decimal)TaxRate / 100);

        // Tổng sau thuế và giảm giá
        TotalAmount = Subtotal - DiscountAmount + TaxAmount;

        // Còn nợ = Tổng - đã trả
        BalanceDue = TotalAmount - AmountPaid();
    }

    #endregion
}