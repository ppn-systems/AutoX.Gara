// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Repairs;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Shared.Frames;

namespace AutoX.Gara.Shared.Protocol.Repairs;

[SerializePackable(SerializeLayout.Explicit)]
public sealed class RepairTaskDto : PacketBase<RepairTaskDto>, IPacketTransformer<RepairTaskDto>, IPacketSequenced
{
    [SerializeOrder(PacketHeaderOffset.DATA_REGION)]
    public System.UInt32 SequenceId { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32? RepairTaskId { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public System.Int32 RepairOrderId { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public System.Int32 EmployeeId { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public System.Int32 ServiceItemId { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 5)]
    public RepairOrderStatus Status { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 6)]
    public System.DateTime? StartDate { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 7)]
    public System.Double EstimatedDuration { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 8)]
    public System.DateTime? CompletionDate { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 9)]
    public System.Boolean IsCompleted { get; set; }

    public RepairTaskDto()
    {
        Status = RepairOrderStatus.Pending;
        EstimatedDuration = 1.0;
        IsCompleted = false;
        OpCode = OpCommand.NONE.AsUInt16();
    }

    public override void ResetForPool()
    {
        base.ResetForPool();

        SequenceId = 0;
        RepairTaskId = null;
        RepairOrderId = 0;
        EmployeeId = 0;
        ServiceItemId = 0;
        Status = RepairOrderStatus.Pending;
        StartDate = null;
        EstimatedDuration = 1.0;
        CompletionDate = null;
        IsCompleted = false;
        OpCode = OpCommand.NONE.AsUInt16();
    }

    public static RepairTaskDto Compress(RepairTaskDto packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        return packet;
    }

    public static RepairTaskDto Decompress(RepairTaskDto packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        return packet;
    }
}
