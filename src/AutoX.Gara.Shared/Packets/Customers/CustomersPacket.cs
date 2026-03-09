// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Enums;
using Nalix.Common.Security.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Framework.Injection;
using Nalix.Shared.Extensions;
using Nalix.Shared.Frames;
using Nalix.Shared.Memory.Pooling;
using System.Collections.Generic;

namespace AutoX.Gara.Shared.Packets.Customers;

/// <summary>
/// Represents a packet that carries a collection of customer records,
/// used for paging and bulk query operations.
/// Uses PacketBase for automatic serialization, pooling and metadata handling.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class CustomersPacket : PacketBase<CustomersPacket>, IPacketTransformer<CustomersPacket>, IPacketSequenced
{
    /// <summary>
    /// Gets the total byte length of this packet, including the fixed header
    /// and the serialized size of all customer entries.
    /// </summary>
    /// <remarks>
    /// The length is computed by summing the header size, the sequence ID field,
    /// the 4-byte customer count prefix, and each customer's individual serialized size.
    /// This avoids allocating a temporary byte array just to measure length.
    /// </remarks>
    [SerializeIgnore]
    public override System.UInt16 Length
    {
        get
        {
            // Start with: header + SequenceId (UInt32) + list count prefix (Int32)
            System.Int32 total = PacketConstants.HeaderSize
                + sizeof(System.UInt32)
                + sizeof(System.Int32);

            // Add each customer's individual serialized length
            for (System.Int32 i = 0; i < Customers.Count; i++)
            {
                total += Customers[i].Length;
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
    /// Gets or sets the list of customer records for the current page.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public List<CustomerDataPacket> Customers { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of <see cref="CustomersPacket"/> with default values.
    /// </summary>
    public CustomersPacket() => OpCode = OpCommand.NONE.AsUInt16();

    /// <inheritdoc/>
    public override void ResetForPool()
    {
        // Return pooled child packets first to avoid leaking pooled instances.
        if (Customers?.Count > 0)
        {
            var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
            for (System.Int32 i = 0; i < Customers.Count; i++)
            {
                var child = Customers[i];
                if (child is not null)
                {
                    // Return child to pool and set slot to null for safety.
                    pool.Return(child);
                }
            }
        }

        // Clear the list and reset header fields via base.
        Customers.Clear();
        SequenceId = 0;
        OpCode = OpCommand.NONE.AsUInt16();

        // Let base reset other serializable properties/header if needed.
        base.ResetForPool();
    }

    // Serialize/Serialize(Span<byte>) are inherited from PacketBase.

    /// <inheritdoc/>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="packet"/> or <paramref name="key"/> is <see langword="null"/>.
    /// </exception>
    public static CustomersPacket Encrypt(CustomersPacket packet, System.Byte[] key, CipherSuiteType algorithm)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        System.ArgumentNullException.ThrowIfNull(key);

        for (System.Int32 i = 0; i < packet.Customers.Count; i++)
        {
            packet.Customers[i] = CustomerDataPacket.Encrypt(packet.Customers[i], key, algorithm);
        }

        packet.Flags.AddFlag(PacketFlags.ENCRYPTED);
        return packet;
    }

    /// <inheritdoc/>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="packet"/> or <paramref name="key"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown when decryption of one or more customer entries fails.
    /// </exception>
    public static CustomersPacket Decrypt(CustomersPacket packet, System.Byte[] key)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        System.ArgumentNullException.ThrowIfNull(key);

        for (System.Int32 i = 0; i < packet.Customers.Count; i++)
        {
            packet.Customers[i] = CustomerDataPacket.Decrypt(packet.Customers[i], key);
        }

        packet.Flags.RemoveFlag(PacketFlags.ENCRYPTED);
        return packet;
    }

    /// <inheritdoc/>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="packet"/> is <see langword="null"/>.
    /// </exception>
    public static CustomersPacket Compress(CustomersPacket packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);

        for (System.Int32 i = 0; i < packet.Customers.Count; i++)
        {
            packet.Customers[i] = CustomerDataPacket.Compress(packet.Customers[i]);
        }

        packet.Flags.AddFlag(PacketFlags.COMPRESSED);
        return packet;
    }

    /// <inheritdoc/>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="packet"/> is <see langword="null"/>.
    /// </exception>
    public static CustomersPacket Decompress(CustomersPacket packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);

        for (System.Int32 i = 0; i < packet.Customers.Count; i++)
        {
            packet.Customers[i] = CustomerDataPacket.Decompress(packet.Customers[i]);
        }

        packet.Flags.RemoveFlag(PacketFlags.COMPRESSED);
        return packet;
    }
}