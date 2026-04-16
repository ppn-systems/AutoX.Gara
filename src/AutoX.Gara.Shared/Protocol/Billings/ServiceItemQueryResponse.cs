using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Common.Networking.Protocols;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.Injection;
using Nalix.Framework.Extensions;
using Nalix.Framework.DataFrames;
using Nalix.Framework.Memory.Objects;
using System.Collections.Generic;

namespace AutoX.Gara.Shared.Protocol.Billings;

[SerializePackable(SerializeLayout.Explicit)]
public sealed class ServiceItemQueryResponse : PacketBase<ServiceItemQueryResponse>
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

            for (int i = 0; i < ServiceItems.Count; i++)
            {
                total += ServiceItems[i].Length;
            }

            return total;
        }
    }

    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public int TotalCount { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public List<ServiceItemDto> ServiceItems { get; set; } = [];

    public ServiceItemQueryResponse() => OpCode = OpCommand.NONE.AsUInt16();

    public override void ResetForPool()
    {
        if (ServiceItems?.Count > 0)
        {
            var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
            for (int i = 0; i < ServiceItems.Count; i++)
            {
                if (ServiceItems[i] is not null)
                {
                    pool.Return(ServiceItems[i]);
                }
            }
        }

        ServiceItems.Clear();
        SequenceId = 0;
        TotalCount = 0;
        OpCode = OpCommand.NONE.AsUInt16();
        base.ResetForPool();
    }

    public static ServiceItemQueryResponse Compress(ServiceItemQueryResponse packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        packet.Flags.AddFlag(PacketFlags.COMPRESSED);
        return packet;
    }

    public static ServiceItemQueryResponse Decompress(ServiceItemQueryResponse packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        packet.Flags.RemoveFlag(PacketFlags.COMPRESSED);
        return packet;
    }
}