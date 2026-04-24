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
public sealed class TransactionQueryRequest : PacketBase<TransactionQueryRequest>
{
    [SerializeOrder(0)]
    public int Page { get; set; } = 1;
    [SerializeOrder(1)]
    public int PageSize { get; set; } = 20;
    [SerializeOrder(2)]
    public TransactionSortField SortBy { get; set; } = TransactionSortField.TransactionDate;
    [SerializeOrder(3)]
    public bool SortDescending { get; set; } = true;
    [SerializeOrder(4)]
    public int FilterInvoiceId { get; set; } = 0;
    [SerializeOrder(5)]
    public TransactionType? FilterType { get; set; } = null;
    [SerializeOrder(6)]
    public TransactionStatus? FilterStatus { get; set; } = null;
    [SerializeOrder(7)]
    public PaymentMethod? FilterPaymentMethod { get; set; } = null;
    [SerializeOrder(8)]
    public decimal? FilterMinAmount { get; set; } = null;
    [SerializeOrder(9)]
    public decimal? FilterMaxAmount { get; set; } = null;
    [SerializeOrder(10)]
    public DateTime? FilterFromDate { get; set; } = null;
    [SerializeOrder(11)]
    public DateTime? FilterToDate { get; set; } = null;
    [SerializeOrder(12)]
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



