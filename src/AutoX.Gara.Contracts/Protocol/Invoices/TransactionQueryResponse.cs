using AutoX.Gara.Contracts.Enums;
// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Contracts.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using Nalix.Framework.Injection;
using Nalix.Framework.Memory.Objects;
using System.Collections.Generic;
namespace AutoX.Gara.Contracts.Protocol.Invoices;
[SerializePackable(SerializeLayout.Explicit)]
public sealed class TransactionQueryResponse : PacketBase<TransactionQueryResponse>
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
            for (int i = 0; i < Transactions.Count; i++)
            {
                total += Transactions[i].Length;
            }
            return total;
        }
    }
    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public int TotalCount { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public List<TransactionDto> Transactions { get; set; } = [];
    public TransactionQueryResponse() => OpCode = OpCommand.NONE.AsUInt16();
    public override void ResetForPool()
    {
        if (Transactions?.Count > 0)
        {
            var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
            for (int i = 0; i < Transactions.Count; i++)
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
}

