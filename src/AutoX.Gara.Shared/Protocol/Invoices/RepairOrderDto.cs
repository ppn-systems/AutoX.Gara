// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Repairs;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;

namespace AutoX.Gara.Shared.Protocol.Invoices;

[SerializePackable(SerializeLayout.Explicit)]
public sealed class RepairOrderDto : PacketBase<RepairOrderDto>
{

    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public System.Int32? RepairOrderId { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public System.Int32 CustomerId { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public System.Int32? VehicleId { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public System.Int32? InvoiceId { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public System.DateTime OrderDate { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public System.DateTime? CompletionDate { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public RepairOrderStatus Status { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 8)]
    public System.Decimal TotalRepairCost { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 9)]
    public System.Boolean IsCompleted { get; set; }

    public RepairOrderDto()
    {
        OrderDate = System.DateTime.UtcNow;
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
        OrderDate = System.DateTime.UtcNow;
        CompletionDate = null;
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
