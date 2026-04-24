using AutoX.Gara.Contracts.Enums;
// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Contracts.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using Nalix.Framework.Injection;
using Nalix.Framework.Memory.Objects;
using System.Collections.Generic;
namespace AutoX.Gara.Contracts.Protocol.Suppliers;
/// <summary>
/// Packet tr? v? danh s�ch nh� cung c?p theo trang t? server xu?ng client.
/// <para>
/// C?u tr�c Length t�nh th? c�ng v� ch?a dynamic list <see cref="Suppliers"/>.
/// <see cref="TotalCount"/> PH?I d?ng tru?c <see cref="Suppliers"/> trong
/// <c>SerializeOrder</c> ? fixed field sau dynamic list s? b? serializer b? qua.
/// </para>
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class SupplierQueryResponse : PacketBase<SupplierQueryResponse>
{
    /// <summary>
    /// T?ng s? byte c?a packet, t�nh th? c�ng d? bao g?m child packets.
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
    public override int Length
    {
        get
        {
            int total = PacketConstants.HeaderSize
                + sizeof(System.UInt32)   // SequenceId
                + sizeof(int)    // TotalCount
                + sizeof(int);   // list item-count prefix
            for (int i = 0; i < Suppliers.Count; i++)
            {
                total += Suppliers[i].Length;
            }
            return total;
        }
    }
    /// <summary>
    /// T?ng s? nh� cung c?p kh?p filter tr�n server (tru?c ph�n trang).
    /// Client d�ng d? t�nh TotalPages.
    /// <para>
    /// PH?I d?ng tru?c <see cref="Suppliers"/> ? d�y l� fixed-size field.
    /// </para>
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public int TotalCount { get; set; }
    /// <summary>
    /// Danh s�ch nh� cung c?p tr�n trang hi?n t?i.
    /// Dynamic field ? ph?i d?ng CU?I C�NG trong SerializeOrder.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public List<SupplierDto> Suppliers { get; set; } = [];
    /// <summary>Kh?i t?o v?i gi� tr? m?c d?nh.</summary>
    public SupplierQueryResponse() => OpCode = OpCommand.NONE.AsUInt16();
    /// <inheritdoc/>
    public override void ResetForPool()
    {
        // Tr? child packets v? pool tru?c d? tr�nh leak.
        if (Suppliers?.Count > 0)
        {
            var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
            for (int i = 0; i < Suppliers.Count; i++)
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

