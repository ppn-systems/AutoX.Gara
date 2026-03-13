// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Domain.Enums.Transactions;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Caching;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Shared.Frames;

namespace AutoX.Gara.Shared.Protocol.Billings;

[SerializePackable(SerializeLayout.Explicit)]
public sealed class TransactionQueryRequest : PacketBase<TransactionQueryRequest>, IPoolable, IPacketSequenced
{
    [SerializeOrder(PacketHeaderOffset.DATA_REGION)]
    public System.UInt32 SequenceId { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32 Page { get; set; } = 1;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public System.Int32 PageSize { get; set; } = 20;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public TransactionSortField SortBy { get; set; } = TransactionSortField.TransactionDate;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public System.Boolean SortDescending { get; set; } = true;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 5)]
    public System.Int32 FilterInvoiceId { get; set; } = 0;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 6)]
    public TransactionType? FilterType { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 7)]
    public TransactionStatus? FilterStatus { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 8)]
    public PaymentMethod? FilterPaymentMethod { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 9)]
    public System.Decimal? FilterMinAmount { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 10)]
    public System.Decimal? FilterMaxAmount { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 11)]
    public System.DateTime? FilterFromDate { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 12)]
    public System.DateTime? FilterToDate { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 13)]
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

