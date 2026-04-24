// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Domain.Enums.Transactions;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Extensions;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using System;
namespace AutoX.Gara.Contracts.Invoices;
[SerializePackable(SerializeLayout.Explicit)]
public sealed class TransactionDto : PacketBase<TransactionDto>
{
    [SerializeOrder(0)]
    public int? TransactionId { get; set; }
    [SerializeOrder(1)]
    public int InvoiceId { get; set; }
    [SerializeOrder(2)]
    public TransactionType Type { get; set; }
    [SerializeOrder(3)]
    public PaymentMethod PaymentMethod { get; set; }
    [SerializeOrder(4)]
    public TransactionStatus Status { get; set; }
    [SerializeOrder(5)]
    public decimal Amount { get; set; }
    [SerializeOrder(6)]
    public DateTime TransactionDate { get; set; }
    [SerializeOrder(7)]
    public int CreatedBy { get; set; }
    [SerializeOrder(8)]
    public int? ModifiedBy { get; set; }
    [SerializeOrder(9)]
    public DateTime? UpdatedAt { get; set; }
    [SerializeOrder(10)]
    public bool IsReversed { get; set; }
    [SerializeOrder(11)]
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



