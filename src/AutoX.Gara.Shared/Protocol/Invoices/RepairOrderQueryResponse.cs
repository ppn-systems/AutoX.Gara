using AutoX.Gara.Shared.Enums;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using Nalix.Framework.Injection;
using Nalix.Framework.Memory.Objects;
using System.Collections.Generic;


namespace AutoX.Gara.Shared.Protocol.Invoices;

[SerializePackable(SerializeLayout.Explicit)]
public sealed class RepairOrderQueryResponse : PacketBase<RepairOrderQueryResponse>
{
    [SerializeIgnore]
    public override int Length
    {
        get
        {
            int total = PacketConstants.HeaderSize
                + sizeof(System.UInt32) // SequenceId
                + sizeof(int)  // TotalCount
                + sizeof(int); // list item-count prefix

            for (int i = 0; i < RepairOrders.Count; i++)
            {
                total += RepairOrders[i].Length;
            }

            return total;
        }
    }


    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public int TotalCount { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public List<RepairOrderDto> RepairOrders { get; set; } = [];

    public RepairOrderQueryResponse() => OpCode = OpCommand.NONE.AsUInt16();

    public override void ResetForPool()
    {
        if (RepairOrders?.Count > 0)
        {
            var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
            for (int i = 0; i < RepairOrders.Count; i++)
            {
                if (RepairOrders[i] is not null)
                {
                    pool.Return(RepairOrders[i]);
                }
            }
        }

        RepairOrders.Clear();
        SequenceId = 0;
        TotalCount = 0;
        OpCode = OpCommand.NONE.AsUInt16();
        base.ResetForPool();
    }
}
