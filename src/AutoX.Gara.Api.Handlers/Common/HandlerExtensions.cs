using Nalix.Common.Networking.Packets;
using Nalix.Common.Networking.Protocols;
using Nalix.Framework.DataFrames.Pooling;
using Nalix.Framework.DataFrames.SignalFrames;

namespace AutoX.Gara.Api.Handlers.Common;

public static class HandlerExtensions
{
    public static async ValueTask FailAsync<TPacket>(this IPacketContext<TPacket> context, ProtocolReason reason)
        where TPacket : class, IPacket
    {
        using var lease = PacketPool<Directive>.Rent();
        var directive = lease.Value;
        directive.Initialize(
            ControlType.ERROR,
            reason,
            ProtocolAdvice.DO_NOT_RETRY,
            context.Packet.SequenceId);

        await context.Connection.TCP.SendAsync(directive).ConfigureAwait(false);
    }

    public static async ValueTask OkAsync<TPacket>(this IPacketContext<TPacket> context)
        where TPacket : class, IPacket
    {
        using var lease = PacketPool<Directive>.Rent();
        var directive = lease.Value;
        directive.Initialize(
            ControlType.NONE,
            ProtocolReason.NONE,
            ProtocolAdvice.NONE,
            context.Packet.SequenceId);

        await context.Connection.TCP.SendAsync(directive).ConfigureAwait(false);
    }
}
