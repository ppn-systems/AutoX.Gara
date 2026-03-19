// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Shared.Frames;

namespace AutoX.Gara.Shared.Protocol.Billings;

[SerializePackable(SerializeLayout.Explicit)]
public sealed class ServiceItemDto : PacketBase<ServiceItemDto>
{

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32? ServiceItemId { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public ServiceType Type { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public System.Decimal UnitPrice { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public System.String Description { get; set; }

    public ServiceItemDto()
    {
        Description = System.String.Empty;
        Type = ServiceType.None;
        UnitPrice = 0;
        OpCode = OpCommand.NONE.AsUInt16();
    }

    public override void ResetForPool()
    {
        base.ResetForPool();

        SequenceId = 0;
        ServiceItemId = null;
        Type = ServiceType.None;
        UnitPrice = 0;
        Description = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}
