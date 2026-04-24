// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Repairs;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Extensions;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using System;
namespace AutoX.Gara.Contracts.Invoices;
[SerializePackable(SerializeLayout.Explicit)]
public sealed class RepairOrderQueryRequest : PacketBase<RepairOrderQueryRequest>
{
    [SerializeOrder(0)]
    public int Page { get; set; } = 1;
    [SerializeOrder(1)]
    public int PageSize { get; set; } = 20;
    [SerializeOrder(2)]
    public RepairOrderSortField SortBy { get; set; } = RepairOrderSortField.OrderDate;
    [SerializeOrder(3)]
    public bool SortDescending { get; set; } = true;
    [SerializeOrder(4)]
    public int FilterCustomerId { get; set; } = 0;
    [SerializeOrder(5)]
    public int FilterVehicleId { get; set; } = 0;
    [SerializeOrder(6)]
    public int FilterInvoiceId { get; set; } = 0;
    [SerializeOrder(7)]
    public RepairOrderStatus? FilterStatus { get; set; } = null;
    [SerializeOrder(8)]
    public DateTime? FilterFromDate { get; set; } = null;
    [SerializeOrder(9)]
    public DateTime? FilterToDate { get; set; } = null;
    [SerializeOrder(10)]
    public string SearchTerm { get; set; } = string.Empty;
    public RepairOrderQueryRequest() => OpCode = OpCommand.NONE.AsUInt16();
    public override void ResetForPool()
    {
        base.ResetForPool();
        SequenceId = 0;
        Page = 1;
        PageSize = 20;
        SortBy = RepairOrderSortField.OrderDate;
        SortDescending = true;
        FilterCustomerId = 0;
        FilterVehicleId = 0;
        FilterInvoiceId = 0;
        FilterStatus = null;
        FilterFromDate = null;
        FilterToDate = null;
        SearchTerm = string.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}



