// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Repairs;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Extensions;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using System;
namespace AutoX.Gara.Contracts.Repairs;
[SerializePackable(SerializeLayout.Explicit)]
public sealed class RepairTaskQueryRequest : PacketBase<RepairTaskQueryRequest>
{
    [SerializeOrder(0)]
    public int Page { get; set; } = 1;
    [SerializeOrder(1)]
    public int PageSize { get; set; } = 20;
    [SerializeOrder(2)]
    public RepairTaskSortField SortBy { get; set; } = RepairTaskSortField.Id;
    [SerializeOrder(3)]
    public bool SortDescending { get; set; } = true;
    [SerializeOrder(4)]
    public int FilterRepairOrderId { get; set; } = 0;
    [SerializeOrder(5)]
    public int FilterEmployeeId { get; set; } = 0;
    [SerializeOrder(6)]
    public int FilterServiceItemId { get; set; } = 0;
    [SerializeOrder(7)]
    public RepairOrderStatus? FilterStatus { get; set; } = null;
    [SerializeOrder(8)]
    public DateTime? FilterFromDate { get; set; } = null;
    [SerializeOrder(9)]
    public DateTime? FilterToDate { get; set; } = null;
    [SerializeOrder(10)]
    public string SearchTerm { get; set; } = string.Empty;
    public RepairTaskQueryRequest() => OpCode = OpCommand.NONE.AsUInt16();
    public override void ResetForPool()
    {
        base.ResetForPool();
        SequenceId = 0;
        Page = 1;
        PageSize = 20;
        SortBy = RepairTaskSortField.Id;
        SortDescending = true;
        FilterRepairOrderId = 0;
        FilterEmployeeId = 0;
        FilterServiceItemId = 0;
        FilterStatus = null;
        FilterFromDate = null;
        FilterToDate = null;
        SearchTerm = string.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}



