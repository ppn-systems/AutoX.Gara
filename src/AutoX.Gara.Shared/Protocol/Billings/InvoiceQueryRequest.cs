// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Common.Shared.Caching;
using Nalix.Shared.Frames;

namespace AutoX.Gara.Shared.Protocol.Billings;

/// <summary>
/// Packet gui tu client len server de truy van danh sach hoa don (Invoice)
/// co ho tro phan trang, tim kiem, loc va sap xep.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class InvoiceQueryRequest : PacketBase<InvoiceQueryRequest>, IPoolable
{
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32 Page { get; set; } = 1;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public System.Int32 PageSize { get; set; } = 20;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public InvoiceSortField SortBy { get; set; } = InvoiceSortField.InvoiceDate;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public System.Boolean SortDescending { get; set; } = true;

    /// <summary>
    /// Loc theo customer id. 0 = khong loc.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 5)]
    public System.Int32 FilterCustomerId { get; set; } = 0;

    /// <summary>
    /// Loc theo trang thai thanh toan. null = khong loc.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 6)]
    public PaymentStatus? FilterPaymentStatus { get; set; } = null;

    /// <summary>
    /// Loc theo khoang thoi gian (InvoiceDate). null = khong loc.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 7)]
    public System.DateTime? FilterFromDate { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 8)]
    public System.DateTime? FilterToDate { get; set; } = null;

    /// <summary>
    /// Tim kiem theo so hoa don. Dynamic field - dat cuoi.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 9)]
    public System.String SearchTerm { get; set; } = System.String.Empty;

    public InvoiceQueryRequest() => OpCode = OpCommand.NONE.AsUInt16();

    public override void ResetForPool()
    {
        base.ResetForPool();

        SequenceId = 0;
        Page = 1;
        PageSize = 20;
        SortBy = InvoiceSortField.InvoiceDate;
        SortDescending = true;
        FilterCustomerId = 0;
        FilterPaymentStatus = null;
        FilterFromDate = null;
        FilterToDate = null;
        SearchTerm = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}

