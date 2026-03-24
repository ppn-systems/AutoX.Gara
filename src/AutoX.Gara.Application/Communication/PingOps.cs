// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Common.Diagnostics;
using Nalix.Common.Networking;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security;
using Nalix.Framework.Injection;
using Nalix.Framework.Time;
using Nalix.Network.Connections;
using Nalix.Shared.Frames.Controls;
using Nalix.Shared.Memory.Objects;

namespace AutoX.Gara.Application.Communication;

/// <summary>
/// Xử lý ControlType.PING từ client: xác thực và trả về ControlType.PONG.
/// </summary>
[PacketController]
public sealed class PingOps
{
    [PacketOpcode(0)]
    [PacketEncryption(false)]
    [PacketPermission(PermissionLevel.GUEST)]
    public static async System.Threading.Tasks.Task Ping(
        IPacket p,
        IConnection connection)
    {
        // Chỉ nhận gói Control có type = PING
        if (p is not Control ping || ping.Type != ControlType.PING)
        {
            System.UInt32 fallbackSeq = p is IPacketSequenced ps ? ps.SequenceId : 0;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);

            return;
        }

        // Tạo PONG response frame (echo lại SequenceId, MonoTicks mới, timestamp mới)
        Control pong = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                               .Get<Control>();

        try
        {
            pong.Initialize(
                opCode: ping.OpCode,      // Echo lại OpCode giống client gửi lên
                type: ControlType.PONG,
                sequenceId: ping.SequenceId,
                reasonCode: ProtocolReason.NONE,    // Không lỗi
                transport: ping.Protocol);

            pong.MonoTicks = ping.MonoTicks; // Option: echo lại MonoTicks client gửi lên (để RTT tốt nhất)
            pong.Timestamp = Clock.UnixMillisecondsNow();

            // Gửi Control PONG về client
            await connection.TCP.SendAsync(pong.Serialize()).ConfigureAwait(false);
        }
        catch (System.Exception ex)
        {
            InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                    .Error($"[APP.{nameof(PingOps)}] failed ep={connection.NetworkEndpoint} ex={ex.Message}");

            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.BACKOFF_RETRY,
                sequenceId: ping.SequenceId,
                flags: ControlFlags.IS_TRANSIENT).ConfigureAwait(false);
        }
        finally
        {
            InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                    .Return<Control>(pong);
        }
    }
}