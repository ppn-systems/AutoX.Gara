// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Repairs;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;

namespace AutoX.Gara.Shared.Protocol.Repairs;

[SerializePackable(SerializeLayout.Explicit)]
public sealed class RepairTaskDto : PacketBase<RepairTaskDto>
{

    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public System.Int32? RepairTaskId { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public System.Int32 RepairOrderId { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public System.Int32 EmployeeId { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public System.Int32 ServiceItemId { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public RepairOrderStatus Status { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public System.DateTime? StartDate { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public System.Double EstimatedDuration { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 8)]
    public System.DateTime? CompletionDate { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 9)]
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
