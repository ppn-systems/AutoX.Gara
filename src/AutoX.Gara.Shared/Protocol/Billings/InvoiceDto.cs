// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;

namespace AutoX.Gara.Shared.Protocol.Billings;

/// <summary>
/// Packet mang du lieu hoa don (Invoice), dung cho create/update va query response.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class InvoiceDto : PacketBase<InvoiceDto>
{
    // Fixed-size fields

    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public System.Int32? InvoiceId { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public System.Int32 CustomerId { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public System.DateTime InvoiceDate { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public PaymentStatus PaymentStatus { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public TaxRateType TaxRate { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public DiscountType DiscountType { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public System.Decimal Discount { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 8)]
    public System.Decimal Subtotal { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 9)]
    public System.Decimal DiscountAmount { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 10)]
    public System.Decimal TaxAmount { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 11)]
    public System.Decimal TotalAmount { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 12)]
    public System.Decimal BalanceDue { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 13)]
    public System.Decimal ServiceSubtotal { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 14)]
    public System.Decimal PartsSubtotal { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 15)]
    public System.Boolean IsFullyPaid { get; set; }

    /// <summary>
    /// Optional: RepairOrderId to link this invoice with a specific repair order when creating/updating.
    /// 0 means no link request.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 16)]
    public System.Int32 RepairOrderId { get; set; }

    // Dynamic-size fields (string) - must be last

    [SerializeOrder(PacketHeaderOffset.Region + 17)]
    public System.String InvoiceNumber { get; set; }

    public InvoiceDto()
    {
        InvoiceNumber = System.String.Empty;
        InvoiceDate = System.DateTime.UtcNow;
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
        InvoiceDate = System.DateTime.UtcNow;
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
        InvoiceNumber = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}
