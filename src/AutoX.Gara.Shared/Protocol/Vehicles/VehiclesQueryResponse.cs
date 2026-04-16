using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Common.Networking.Protocols;
using AutoX.Gara.Shared.Extensions;
using AutoX.Gara.Shared.Protocol.Customers;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.Injection;
using Nalix.Framework.DataFrames;
using Nalix.Framework.Memory.Objects;
using System.Collections.Generic;

namespace AutoX.Gara.Shared.Protocol.Vehicles;

/// <summary>
/// Represents a packet that carries a collection of customer records,
/// used for paging and bulk query operations.
/// Uses PacketBase for automatic serialization, pooling and metadata handling.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class VehiclesQueryResponse : PacketBase<VehiclesQueryResponse>
{
    /// <summary>
    /// Gets the total byte length of this packet, including the fixed header
    /// and the serialized size of all customer entries.
    /// </summary>
    /// <remarks>
    /// The length is computed by summing:
    ///   - Fixed header  (PacketConstants.HeaderSize)
    ///   - SequenceId    (UInt32 = 4 bytes)
    ///   - TotalCount    (Int32  = 4 bytes)   ? fixed, must be counted
    ///   - List prefix   (Int32  = 4 bytes)   ? item count written by serializer
    ///   - Each CustomerDataPacket.Length
    /// TotalCount MUST come before Customers in SerializeOrder so the serializer
    /// writes it before the dynamic list ? otherwise it is silently dropped.
    /// </remarks>
    [SerializeIgnore]
    public override int Length
    {
        get
        {
            // header + SequenceId (UInt32) + TotalCount (Int32) + list-count prefix (Int32)
            int total = PacketConstants.HeaderSize
                + sizeof(System.UInt32)   // SequenceId
                + sizeof(int)    // TotalCount  ? was missing before
                + sizeof(int);   // list item-count prefix

            // Add each customer's individual serialized length
            for (int i = 0; i < Vehicles.Count; i++)
            {
                total += Vehicles[i].Length;
            }

            return total;
        }
    }

    /// <summary>
    /// T?ng s? kh�ch h�ng kh?p v?i filter tr�n server (tru?c khi ph�n trang).
    /// Client d�ng d? t�nh TotalPages.
    /// <para>
    /// PH?I d?ng tru?c <see cref="Vehicles"/> v� d�y l� fixed-size field.
    /// PacketBase d?ng t�nh Length ngay khi g?p dynamic field d?u ti�n ?
    /// b?t k? fixed field n�o d?ng sau List s? b? b? qua khi serialize.
    /// </para>
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the list of customer records for the current page.
    /// Dynamic field ? ph?i d?ng CU?I C�NG trong SerializeOrder.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public List<VehicleDto> Vehicles { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of <see cref="CustomerQueryResponse"/> with default values.
    /// </summary>
    public VehiclesQueryResponse() => OpCode = OpCommand.NONE.AsUInt16();

    /// <inheritdoc/>
    public override void ResetForPool()
    {
        // Return pooled child packets first to avoid leaking pooled instances.
        if (Vehicles?.Count > 0)
        {
            var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
            for (int i = 0; i < Vehicles.Count; i++)
            {
                var child = Vehicles[i];
                if (child is not null)
                {
                    // Return child to pool and set slot to null for safety.
                    pool.Return(child);
                }
            }
        }

        // Clear the list and reset header fields via base.
        Vehicles.Clear();
        SequenceId = 0;
        TotalCount = 0;
        OpCode = OpCommand.NONE.AsUInt16();

        // Let base reset other serializable properties/header if needed.
        base.ResetForPool();
    }
}