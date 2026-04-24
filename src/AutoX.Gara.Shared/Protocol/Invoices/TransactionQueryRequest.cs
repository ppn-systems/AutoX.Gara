// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Domain.Enums.Transactions;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using System;

namespace AutoX.Gara.Shared.Protocol.Invoices;

[SerializePackable(SerializeLayout.Explicit)]
public sealed class TransactionQueryRequest : PacketBase<TransactionQueryRequest>
{

    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public int Page { get; set; } = 1;

    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public int PageSize { get; set; } = 20;

    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public TransactionSortField SortBy { get; set; } = TransactionSortField.TransactionDate;

    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public bool SortDescending { get; set; } = true;

    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public int FilterInvoiceId { get; set; } = 0;

    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public TransactionType? FilterType { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public TransactionStatus? FilterStatus { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.Region + 8)]
    public PaymentMethod? FilterPaymentMethod { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.Region + 9)]
    public decimal? FilterMinAmount { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.Region + 10)]
    public decimal? FilterMaxAmount { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.Region + 11)]
    public DateTime? FilterFromDate { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.Region + 12)]
    public DateTime? FilterToDate { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.Region + 13)]
    public string SearchTerm { get; set; } = string.Empty;

    public TransactionQueryRequest() => OpCode = OpCommand.NONE.AsUInt16();

    public override void ResetForPool()
    {
        base.ResetForPool();

        SequenceId = 0;
        Page = 1;
        PageSize = 20;
        SortBy = TransactionSortField.TransactionDate;
        SortDescending = true;
        FilterInvoiceId = 0;
        FilterType = null;
        FilterStatus = null;
        FilterPaymentMethod = null;
        FilterMinAmount = null;
        FilterMaxAmount = null;
        FilterFromDate = null;
        FilterToDate = null;
        SearchTerm = string.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}
