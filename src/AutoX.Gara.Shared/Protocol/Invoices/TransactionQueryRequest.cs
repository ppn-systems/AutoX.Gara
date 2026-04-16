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
public sealed class TransactionQueryRequest : PacketBase<TransactionQueryRequest>
{

    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public System.Int32 Page { get; set; } = 1;

    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public System.Int32 PageSize { get; set; } = 20;

    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public TransactionSortField SortBy { get; set; } = TransactionSortField.TransactionDate;

    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public System.Boolean SortDescending { get; set; } = true;

    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public System.Int32 FilterInvoiceId { get; set; } = 0;

    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public TransactionType? FilterType { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public TransactionStatus? FilterStatus { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.Region + 8)]
    public PaymentMethod? FilterPaymentMethod { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.Region + 9)]
    public System.Decimal? FilterMinAmount { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.Region + 10)]
    public System.Decimal? FilterMaxAmount { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.Region + 11)]
    public System.DateTime? FilterFromDate { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.Region + 12)]
    public System.DateTime? FilterToDate { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.Region + 13)]
    public System.String SearchTerm { get; set; } = System.String.Empty;

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
        SearchTerm = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}

