// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.Injection;
using Nalix.Shared.Extensions;
using Nalix.Shared.Frames;
using Nalix.Shared.Memory.Objects;
using System.Collections.Generic;

namespace AutoX.Gara.Shared.Protocol.Inventory;

// ═════════════════════════════════════════════════════════════════════════════
// PART QUERY RESPONSE
// ═════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Packet trả về danh sách phụ tùng (<c>Part</c>) theo trang.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class PartQueryResponse : PacketBase<PartQueryResponse>
{
    /// <summary>
    /// Tổng byte length của packet, tính bằng tay vì có dynamic list.
    /// </summary>
    [SerializeIgnore]
    public override System.UInt16 Length
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

            return (System.UInt16)total;
        }
    }

    /// <summary>
    /// Tổng số phụ tùng khớp filter (trước khi phân trang).
    /// <para>Phải đứng trước <see cref="Parts"/> — fixed field phải trước dynamic field.</para>
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32 TotalCount { get; set; }

    /// <summary>Danh sách phụ tùng của trang hiện tại. Dynamic — đặt cuối cùng.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public List<PartDto> Parts { get; set; } = [];

    /// <summary>Khởi tạo với giá trị mặc định.</summary>
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