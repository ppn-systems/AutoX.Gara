using AutoX.Gara.Shared.Enums;
using Nalix.Common.Attributes;
using Nalix.Common.Messaging.Packets.Abstractions;
using Nalix.Common.Serialization;
using Nalix.Shared.Messaging;
using Nalix.Shared.Serialization;

namespace AutoX.Gara.Shared.Packets.Customers;

[SerializePackable(SerializeLayout.Explicit)]
[MagicNumber((System.UInt32)PacketMagic.CUSTOMER_LIST_REQUEST)]
public sealed class CustomersQueryPacket : FrameBase, IPacketSequenced
{
    [SerializeOrder(0)]
    public System.UInt32 SequenceId { get; set; }

    [SerializeOrder(1)]
    public System.Int32 Page { get; set; } = 1;

    [SerializeOrder(2)]
    public System.Int32 PageSize { get; set; } = 20;

    public override System.UInt16 Length => 8;

    /// <inheritdoc/>
    public static CustomersQueryPacket Deserialize(System.ReadOnlySpan<System.Byte> buffer)
    {
        CustomersQueryPacket packet = new();
        _ = LiteSerializer.Deserialize(buffer, ref packet);
        return packet;
    }

    public override void ResetForPool() => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public override System.Byte[] Serialize() => LiteSerializer.Serialize(this);

    /// <inheritdoc/>
    public override System.Int32 Serialize(System.Span<System.Byte> buffer) => LiteSerializer.Serialize(this, buffer);
}
