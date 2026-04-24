// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using System;
namespace AutoX.Gara.Shared.Protocol.Billings;
/// <summary>
/// Packet gui tu client len server de truy van danh sach hoa don (Invoice)
/// co ho tro phan trang, tim kiem, loc va sap xep.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class InvoiceQueryRequest : PacketBase<InvoiceQueryRequest>
{
    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public int Page { get; set; } = 1;
    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public int PageSize { get; set; } = 20;
    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public InvoiceSortField SortBy { get; set; } = InvoiceSortField.InvoiceDate;
    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public bool SortDescending { get; set; } = true;
    /// <summary>
    /// Loc theo customer id. 0 = khong loc.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public int FilterCustomerId { get; set; } = 0;
    /// <summary>
    /// Loc theo trang thai thanh toan. null = khong loc.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public PaymentStatus? FilterPaymentStatus { get; set; } = null;
    /// <summary>
    /// Loc theo khoang thoi gian (InvoiceDate). null = khong loc.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public DateTime? FilterFromDate { get; set; } = null;
    [SerializeOrder(PacketHeaderOffset.Region + 8)]
    public DateTime? FilterToDate { get; set; } = null;
    /// <summary>
    /// Tim kiem theo so hoa don. Dynamic field - dat cuoi.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 9)]
    public string SearchTerm { get; set; } = string.Empty;
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
        SearchTerm = string.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}
