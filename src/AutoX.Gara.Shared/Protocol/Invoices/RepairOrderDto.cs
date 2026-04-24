// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Repairs;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using System;
namespace AutoX.Gara.Shared.Protocol.Invoices;
[SerializePackable(SerializeLayout.Explicit)]
public sealed class RepairOrderDto : PacketBase<RepairOrderDto>
{
    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public int? RepairOrderId { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public int CustomerId { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public int? VehicleId { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public int? InvoiceId { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public DateTime OrderDate { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public DateTime? CompletionDate { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 10)]
    public DateTime? ExpectedCompletionDate { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 11)]
    public RepairOrderPriority OrderPriority { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 12)]
    public int? EmployeeId { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 13)]
    public string Description { get; set; } = string.Empty;
    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public RepairOrderStatus Status { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 8)]
    public decimal TotalRepairCost { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 9)]
    public bool IsCompleted { get; set; }
    public RepairOrderDto()
    {
        OrderDate = DateTime.UtcNow;
        Status = RepairOrderStatus.None;
        OpCode = OpCommand.NONE.AsUInt16();
    }
    public override void ResetForPool()
    {
        base.ResetForPool();
        SequenceId = 0;
        RepairOrderId = null;
        CustomerId = 0;
        VehicleId = null;
        InvoiceId = null;
        OrderDate = DateTime.UtcNow;
        CompletionDate = null;
        ExpectedCompletionDate = null;
        OrderPriority = RepairOrderPriority.Normal;
        EmployeeId = null;
        Description = string.Empty;
        Status = RepairOrderStatus.None;
        TotalRepairCost = 0;
        IsCompleted = false;
        OpCode = OpCommand.NONE.AsUInt16();
    }
    public static RepairOrderDto Compress(RepairOrderDto packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        return packet;
    }
    public static RepairOrderDto Decompress(RepairOrderDto packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        return packet;
    }
}
