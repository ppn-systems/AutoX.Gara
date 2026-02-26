// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using AutoX.Gara.Shared.Models;
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
using Nalix.Shared.Serialization;

namespace AutoX.Gara.Shared.Packets;

/// <summary>
/// Gói tin chứa thông tin đăng nhập từ client (username, mật khẩu băm, metadata),
/// dùng trong quá trình xác thực sau handshake.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
[MagicNumber((System.UInt32)PacketMagic.ACCOUNT)]
public class AccountPacket : FrameBase, IPoolable, IPacketTransformer<AccountPacket>, IPacketSequenced
{
    /// <summary>
    /// Tổng độ dài gói tin (byte), gồm header và nội dung.
    /// </summary>
    [SerializeIgnore]
    public override System.UInt16 Length => (System.UInt16)(PacketConstants.HeaderSize + Account.Length);

    [SerializeOrder(PacketHeaderOffset.DATA_REGION)]
    public System.UInt32 SequenceId { get; set; }

    /// <summary>
    /// Thông tin đăng nhập (username, mật khẩu băm, metadata).
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public AccountModel Account { get; set; }

    /// <summary>
    /// Khởi tạo mặc định với MagicNumber và CREDENTIALS rỗng.
    /// </summary>
    public AccountPacket()
    {
        Account = new AccountModel();
        OpCode = OpCommand.NONE.AsUInt16();
        MagicNumber = PacketMagic.ACCOUNT.AsUInt32();
    }

    /// <summary>
    /// Thiết lập OpCode và CREDENTIALS.
    /// </summary>
    public void Initialize(System.UInt16 opCode, AccountModel account)
    {
        OpCode = opCode;
        Account = account ?? throw new System.ArgumentNullException(nameof(account));
    }

    /// <summary>
    /// Đặt lại trạng thái để tái sử dụng từ pool.
    /// </summary>
    public override void ResetForPool()
    {
        Account = new AccountModel();
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

    /// <inheritdoc/>
    public static AccountPacket Encrypt(AccountPacket packet, System.Byte[] key, CipherSuiteType algorithm)
    {
        if (packet?.Account == null)
        {
            throw new System.ArgumentNullException(nameof(packet));
        }

        packet.Account.Username = packet.Account.Username.EncryptToBase64(key, algorithm);
        packet.Account.Password = packet.Account.Password.EncryptToBase64(key, algorithm);

        packet.Flags.AddFlag(PacketFlags.ENCRYPTED);

        return packet;
    }

    /// <inheritdoc/>
    public static AccountPacket Decrypt(AccountPacket packet, System.Byte[] key)
    {
        if (packet?.Account == null)
        {
            throw new System.ArgumentNullException(nameof(packet));
        }

        try
        {
            packet.Account.Username = packet.Account.Username.DecryptFromBase64(key);
            packet.Account.Password = packet.Account.Password.DecryptFromBase64(key);

            packet.Flags.RemoveFlag(PacketFlags.ENCRYPTED);

            return packet;
        }
        catch (System.FormatException ex)
        {
            throw new System.InvalidOperationException("Failed to decode Base64-encoded credentials.", ex);
        }
        catch (System.Exception ex)
        {
            throw new System.InvalidOperationException("Failed to decrypt credentials.", ex);
        }
    }

    /// <inheritdoc/>
    public static AccountPacket Compress(AccountPacket packet)
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
    public static AccountPacket Decompress(AccountPacket packet)
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