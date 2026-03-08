// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Attributes;
using Nalix.Common.Enums;
using Nalix.Common.Infrastructure.Caching;
using Nalix.Common.Messaging.Packets;
using Nalix.Common.Messaging.Packets.Abstractions;
using Nalix.Common.Serialization;
using Nalix.Framework.Injection;
using Nalix.Shared.Extensions;
using Nalix.Shared.Memory.Pooling;
using Nalix.Shared.Messaging;
using Nalix.Shared.Security;
using Nalix.Shared.Serialization;

namespace AutoX.Gara.Shared.Packets.Auth;

/// <summary>
/// Gói tin chứa thông tin đăng nhập từ client (username, mật khẩu băm, metadata),
/// dùng trong quá trình xác thực sau handshake.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
[MagicNumber((System.UInt32)PacketMagic.ACCOUNT)]
public class LoginPacket : FrameBase, IPoolable, IPacketTransformer<LoginPacket>, IPacketSequenced
{
    /// <summary>
    /// Tổng độ dài gói tin (byte), gồm header và nội dung.
    /// </summary>
    [SerializeIgnore]
    public override System.UInt16 Length => (System.UInt16)(PacketConstants.HeaderSize + Account.Length + sizeof(System.UInt32));

    [SerializeOrder(PacketHeaderOffset.DATA_REGION)]
    public System.UInt32 SequenceId { get; set; }

    /// <summary>
    /// Thông tin đăng nhập (username, mật khẩu băm, metadata).
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public LoginRequestModel Account { get; set; }

    /// <summary>
    /// Khởi tạo mặc định với MagicNumber và CREDENTIALS rỗng.
    /// </summary>
    public LoginPacket()
    {
        Account = new LoginRequestModel();
        OpCode = OpCommand.NONE.AsUInt16();
        MagicNumber = PacketMagic.ACCOUNT.AsUInt32();
    }

    /// <summary>
    /// Thiết lập OpCode và CREDENTIALS.
    /// </summary>
    public void Initialize(System.UInt16 opCode, LoginRequestModel account)
    {
        OpCode = opCode;
        Account = account ?? throw new System.ArgumentNullException(nameof(account));
    }

    /// <summary>
    /// Đặt lại trạng thái để tái sử dụng từ pool.
    /// </summary>
    public override void ResetForPool()
    {
        Account = new LoginRequestModel();
        OpCode = OpCommand.NONE.AsUInt16();
    }

    /// <inheritdoc/>
    public static LoginPacket Deserialize(System.ReadOnlySpan<System.Byte> buffer)
    {
        LoginPacket packet = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                                       .Get<LoginPacket>();

        _ = LiteSerializer.Deserialize(buffer, ref packet);
        return packet;
    }

    /// <inheritdoc/>
    public override System.Byte[] Serialize() => LiteSerializer.Serialize(this);

    /// <inheritdoc/>
    public override System.Int32 Serialize(System.Span<System.Byte> buffer) => LiteSerializer.Serialize(this, buffer);

    /// <inheritdoc/>
    public static LoginPacket Encrypt(LoginPacket packet, System.Byte[] key, CipherSuiteType algorithm) => EnvelopeEncryptor.Encrypt(packet, key, algorithm);

    /// <inheritdoc/>
    public static LoginPacket Decrypt(LoginPacket packet, System.Byte[] key) => EnvelopeEncryptor.Decrypt(packet, key);

    /// <inheritdoc/>
    public static LoginPacket Compress(LoginPacket packet)
    {
        if (packet?.Account == null)
        {
            throw new System.ArgumentNullException(nameof(packet));
        }

        packet.Account.Username = packet.Account.Username.CompressToBase64();
        packet.Account.Password = packet.Account.Password.CompressToBase64();

        packet.Flags.AddFlag(PacketFlags.COMPRESSED);

        return packet;
    }

    /// <inheritdoc/>
    public static LoginPacket Decompress(LoginPacket packet)
    {
        if (packet?.Account == null)
        {
            throw new System.ArgumentNullException(nameof(packet));
        }

        packet.Account.Username = packet.Account.Username.DecompressFromBase64();
        packet.Account.Password = packet.Account.Password.DecompressFromBase64();

        packet.Flags.RemoveFlag(PacketFlags.COMPRESSED);

        return packet;
    }
}