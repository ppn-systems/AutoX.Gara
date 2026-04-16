// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;

namespace AutoX.Gara.Shared.Protocol.Billings;

[SerializePackable(SerializeLayout.Explicit)]
public sealed class ServiceItemQueryRequest : PacketBase<ServiceItemQueryRequest>
{

    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public System.Int32 Page { get; set; } = 1;

    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public System.Int32 PageSize { get; set; } = 20;

    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public ServiceItemSortField SortBy { get; set; } = ServiceItemSortField.Description;

    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public System.Boolean SortDescending { get; set; } = false;

    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public ServiceType? FilterType { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public System.Decimal? FilterMinUnitPrice { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public System.Decimal? FilterMaxUnitPrice { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.Region + 8)]
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
