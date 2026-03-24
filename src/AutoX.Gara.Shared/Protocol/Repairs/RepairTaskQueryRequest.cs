// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Repairs;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Shared.Frames;

namespace AutoX.Gara.Shared.Protocol.Repairs;

[SerializePackable(SerializeLayout.Explicit)]
public sealed class RepairTaskQueryRequest : PacketBase<RepairTaskQueryRequest>
{

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32 Page { get; set; } = 1;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public System.Int32 PageSize { get; set; } = 20;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public RepairTaskSortField SortBy { get; set; } = RepairTaskSortField.Id;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public System.Boolean SortDescending { get; set; } = true;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 5)]
    public System.Int32 FilterRepairOrderId { get; set; } = 0;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 6)]
    public System.Int32 FilterEmployeeId { get; set; } = 0;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 7)]
    public System.Int32 FilterServiceItemId { get; set; } = 0;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 8)]
    public RepairOrderStatus? FilterStatus { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 9)]
    public System.DateTime? FilterFromDate { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 10)]
    public System.DateTime? FilterToDate { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 11)]
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
