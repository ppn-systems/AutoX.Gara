// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Caching;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Shared.Frames;

namespace AutoX.Gara.Shared.Protocol.Repairs;

[SerializePackable(SerializeLayout.Explicit)]
public sealed class RepairOrderItemQueryRequest : PacketBase<RepairOrderItemQueryRequest>, IPoolable, IPacketSequenced
{
    [SerializeOrder(PacketHeaderOffset.DATA_REGION)]
    public System.UInt32 SequenceId { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32 Page { get; set; } = 1;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public System.Int32 PageSize { get; set; } = 20;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public RepairOrderItemSortField SortBy { get; set; } = RepairOrderItemSortField.Id;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public System.Boolean SortDescending { get; set; } = true;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 5)]
    public System.Int32 FilterRepairOrderId { get; set; } = 0;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 6)]
    public System.Int32 FilterPartId { get; set; } = 0;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 7)]
    public System.String SearchTerm { get; set; } = System.String.Empty;

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
        SearchTerm = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}
