// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Common.Connection;
using Nalix.Common.Diagnostics;
using Nalix.Common.Infrastructure.Connection;
using Nalix.Framework.Injection;
using Nalix.Network.Abstractions;
using Nalix.Network.Connections;
using Nalix.Network.Protocols;
using System.Threading;

namespace AutoX.Gara.Infrastructure.Networking;

/// <summary>
/// Represents a custom protocol handler for the AutoX application.
/// Implements specific logic for inbound message processing and connection validation.
/// </summary>
public sealed class AutoXProtocol : Protocol
{
    private readonly IPacketDispatch s_Dispatch;

    /// <inheritdoc/>
    public AutoXProtocol(IPacketDispatch dispatch) : base()
    {
        s_Dispatch = dispatch;
        IsAccepting = true; // Enable accepting connections by default
        KeepConnectionOpen = true;
    }

    public override void OnAccept(IConnection connection, CancellationToken cancellationToken = default)
    {
        base.OnAccept(connection, cancellationToken);

        InstanceManager.Instance.GetExistingInstance<ConnectionHub>()?
                                .RegisterConnection(connection);
    }

    /// <summary>
    /// Processes a received message from an active connection.
    /// Handles application-specific parsing and validation logic.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="args">Event arguments containing the connection and message information.</param>
    public override void ProcessMessage(System.Object sender, IConnectEventArgs args)
    {
        // Validate arguments and protocol state
        System.ArgumentNullException.ThrowIfNull(args);

        // TODO: Parse message and implement business logic here

        InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                .Debug($"[AutoX.{nameof(AutoXProtocol)}:{nameof(ProcessMessage)}] Received message on connection id={args.Connection.ID}");

        s_Dispatch.HandlePacket(args.Connection.IncomingPacket, args.Connection);
    }

    /// <summary>
    /// Validates an incoming connection prior to accepting.
    /// Override to implement custom validation (e.g., IP filter, authentication handshake).
    /// </summary>
    /// <param name="connection">The connection to validate.</param>
    /// <returns>True if accepted; false otherwise.</returns>
    protected override System.Boolean ValidateConnection(IConnection connection)
    {
        // TODO: Add custom validation logic, e.g. check IP, token, etc.
        InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                .Debug($"[AutoX.{nameof(AutoXProtocol)}:{nameof(ValidateConnection)}] Validating connection id={connection.ID}");

        return true; // Accept all for now
    }

    /// <summary>
    /// Custom post-processing after a message is handled.
    /// Called from base.PostProcessMessage().
    /// </summary>
    /// <param name="args">Event arguments containing connection and processing results.</param>
    protected override void OnPostProcess(IConnectEventArgs args)
    {
        var stack = System.Environment.StackTrace;
        InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                .Debug($"[AutoX.{nameof(AutoXProtocol)}:{nameof(OnPostProcess)}] Post-processing connection id={args.Connection.ID} st={stack}");

        // TODO: Add post-processing logic if needed (audit, cleanup, stats)
    }

    /// <summary>
    /// Handles protocol-level errors occurring on a connection.
    /// Increments error count and logs details.
    /// </summary>
    /// <param name="connection">The connection where the error occurred.</param>
    /// <param name="exception">The exception thrown.</param>
    protected override void OnConnectionError(IConnection connection, System.Exception exception)
    {
        base.OnConnectionError(connection, exception);
        InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                .Error($"[AutoX.{nameof(AutoXProtocol)}:{nameof(OnConnectionError)}] Protocol error id={connection.ID}: {exception.Message}");
    }
}