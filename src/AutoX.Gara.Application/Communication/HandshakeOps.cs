// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Enums;
using Nalix.Common.Diagnostics.Abstractions;
using Nalix.Common.Networking.Abstractions;
using Nalix.Common.Networking.Packets.Attributes;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security.Enums;
using Nalix.Framework.Injection;
using Nalix.Network.Connections;
using Nalix.Shared.Frames.Controls;
using Nalix.Shared.Memory.Pooling;
using Nalix.Shared.Security.Asymmetric;
using Nalix.Shared.Security.Hashing;

namespace AutoX.Gara.Application.Communication;

/// <summary>
/// Quản lý quá trình bắt tay bảo mật để thiết lập kết nối mã hóa an toàn với client.
/// Sử dụng thuật toán trao đổi khóa X25519 và băm Keccak256 để đảm bảo tính bảo mật và toàn vẹn của kết nối.
/// Lớp này chịu trách nhiệm khởi tạo bắt tay, tạo cặp khóa, và tính toán khóa mã hóa chung.
/// </summary>
[PacketController]
public sealed class HandshakeOps
{
    [PacketEncryption(false)]
    [PacketPermission(PermissionLevel.NONE)]
    [PacketOpcode((System.UInt16)OpCommand.HANDSHAKE)]
    public static async System.Threading.Tasks.Task Handshake(
        Handshake packet,
        IConnection connection)
    {
        // Defensive programming - kiểm tra payload null
        if (packet.Data is null)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MISSING_REQUIRED_FIELD,
                ProtocolAdvice.FIX_AND_RETRY).ConfigureAwait(false);

            return;
        }

        // Xác thực độ dài khóa công khai, phải đúng 32 byte theo chuẩn X25519
        if (packet.Data.Length != 32)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.VALIDATION_FAILED,
                ProtocolAdvice.FIX_AND_RETRY).ConfigureAwait(false);

            return;
        }

        // Tạo response packet chứa public key của server
        Handshake response = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                                     .Get<Handshake>();
        System.Byte[] payload = [];

        try
        {
            // Tạo cặp khóa X25519 (khóa riêng và công khai) cho server
            X25519.X25519KeyPair keyPair = X25519.GenerateKeyPair();

            // Tính toán shared secret từ private key của server và public key của client
            System.Byte[] secret = X25519.Agreement(keyPair.PrivateKey, packet.Data);

            // Băm bí mật chung bằng Keccak256 để tạo khóa mã hóa an toàn
            connection.Secret = Keccak256.HashData(secret);

            // Security: Clear sensitive data từ memory
            System.Array.Clear(keyPair.PrivateKey, 0, keyPair.PrivateKey.Length);
            System.Array.Clear(secret, 0, secret.Length);

            // Nâng cấp quyền truy cập của client lên mức Guest
            connection.Level = PermissionLevel.GUEST;

            response.Initialize(keyPair.PublicKey);
            response.OpCode = (System.UInt16)OpCommand.HANDSHAKE;

            payload = response.Serialize();
        }
        catch (System.Exception ex)
        {
            // Error handling theo security best practices
            InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                    .Error($"[APP.{nameof(HandshakeOps)}] failed ep={connection.RemoteEndPoint} ex={ex.Message}");

            // Reset connection state nếu có lỗi
            connection.Secret = null;
            connection.Level = PermissionLevel.NONE;

            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.BACKOFF_RETRY,
                flags: ControlFlags.IS_TRANSIENT).ConfigureAwait(false);
        }
        finally
        {
            InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                    .Return<Handshake>(response);
        }

        if (payload is { Length: > 0 })
        {
            // If send fails, rollback state to avoid “half-upgraded” connection
            System.Boolean sent = await connection.TCP.SendAsync(payload).ConfigureAwait(false);
            if (!sent)
            {
                connection.Secret = null;
                connection.Level = PermissionLevel.GUEST;
                InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                        .Warn($"[APP.{nameof(HandshakeOps)}] send-failed ep={connection.RemoteEndPoint}");
                return;
            }
        }
    }
}