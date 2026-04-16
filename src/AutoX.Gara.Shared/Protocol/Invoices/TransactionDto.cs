// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Domain.Enums.Transactions;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;

namespace AutoX.Gara.Shared.Protocol.Invoices;

[SerializePackable(SerializeLayout.Explicit)]
public sealed class TransactionDto : PacketBase<TransactionDto>
{

    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public System.Int32? TransactionId { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public System.Int32 InvoiceId { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public TransactionType Type { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public PaymentMethod PaymentMethod { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public TransactionStatus Status { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public System.Decimal Amount { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public System.DateTime TransactionDate { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 8)]
    public System.Int32 CreatedBy { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 9)]
    public System.Int32? ModifiedBy { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 10)]
    public System.DateTime? UpdatedAt { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 11)]
    public System.Boolean IsReversed { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 12)]
    public System.String Description { get; set; }

    public TransactionDto()
    {
        Description = System.String.Empty;
        TransactionDate = System.DateTime.UtcNow;
        Type = TransactionType.Revenue;
        PaymentMethod = PaymentMethod.None;
        Status = TransactionStatus.Pending;
        OpCode = OpCommand.NONE.AsUInt16();
    }

    public override void ResetForPool()
    {
        base.ResetForPool();

        SequenceId = 0;
        TransactionId = null;
        InvoiceId = 0;
        Type = TransactionType.Revenue;
        PaymentMethod = PaymentMethod.None;
        Status = TransactionStatus.Pending;
        Amount = 0;
        TransactionDate = System.DateTime.UtcNow;
        CreatedBy = 0;
        ModifiedBy = null;
        UpdatedAt = null;
        IsReversed = false;
        Description = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}

