using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using Nalix.Common.Networking.Protocols;
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
    public int? InvoiceId { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public int CustomerId { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public DateTime InvoiceDate { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public PaymentStatus PaymentStatus { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public TaxRateType TaxRate { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public DiscountType DiscountType { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public decimal Discount { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 8)]
    public decimal Subtotal { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 9)]
    public decimal DiscountAmount { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 10)]
    public decimal TaxAmount { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 11)]
    public decimal TotalAmount { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 12)]
    public decimal BalanceDue { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 13)]
    public decimal ServiceSubtotal { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 14)]
    public decimal PartsSubtotal { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 15)]
    public bool IsFullyPaid { get; set; }

    /// <summary>
    /// Optional: RepairOrderId to link this invoice with a specific repair order when creating/updating.
    /// 0 means no link request.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 16)]
    public int RepairOrderId { get; set; }

    // Dynamic-size fields (string) - must be last

    [SerializeOrder(PacketHeaderOffset.Region + 17)]
    public string InvoiceNumber { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 18)]
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
