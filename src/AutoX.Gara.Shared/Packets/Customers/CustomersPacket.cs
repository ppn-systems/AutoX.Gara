using AutoX.Gara.Shared.Enums;
using Nalix.Common.Networking.Caching;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Security.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Common.Shared.Attributes;
using Nalix.Framework.Injection;
using Nalix.Shared.Frames;
using Nalix.Shared.Memory.Pooling;
using Nalix.Shared.Serialization;
using System.Collections.Generic;

namespace AutoX.Gara.Shared.Packets.Customers;

/// <summary>
/// Packet containing a list of customers for paging/query operations.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
[MagicNumber((System.UInt32)PacketMagic.CUSTOMER_LIST)]
public class CustomersPacket : FrameBase, IPoolable, IPacketEncryptor<CustomersPacket>, IPacketSequenced
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
    public List<CustomerDataPacket> Customers { get; set; } = [];

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CustomersPacket() { }

    /// <summary>
    /// Deserialize a CustomerListPacket from a byte buffer.
    /// </summary>
    /// <param name="buffer">Input buffer containing serialized data.</param>
    /// <returns>CustomerListPacket populated from buffer.</returns>
    public static CustomersPacket Deserialize(System.ReadOnlySpan<System.Byte> buffer)
    {
        CustomersPacket packet = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                                            .Get<CustomersPacket>();

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
    public static CustomersPacket Encrypt(CustomersPacket packet, System.Byte[] key, CipherSuiteType algorithm)
    {
        System.ArgumentNullException.ThrowIfNull(packet);

        for (System.Int32 i = 0; i < packet.Customers.Count; i++)
        {
            packet.Customers[i] = CustomerDataPacket.Encrypt(packet.Customers[i], key, algorithm);
        }
        return packet;
    }

    /// <inheritdoc/>
    public static CustomersPacket Decrypt(CustomersPacket packet, System.Byte[] key)
    {
        System.ArgumentNullException.ThrowIfNull(packet);

        for (System.Int32 i = 0; i < packet.Customers.Count; i++)
        {
            packet.Customers[i] = CustomerDataPacket.Decrypt(packet.Customers[i], key);
        }
        return packet;
    }
}