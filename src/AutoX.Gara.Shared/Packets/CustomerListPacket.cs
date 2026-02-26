using AutoX.Gara.Shared.Enums;
using Nalix.Common.Attributes;
using Nalix.Common.Enums;
using Nalix.Common.Infrastructure.Caching;
using Nalix.Common.Messaging.Packets.Abstractions;
using Nalix.Common.Serialization;
using Nalix.Framework.Injection;
using Nalix.Shared.Memory.Pooling;
using Nalix.Shared.Messaging;
using Nalix.Shared.Serialization;
using System.Collections.Generic;

namespace AutoX.Gara.Shared.Packets;

/// <summary>
/// Packet containing a list of customers for paging/query operations.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
[MagicNumber((System.UInt32)PacketMagic.CUSTOMER_LIST)]
public class CustomerListPacket : FrameBase, IPoolable, IPacketEncryptor<CustomerListPacket>, IPacketSequenced
{
    /// <summary>
    /// Gets the total length of the packet in bytes (after serialization).
    /// </summary>
    public override System.UInt16 Length => (System.UInt16)LiteSerializer.Serialize(this).Length;

    [SerializeOrder(0)]
    public System.UInt32 SequenceId { get; set; }

    /// <summary>
    /// List of customers for the current page.
    /// </summary>
    [SerializeOrder(1)]
    public List<CustomerPacket> Customers { get; set; } = [];

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CustomerListPacket() { }

    /// <summary>
    /// Deserialize a CustomerListPacket from a byte buffer.
    /// </summary>
    /// <param name="buffer">Input buffer containing serialized data.</param>
    /// <returns>CustomerListPacket populated from buffer.</returns>
    public static CustomerListPacket Deserialize(System.ReadOnlySpan<System.Byte> buffer)
    {
        CustomerListPacket packet = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                                            .Get<CustomerListPacket>();

        _ = LiteSerializer.Deserialize(buffer, ref packet);
        return packet;
    }

    /// <inheritdoc/>
    public override System.Byte[] Serialize() => LiteSerializer.Serialize(this);

    /// <inheritdoc/>
    public override System.Int32 Serialize(System.Span<System.Byte> buffer) => LiteSerializer.Serialize(this, buffer);

    /// <inheritdoc/>
    public override void ResetForPool() => Customers = [];

    /// <inheritdoc/>
    public static CustomerListPacket Encrypt(CustomerListPacket packet, System.Byte[] key, CipherSuiteType algorithm)
    {
        System.ArgumentNullException.ThrowIfNull(packet);

        for (System.Int32 i = 0; i < packet.Customers.Count; i++)
        {
            packet.Customers[i] = CustomerPacket.Encrypt(packet.Customers[i], key, algorithm);
        }
        return packet;
    }

    /// <inheritdoc/>
    public static CustomerListPacket Decrypt(CustomerListPacket packet, System.Byte[] key)
    {
        System.ArgumentNullException.ThrowIfNull(packet);

        for (System.Int32 i = 0; i < packet.Customers.Count; i++)
        {
            packet.Customers[i] = CustomerPacket.Decrypt(packet.Customers[i], key);
        }
        return packet;
    }
}