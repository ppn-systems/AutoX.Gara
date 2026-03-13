// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Caching;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Shared.Frames;

namespace AutoX.Gara.Shared.Protocol.Billings;

[SerializePackable(SerializeLayout.Explicit)]
public sealed class ServiceItemQueryRequest : PacketBase<ServiceItemQueryRequest>, IPoolable, IPacketSequenced
{
    [SerializeOrder(PacketHeaderOffset.DATA_REGION)]
    public System.UInt32 SequenceId { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32 Page { get; set; } = 1;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public System.Int32 PageSize { get; set; } = 20;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public ServiceItemSortField SortBy { get; set; } = ServiceItemSortField.Description;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public System.Boolean SortDescending { get; set; } = false;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 5)]
    public ServiceType? FilterType { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 6)]
    public System.Decimal? FilterMinUnitPrice { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 7)]
    public System.Decimal? FilterMaxUnitPrice { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 8)]
    public System.String SearchTerm { get; set; } = System.String.Empty;

    public ServiceItemQueryRequest() => OpCode = OpCommand.NONE.AsUInt16();

    public override void ResetForPool()
    {
        base.ResetForPool();

        SequenceId = 0;
        Page = 1;
        PageSize = 20;
        SortBy = ServiceItemSortField.Description;
        SortDescending = false;
        FilterType = null;
        FilterMinUnitPrice = null;
        FilterMaxUnitPrice = null;
        SearchTerm = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}
