// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Shared.Frames;

namespace AutoX.Gara.Shared.Protocol.Billings;

[SerializePackable(SerializeLayout.Explicit)]
public sealed class RepairOrderItemDto : PacketBase<RepairOrderItemDto>, IPacketTransformer<RepairOrderItemDto>, IPacketSequenced
{
    [SerializeOrder(PacketHeaderOffset.DATA_REGION)]
    public System.UInt32 SequenceId { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32? RepairOrderItemId { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public System.Int32 RepairOrderId { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public System.Int32 PartId { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public System.Int32 Quantity { get; set; }

    public RepairOrderItemDto() => OpCode = OpCommand.NONE.AsUInt16();

    public override void ResetForPool()
    {
        base.ResetForPool();

        SequenceId = 0;
        RepairOrderItemId = null;
        RepairOrderId = 0;
        PartId = 0;
        Quantity = 0;
        OpCode = OpCommand.NONE.AsUInt16();
    }

    public static RepairOrderItemDto Compress(RepairOrderItemDto packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        return packet;
    }

    public static RepairOrderItemDto Decompress(RepairOrderItemDto packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        return packet;
    }
}
