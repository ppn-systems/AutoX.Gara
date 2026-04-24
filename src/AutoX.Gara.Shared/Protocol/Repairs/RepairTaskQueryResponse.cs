using AutoX.Gara.Shared.Enums;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using Nalix.Framework.Injection;
using Nalix.Framework.Memory.Objects;
using System.Collections.Generic;


namespace AutoX.Gara.Shared.Protocol.Repairs;

[SerializePackable(SerializeLayout.Explicit)]
public sealed class RepairTaskQueryResponse : PacketBase<RepairTaskQueryResponse>
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

            for (int i = 0; i < RepairTasks.Count; i++)
            {
                total += RepairTasks[i].Length;
            }

            return total;
        }
    }

    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public int TotalCount { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public List<RepairTaskDto> RepairTasks { get; set; } = [];

    public RepairTaskQueryResponse() => OpCode = OpCommand.NONE.AsUInt16();

    public override void ResetForPool()
    {
        if (RepairTasks?.Count > 0)
        {
            var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
            for (int i = 0; i < RepairTasks.Count; i++)
            {
                if (RepairTasks[i] is not null)
                {
                    pool.Return(RepairTasks[i]);
                }
            }
        }

        RepairTasks.Clear();
        SequenceId = 0;
        TotalCount = 0;
        OpCode = OpCommand.NONE.AsUInt16();
        base.ResetForPool();
    }
}
