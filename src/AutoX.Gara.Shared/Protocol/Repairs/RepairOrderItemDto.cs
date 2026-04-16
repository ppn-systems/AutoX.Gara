using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Common.Networking.Protocols;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;

namespace AutoX.Gara.Shared.Protocol.Repairs;

[SerializePackable(SerializeLayout.Explicit)]
public sealed class RepairOrderItemDto : PacketBase<RepairOrderItemDto>
{

    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public int? RepairOrderItemId { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public int RepairOrderId { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public int PartId { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public int Quantity { get; set; }

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