// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Extensions;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using System;
namespace AutoX.Gara.Contracts.Billings;
/// <summary>
/// Packet gui tu client len server de truy van danh sach hoa don (Invoice)
/// co ho tro phan trang, tim kiem, loc va sap xep.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class InvoiceQueryRequest : PacketBase<InvoiceQueryRequest>
{
    [SerializeOrder(0)]
    public int Page { get; set; } = 1;
    [SerializeOrder(1)]
    public int PageSize { get; set; } = 20;
    [SerializeOrder(2)]
    public InvoiceSortField SortBy { get; set; } = InvoiceSortField.InvoiceDate;
    [SerializeOrder(3)]
    public bool SortDescending { get; set; } = true;
    /// <summary>
    /// Loc theo customer id. 0 = khong loc.
    /// </summary>
    [SerializeOrder(4)]
    public int FilterCustomerId { get; set; } = 0;
    /// <summary>
    /// Loc theo trang thai thanh toan. null = khong loc.
    /// </summary>
    [SerializeOrder(5)]
    public PaymentStatus? FilterPaymentStatus { get; set; } = null;
    /// <summary>
    /// Loc theo khoang thoi gian (InvoiceDate). null = khong loc.
    /// </summary>
    [SerializeOrder(6)]
    public DateTime? FilterFromDate { get; set; } = null;
    [SerializeOrder(7)]
    public DateTime? FilterToDate { get; set; } = null;
    /// <summary>
    /// Tim kiem theo so hoa don. Dynamic field - dat cuoi.
    /// </summary>
    [SerializeOrder(8)]
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



