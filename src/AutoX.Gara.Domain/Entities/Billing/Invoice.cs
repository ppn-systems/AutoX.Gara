// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Domain.Entities.Repairs;
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Domain.Enums.Transactions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AutoX.Gara.Domain.Entities.Billing;

/// <summary>
/// Lớp đại diện cho hóa đơn gara ô tô.
/// </summary>
[Table(nameof(Invoice))]
public class Invoice
{
    #region Fields

    private System.Decimal _discount;
    private System.String _invoiceNumber = System.String.Empty;

    #endregion

    #region Identification Properties

    /// <summary>
    /// Mã hóa đơn.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public System.Int32 Id { get; set; }

    /// <summary>
    /// Id chủ xe.
    /// </summary>
    public System.Int32 CustomerId { get; set; }

    /// <summary>
    /// Thông tin người tạo hóa đơn (Id nhân viên).
    /// </summary>
    public System.Int32 CreatedById { get; set; }

    /// <summary>
    /// Thay đổi bởi (Id nhân viên).
    /// </summary>
    public System.Int32? ModifiedById { get; set; }

    /// <summary>
    /// Số hóa đơn (mã duy nhất).
    /// </summary>
    [Required(ErrorMessage = "Invoice number is required.")]
    [MaxLength(30, ErrorMessage = "Invoice number must not exceed 30 characters.")]
    public System.String InvoiceNumber
    {
        get => _invoiceNumber;
        set => _invoiceNumber = value?.Trim() ?? System.String.Empty;
    }

    #endregion

    #region Audit Properties

    /// <summary>
    /// Thông tin chủ xe (Navigation Property).
    /// </summary>
    [ForeignKey(nameof(CustomerId))]
    public virtual Customer Customer { get; set; }

    /// <summary>
    /// Người tạo hóa đơn.
    /// </summary>
    [ForeignKey(nameof(CreatedById))]
    public Employee CreatedBy { get; set; } = null!;

    /// <summary>
    /// Người chỉnh sửa hóa đơn.
    /// </summary>
    [ForeignKey(nameof(ModifiedById))]
    public Employee ModifiedBy { get; set; }

    /// <summary>
    /// Ngày lập hóa đơn.
    /// </summary>
    public System.DateTime InvoiceDate { get; set; } = System.DateTime.UtcNow;

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
    [Range(0, System.Double.MaxValue, ErrorMessage = "Discount must be a positive value.")]
    public System.Decimal Discount
    {
        get => _discount;
        set
        {
            if (DiscountType == DiscountType.Percentage && (value < 0 || value > 100))
            {
                throw new System.ArgumentException("Discount percentage must be between 0 and 100.");
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

    /// <summary>
    /// Tính tổng tiền trước thuế và giảm giá.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public System.Decimal Subtotal { get; private set; }

    /// <summary>
    /// Số tiền giảm giá thực tế.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public System.Decimal DiscountAmount { get; private set; }

    /// <summary>
    /// Số tiền thuế thực tế.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public System.Decimal TaxAmount { get; private set; }

    /// <summary>
    /// Tổng số tiền cần thanh toán sau thuế và giảm giá.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public System.Decimal TotalAmount { get; private set; }

    /// <summary>
    /// Số tiền còn nợ.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public System.Decimal BalanceDue { get; private set; }

    /// <summary>
    /// Tiền dịch vụ
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public System.Decimal ServiceSubtotal { get; private set; }

    /// <summary>
    /// Tiền phụ tùng
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public System.Decimal PartsSubtotal { get; private set; }

    /// <summary>
    /// Hóa đơn đã thanh toán đủ chưa?
    /// </summary>
    public System.Boolean IsFullyPaid => BalanceDue <= 0;

    #endregion

    #region Methods

    /// <summary>
    /// Số tiền khách đã thanh toán.
    /// </summary>
    public System.Decimal AmountPaid() =>
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
            .Sum(o => o.RepairTaskList?.Sum(task => task.ServiceItem.UnitPrice) ?? 0) ?? 0;

        // Tiền phụ tùng
        PartsSubtotal = RepairOrders?
            .Sum(o => o.RepairOrderItems?
                .Sum(sp => sp.SparePart.SellingPrice * sp.Quantity) ?? 0
            ) ?? 0;

        // Subtotal = tổng chi phí sửa chữa (dịch vụ + phụ tùng) của tất cả RepairOrder
        Subtotal = ServiceSubtotal + PartsSubtotal;

        // Tính giảm giá
        DiscountAmount = DiscountType == DiscountType.Percentage
            ? Subtotal * Discount / 100
            : Discount;

        // Tính thuế
        TaxAmount = (Subtotal - DiscountAmount) * ((System.Decimal)TaxRate / 100);

        // Tổng sau thuế và giảm giá
        TotalAmount = Subtotal - DiscountAmount + TaxAmount;

        // Còn nợ = Tổng - đã trả
        BalanceDue = TotalAmount - AmountPaid();
    }

    #endregion
}