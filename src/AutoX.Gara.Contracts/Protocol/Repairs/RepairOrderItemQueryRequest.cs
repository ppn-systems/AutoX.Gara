using AutoX.Gara.Contracts.Enums;
// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Contracts.Extensions;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
namespace AutoX.Gara.Contracts.Repairs;
[SerializePackable(SerializeLayout.Explicit)]
public sealed class RepairOrderItemQueryRequest : PacketBase<RepairOrderItemQueryRequest>
{
    [SerializeOrder(0)]
    public int Page { get; set; } = 1;
    [SerializeOrder(1)]
    public int PageSize { get; set; } = 20;
    [SerializeOrder(2)]
    public RepairOrderItemSortField SortBy { get; set; } = RepairOrderItemSortField.Id;
    [SerializeOrder(3)]
    public bool SortDescending { get; set; } = true;
    [SerializeOrder(4)]
    public int FilterRepairOrderId { get; set; } = 0;
    [SerializeOrder(5)]
    public int FilterPartId { get; set; } = 0;
    [SerializeOrder(6)]
    public string SearchTerm { get; set; } = string.Empty;
    public RepairOrderItemQueryRequest() => OpCode = OpCommand.NONE.AsUInt16();
    public override void ResetForPool()
    {
        base.ResetForPool();
        SequenceId = 0;
        Page = 1;
        PageSize = 20;
        SortBy = RepairOrderItemSortField.Id;
        SortDescending = true;
        FilterRepairOrderId = 0;
        FilterPartId = 0;
        SearchTerm = string.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}



