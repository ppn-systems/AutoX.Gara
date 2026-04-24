// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Repairs;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Extensions;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using System;
namespace AutoX.Gara.Contracts.Repairs;
[SerializePackable(SerializeLayout.Explicit)]
public sealed class RepairTaskDto : PacketBase<RepairTaskDto>
{
    [SerializeOrder(0)]
    public int? RepairTaskId { get; set; }
    [SerializeOrder(1)]
    public int RepairOrderId { get; set; }
    [SerializeOrder(2)]
    public int EmployeeId { get; set; }
    [SerializeOrder(3)]
    public int ServiceItemId { get; set; }
    [SerializeOrder(4)]
    public RepairOrderStatus Status { get; set; }
    [SerializeOrder(5)]
    public DateTime? StartDate { get; set; }
    [SerializeOrder(6)]
    public double EstimatedDuration { get; set; }
    [SerializeOrder(7)]
    public DateTime? CompletionDate { get; set; }
    [SerializeOrder(8)]
    public bool IsCompleted { get; set; }
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



