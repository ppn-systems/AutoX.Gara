// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.Injection;
using Nalix.Framework.Extensions;
using Nalix.Framework.DataFrames;
using Nalix.Framework.Memory.Objects;
using System.Collections.Generic;

namespace AutoX.Gara.Shared.Protocol.Inventory;

// -----------------------------------------------------------------------------
// PART QUERY RESPONSE
// -----------------------------------------------------------------------------

/// <summary>
/// Packet tr? v? danh sách ph? tùng (<c>Part</c>) theo trang.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class PartQueryResponse : PacketBase<PartQueryResponse>
{
    /// <summary>
    /// T?ng byte length c?a packet, tính b?ng tay vì có dynamic list.
    /// </summary>
    [SerializeIgnore]
    public override System.Int32 Length
    {
        get
        {
            System.Int32 total = PacketConstants.HeaderSize
                + sizeof(System.UInt32)   // SequenceId
                + sizeof(System.Int32)    // TotalCount
                + sizeof(System.Int32);   // list item-count prefix

            for (System.Int32 i = 0; i < Parts.Count; i++)
            {
                total += Parts[i].Length;
            }

            return total;
        }
    }

    /// <summary>
    /// T?ng s? ph? tùng kh?p filter (tru?c khi phân trang).
    /// <para>Ph?i d?ng tru?c <see cref="Parts"/> — fixed field ph?i tru?c dynamic field.</para>
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public System.Int32 TotalCount { get; set; }

    /// <summary>Danh sách ph? tùng c?a trang hi?n t?i. Dynamic — d?t cu?i cùng.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public List<PartDto> Parts { get; set; } = [];

    /// <summary>Kh?i t?o v?i giá tr? m?c d?nh.</summary>
    public PartQueryResponse() => OpCode = OpCommand.NONE.AsUInt16();

    /// <inheritdoc/>
    public override void ResetForPool()
    {
        if (Parts?.Count > 0)
        {
            var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
            for (System.Int32 i = 0; i < Parts.Count; i++)
            {
                if (Parts[i] is not null)
                {
                    pool.Return(Parts[i]);
                }
            }
        }

        Parts.Clear();
        SequenceId = 0;
        TotalCount = 0;
        OpCode = OpCommand.NONE.AsUInt16();
        base.ResetForPool();
    }

    /// <inheritdoc/>
    public static PartQueryResponse Compress(PartQueryResponse packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        packet.Flags.AddFlag(PacketFlags.COMPRESSED);
        return packet;
    }

    /// <inheritdoc/>
    public static PartQueryResponse Decompress(PartQueryResponse packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        packet.Flags.RemoveFlag(PacketFlags.COMPRESSED);
        return packet;
    }
}