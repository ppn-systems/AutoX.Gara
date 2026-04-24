// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Repairs;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Extensions;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using System;
namespace AutoX.Gara.Contracts.Invoices;
[SerializePackable(SerializeLayout.Explicit)]
public sealed class RepairOrderDto : PacketBase<RepairOrderDto>
{
    [SerializeOrder(0)]
    public int? RepairOrderId { get; set; }
    [SerializeOrder(1)]
    public int CustomerId { get; set; }
    [SerializeOrder(2)]
    public int? VehicleId { get; set; }
    [SerializeOrder(3)]
    public int? InvoiceId { get; set; }
    [SerializeOrder(4)]
    public DateTime OrderDate { get; set; }
    [SerializeOrder(5)]
    public DateTime? CompletionDate { get; set; }
    [SerializeOrder(9)]
    public DateTime? ExpectedCompletionDate { get; set; }
    [SerializeOrder(10)]
    public RepairOrderPriority OrderPriority { get; set; }
    [SerializeOrder(11)]
    public int? EmployeeId { get; set; }
    [SerializeOrder(12)]
    public string Description { get; set; } = string.Empty;
    [SerializeOrder(6)]
    public RepairOrderStatus Status { get; set; }
    [SerializeOrder(7)]
    public decimal TotalRepairCost { get; set; }
    [SerializeOrder(8)]
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



