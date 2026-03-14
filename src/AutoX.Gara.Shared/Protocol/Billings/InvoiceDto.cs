// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Shared.Frames;

namespace AutoX.Gara.Shared.Protocol.Billings;

/// <summary>
/// Packet mang du lieu hoa don (Invoice), dung cho create/update va query response.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class InvoiceDto : PacketBase<InvoiceDto>, IPacketTransformer<InvoiceDto>, IPacketSequenced
{
    // Fixed-size fields

    [SerializeOrder(PacketHeaderOffset.DATA_REGION)]
    public System.UInt32 SequenceId { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32? InvoiceId { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public System.Int32 CustomerId { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public System.DateTime InvoiceDate { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public PaymentStatus PaymentStatus { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 5)]
    public TaxRateType TaxRate { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 6)]
    public DiscountType DiscountType { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 7)]
    public System.Decimal Discount { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 8)]
    public System.Decimal Subtotal { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 9)]
    public System.Decimal DiscountAmount { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 10)]
    public System.Decimal TaxAmount { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 11)]
    public System.Decimal TotalAmount { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 12)]
    public System.Decimal BalanceDue { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 13)]
    public System.Decimal ServiceSubtotal { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 14)]
    public System.Decimal PartsSubtotal { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 15)]
    public System.Boolean IsFullyPaid { get; set; }

    /// <summary>
    /// Optional: RepairOrderId to link this invoice with a specific repair order when creating/updating.
    /// 0 means no link request.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 16)]
    public System.Int32 RepairOrderId { get; set; }

    // Dynamic-size fields (string) - must be last

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 17)]
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

    public static InvoiceDto Compress(InvoiceDto packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        return packet;
    }

    public static InvoiceDto Decompress(InvoiceDto packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        return packet;
    }
}
