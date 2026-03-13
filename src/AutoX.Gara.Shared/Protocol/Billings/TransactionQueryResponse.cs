// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Framework.Injection;
using Nalix.Shared.Extensions;
using Nalix.Shared.Frames;
using Nalix.Shared.Memory.Pooling;
using System.Collections.Generic;

namespace AutoX.Gara.Shared.Protocol.Billings;

[SerializePackable(SerializeLayout.Explicit)]
public sealed class TransactionQueryResponse : PacketBase<TransactionQueryResponse>, IPacketTransformer<TransactionQueryResponse>, IPacketSequenced
{
    [SerializeIgnore]
    public override System.UInt16 Length
    {
        get
        {
            System.Int32 total = PacketConstants.HeaderSize
                + sizeof(System.UInt32) // SequenceId
                + sizeof(System.Int32)  // TotalCount
                + sizeof(System.Int32); // list item-count prefix

            for (System.Int32 i = 0; i < Transactions.Count; i++)
            {
                total += Transactions[i].Length;
            }

            return (System.UInt16)total;
        }
    }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION)]
    public System.UInt32 SequenceId { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32 TotalCount { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public List<TransactionDto> Transactions { get; set; } = [];

    public TransactionQueryResponse() => OpCode = OpCommand.NONE.AsUInt16();

    public override void ResetForPool()
    {
        if (Transactions?.Count > 0)
        {
            var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
            for (System.Int32 i = 0; i < Transactions.Count; i++)
            {
                if (Transactions[i] is not null)
                {
                    pool.Return(Transactions[i]);
                }
            }
        }

        Transactions.Clear();
        SequenceId = 0;
        TotalCount = 0;
        OpCode = OpCommand.NONE.AsUInt16();
        base.ResetForPool();
    }

    public static TransactionQueryResponse Compress(TransactionQueryResponse packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        packet.Flags.AddFlag(PacketFlags.COMPRESSED);
        return packet;
    }

    public static TransactionQueryResponse Decompress(TransactionQueryResponse packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        packet.Flags.RemoveFlag(PacketFlags.COMPRESSED);
        return packet;
    }
}

