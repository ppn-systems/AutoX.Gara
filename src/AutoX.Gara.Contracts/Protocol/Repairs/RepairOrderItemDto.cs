using AutoX.Gara.Contracts.Enums;
// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Contracts.Extensions;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
namespace AutoX.Gara.Contracts.Repairs;
[SerializePackable(SerializeLayout.Explicit)]
public sealed class RepairOrderItemDto : PacketBase<RepairOrderItemDto>
{
    [SerializeOrder(0)]
    public int? RepairOrderItemId { get; set; }
    [SerializeOrder(1)]
    public int RepairOrderId { get; set; }
    [SerializeOrder(2)]
    public int PartId { get; set; }
    [SerializeOrder(3)]
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



