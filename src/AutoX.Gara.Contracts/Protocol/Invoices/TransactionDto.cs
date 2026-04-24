// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Domain.Enums.Transactions;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using System;
namespace AutoX.Gara.Contracts.Protocol.Invoices;
[SerializePackable(SerializeLayout.Explicit)]
public sealed class TransactionDto : PacketBase<TransactionDto>
{
    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public int? TransactionId { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public int InvoiceId { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public TransactionType Type { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public PaymentMethod PaymentMethod { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public TransactionStatus Status { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public decimal Amount { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public DateTime TransactionDate { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 8)]
    public int CreatedBy { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 9)]
    public int? ModifiedBy { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 10)]
    public DateTime? UpdatedAt { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 11)]
    public bool IsReversed { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 12)]
    public string Description { get; set; }
    public TransactionDto()
    {
        Description = string.Empty;
        TransactionDate = DateTime.UtcNow;
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
        TransactionDate = DateTime.UtcNow;
        CreatedBy = 0;
        ModifiedBy = null;
        UpdatedAt = null;
        IsReversed = false;
        Description = string.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}

