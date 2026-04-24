// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Extensions;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using System;
namespace AutoX.Gara.Contracts.Billings;
/// <summary>
/// Packet mang du lieu hoa don (Invoice), dung cho create/update va query response.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class InvoiceDto : PacketBase<InvoiceDto>
{
    // Fixed-size fields
    [SerializeOrder(0)]
    public int? InvoiceId { get; set; }
    [SerializeOrder(1)]
    public int CustomerId { get; set; }
    [SerializeOrder(2)]
    public DateTime InvoiceDate { get; set; }
    [SerializeOrder(3)]
    public PaymentStatus PaymentStatus { get; set; }
    [SerializeOrder(4)]
    public TaxRateType TaxRate { get; set; }
    [SerializeOrder(5)]
    public DiscountType DiscountType { get; set; }
    [SerializeOrder(6)]
    public decimal Discount { get; set; }
    [SerializeOrder(7)]
    public decimal Subtotal { get; set; }
    [SerializeOrder(8)]
    public decimal DiscountAmount { get; set; }
    [SerializeOrder(9)]
    public decimal TaxAmount { get; set; }
    [SerializeOrder(10)]
    public decimal TotalAmount { get; set; }
    [SerializeOrder(11)]
    public decimal BalanceDue { get; set; }
    [SerializeOrder(12)]
    public decimal ServiceSubtotal { get; set; }
    [SerializeOrder(13)]
    public decimal PartsSubtotal { get; set; }
    [SerializeOrder(14)]
    public bool IsFullyPaid { get; set; }
    /// <summary>
    /// Optional: RepairOrderId to link this invoice with a specific repair order when creating/updating.
    /// 0 means no link request.
    /// </summary>
    [SerializeOrder(15)]
    public int RepairOrderId { get; set; }
    // Dynamic-size fields (string) - must be last
    [SerializeOrder(16)]
    public string InvoiceNumber { get; set; }
    [SerializeOrder(17)]
    public string Notes { get; set; }
    public InvoiceDto()
    {
        InvoiceNumber = string.Empty;
        Notes = string.Empty;
        InvoiceDate = DateTime.UtcNow;
        PaymentStatus = PaymentStatus.Unpaid;
        TaxRate = TaxRateType.VAT10;
        DiscountType = DiscountType.None;
        RepairOrderId = 0;
        OpCode = OpCommand.NONE.AsUInt16();
    }
    public override void ResetForPool()
    {
        base.ResetForPool();
        SequenceId = 0;
        InvoiceId = null;
        CustomerId = 0;
        InvoiceDate = DateTime.UtcNow;
        PaymentStatus = PaymentStatus.Unpaid;
        TaxRate = TaxRateType.VAT10;
        DiscountType = DiscountType.None;
        Discount = 0;
        Subtotal = 0;
        DiscountAmount = 0;
        TaxAmount = 0;
        TotalAmount = 0;
        BalanceDue = 0;
        ServiceSubtotal = 0;
        PartsSubtotal = 0;
        IsFullyPaid = false;
        RepairOrderId = 0;
        InvoiceNumber = string.Empty;
        Notes = string.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}



