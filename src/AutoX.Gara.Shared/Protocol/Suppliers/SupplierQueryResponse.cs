// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.Injection;
using Nalix.Framework.DataFrames;
using Nalix.Framework.Memory.Objects;
using System.Collections.Generic;

namespace AutoX.Gara.Shared.Protocol.Suppliers;

/// <summary>
/// Packet tr? v? danh sách nhà cung c?p theo trang t? server xu?ng client.
/// <para>
/// C?u trúc Length tính th? công vì ch?a dynamic list <see cref="Suppliers"/>.
/// <see cref="TotalCount"/> PH?I d?ng tru?c <see cref="Suppliers"/> trong
/// <c>SerializeOrder</c> — fixed field sau dynamic list s? b? serializer b? qua.
/// </para>
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class SupplierQueryResponse : PacketBase<SupplierQueryResponse>
{
    /// <summary>
    /// T?ng s? byte c?a packet, tính th? công d? bao g?m child packets.
    /// </summary>
    /// <remarks>
    /// Layout:
    ///   - Fixed header    (PacketConstants.HeaderSize)
    ///   - SequenceId      (UInt32 = 4 bytes)
    ///   - TotalCount      (Int32  = 4 bytes)
    ///   - List item-count (Int32  = 4 bytes) ? prefix ghi b?i serializer
    ///   - M?i SupplierDto.Length
    /// </remarks>
    [SerializeIgnore]
    public override System.Int32 Length
    {
        get
        {
            System.Int32 total = PacketConstants.HeaderSize
                + sizeof(System.UInt32)   // SequenceId
                + sizeof(System.Int32)    // TotalCount
                + sizeof(System.Int32);   // list item-count prefix

            for (System.Int32 i = 0; i < Suppliers.Count; i++)
            {
                total += Suppliers[i].Length;
            }

            return total;
        }
    }

    /// <summary>
    /// T?ng s? nhà cung c?p kh?p filter trên server (tru?c phân trang).
    /// Client dùng d? tính TotalPages.
    /// <para>
    /// PH?I d?ng tru?c <see cref="Suppliers"/> — dây là fixed-size field.
    /// </para>
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public System.Int32 TotalCount { get; set; }

    /// <summary>
    /// Danh sách nhà cung c?p trên trang hi?n t?i.
    /// Dynamic field — ph?i d?ng CU?I CÙNG trong SerializeOrder.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public List<SupplierDto> Suppliers { get; set; } = [];

    /// <summary>Kh?i t?o v?i giá tr? m?c d?nh.</summary>
    public SupplierQueryResponse() => OpCode = OpCommand.NONE.AsUInt16();

    /// <inheritdoc/>
    public override void ResetForPool()
    {
        // Tr? child packets v? pool tru?c d? tránh leak.
        if (Suppliers?.Count > 0)
        {
            var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
            for (System.Int32 i = 0; i < Suppliers.Count; i++)
            {
                if (Suppliers[i] is not null)
                {
                    pool.Return(Suppliers[i]);
                }
            }
        }

        Suppliers.Clear();
        SequenceId = 0;
        TotalCount = 0;
        OpCode = OpCommand.NONE.AsUInt16();

        base.ResetForPool();
    }
}