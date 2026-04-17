using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Common.Networking.Protocols;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.Injection;
using Nalix.Framework.DataFrames;
using Nalix.Framework.Memory.Objects;
using System.Collections.Generic;

namespace AutoX.Gara.Shared.Protocol.Billings;

/// <summary>
/// Packet tra ve danh sach hoa don theo trang.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class InvoiceQueryResponse : PacketBase<InvoiceQueryResponse>
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

            for (int i = 0; i < Invoices.Count; i++)
            {
                total += Invoices[i].Length;
            }

            return total;
        }
    }

    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public int TotalCount { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public List<InvoiceDto> Invoices { get; set; } = [];

    public InvoiceQueryResponse() => OpCode = OpCommand.NONE.AsUInt16();

    public override void ResetForPool()
    {
        if (Invoices?.Count > 0)
        {
            var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
            for (int i = 0; i < Invoices.Count; i++)
            {
                if (Invoices[i] is not null)
                {
                    pool.Return(Invoices[i]);
                }
            }
        }

        Invoices.Clear();
        SequenceId = 0;
        TotalCount = 0;
        OpCode = OpCommand.NONE.AsUInt16();
        base.ResetForPool();
    }
}
