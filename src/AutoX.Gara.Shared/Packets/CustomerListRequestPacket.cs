using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Attributes;
using Nalix.Common.Messaging.Packets.Abstractions;
using Nalix.Common.Serialization;
using Nalix.Framework.Injection;
using Nalix.Shared.Memory.Pooling;
using Nalix.Shared.Messaging;
using Nalix.Shared.Serialization;

namespace AutoX.Gara.Shared.Packets;

[SerializePackable(SerializeLayout.Explicit)]
[MagicNumber((System.UInt32)PacketMagic.CUSTOMER_LIST_REQUEST)]
public sealed class CustomerListRequestPacket : FrameBase, IPacketSequenced
{
    [SerializeOrder(0)]
    public System.UInt32 SequenceId { get; set; }

    [SerializeOrder(1)]
    public System.Int32 Page { get; set; } = 1;

    [SerializeOrder(2)]
    public System.Int32 PageSize { get; set; } = 20;

    public override System.UInt16 Length => 8;

    /// <summary>
    /// Đặt lại trạng thái để tái sử dụng từ pool.
    /// </summary>
    public override void ResetForPool()
    {
        Page = 1;
        PageSize = 20;
        SequenceId = 0;
        OpCode = OpCommand.NONE.AsUInt16();
    }

    /// <inheritdoc/>
    public static AccountPacket Deserialize(System.ReadOnlySpan<System.Byte> buffer)
    {
        AccountPacket packet = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                                       .Get<AccountPacket>();

        _ = LiteSerializer.Deserialize(buffer, ref packet);
        return packet;
    }

    /// <inheritdoc/>
    public override System.Byte[] Serialize() => LiteSerializer.Serialize(this);

    /// <inheritdoc/>
    public override System.Int32 Serialize(System.Span<System.Byte> buffer) => LiteSerializer.Serialize(this, buffer);
}
