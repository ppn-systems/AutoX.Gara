// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using AutoX.Gara.Shared.Protocol.Customers;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Enums;
using Nalix.Common.Security.Attributes;
using Nalix.Common.Security.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Framework.Injection;
using Nalix.Shared.Extensions;
using Nalix.Shared.Frames;
using Nalix.Shared.Memory.Pooling;
using System.Collections.Generic;

namespace AutoX.Gara.Shared.Protocol.Vehicles;

/// <summary>
/// Represents a packet that carries a collection of customer records,
/// used for paging and bulk query operations.
/// Uses PacketBase for automatic serialization, pooling and metadata handling.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class VehiclesQueryResponse : PacketBase<VehiclesQueryResponse>, IPacketTransformer<VehiclesQueryResponse>, IPacketSequenced
{
    /// <summary>
    /// Gets the total byte length of this packet, including the fixed header
    /// and the serialized size of all customer entries.
    /// </summary>
    /// <remarks>
    /// The length is computed by summing:
    ///   - Fixed header  (PacketConstants.HeaderSize)
    ///   - SequenceId    (UInt32 = 4 bytes)
    ///   - TotalCount    (Int32  = 4 bytes)   ← fixed, must be counted
    ///   - List prefix   (Int32  = 4 bytes)   ← item count written by serializer
    ///   - Each CustomerDataPacket.Length
    /// TotalCount MUST come before Customers in SerializeOrder so the serializer
    /// writes it before the dynamic list — otherwise it is silently dropped.
    /// </remarks>
    [SerializeIgnore]
    public override System.UInt16 Length
    {
        get
        {
            // header + SequenceId (UInt32) + TotalCount (Int32) + list-count prefix (Int32)
            System.Int32 total = PacketConstants.HeaderSize
                + sizeof(System.UInt32)   // SequenceId
                + sizeof(System.Int32)    // TotalCount  ← was missing before
                + sizeof(System.Int32);   // list item-count prefix

            // Add each customer's individual serialized length
            for (System.Int32 i = 0; i < Vehicles.Count; i++)
            {
                total += Vehicles[i].Length;
            }

            return (System.UInt16)total;
        }
    }

    /// <summary>
    /// Gets or sets the sequence identifier used for packet ordering and deduplication.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION)]
    public System.UInt32 SequenceId { get; set; }

    /// <summary>
    /// Tổng số khách hàng khớp với filter trên server (trước khi phân trang).
    /// Client dùng để tính TotalPages.
    /// <para>
    /// PHẢI đứng trước <see cref="Vehicles"/> vì đây là fixed-size field.
    /// PacketBase dừng tính Length ngay khi gặp dynamic field đầu tiên —
    /// bất kỳ fixed field nào đứng sau List sẽ bị bỏ qua khi serialize.
    /// </para>
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32 TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the list of customer records for the current page.
    /// Dynamic field — phải đứng CUỐI CÙNG trong SerializeOrder.
    /// </summary>
    [SensitiveData(DataSensitivityLevel.Internal)]
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
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
            for (System.Int32 i = 0; i < Vehicles.Count; i++)
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

    /// <inheritdoc/>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="packet"/> is <see langword="null"/>.
    /// </exception>
    public static VehiclesQueryResponse Compress(VehiclesQueryResponse packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);

        for (System.Int32 i = 0; i < packet.Vehicles.Count; i++)
        {
            packet.Vehicles[i] = VehicleDto.Compress(packet.Vehicles[i]);
        }

        packet.Flags.AddFlag(PacketFlags.COMPRESSED);
        return packet;
    }

    /// <inheritdoc/>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="packet"/> is <see langword="null"/>.
    /// </exception>
    public static VehiclesQueryResponse Decompress(VehiclesQueryResponse packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);

        for (System.Int32 i = 0; i < packet.Vehicles.Count; i++)
        {
            packet.Vehicles[i] = VehicleDto.Decompress(packet.Vehicles[i]);
        }

        packet.Flags.RemoveFlag(PacketFlags.COMPRESSED);
        return packet;
    }
}