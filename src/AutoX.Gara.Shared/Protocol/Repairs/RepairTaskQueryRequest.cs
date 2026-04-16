// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Repairs;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;

namespace AutoX.Gara.Shared.Protocol.Repairs;

[SerializePackable(SerializeLayout.Explicit)]
public sealed class RepairTaskQueryRequest : PacketBase<RepairTaskQueryRequest>
{

    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public System.Int32 Page { get; set; } = 1;

    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public System.Int32 PageSize { get; set; } = 20;

    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public RepairTaskSortField SortBy { get; set; } = RepairTaskSortField.Id;

    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public System.Boolean SortDescending { get; set; } = true;

    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public System.Int32 FilterRepairOrderId { get; set; } = 0;

    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public System.Int32 FilterEmployeeId { get; set; } = 0;

    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public System.Int32 FilterServiceItemId { get; set; } = 0;

    [SerializeOrder(PacketHeaderOffset.Region + 8)]
    public RepairOrderStatus? FilterStatus { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.Region + 9)]
    public System.DateTime? FilterFromDate { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.Region + 10)]
    public System.DateTime? FilterToDate { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.Region + 11)]
    public System.String SearchTerm { get; set; } = System.String.Empty;

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
        SearchTerm = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}
