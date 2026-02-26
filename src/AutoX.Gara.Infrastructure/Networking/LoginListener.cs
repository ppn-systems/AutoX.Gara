// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Network.Abstractions;
using Nalix.Network.Listeners.Tcp;

namespace AutoX.Gara.Infrastructure.Networking;

/// <summary>
/// Represents a specialized TCP listener for AutoX application.
/// Inherits from <see cref="TcpListenerBase"/> and provides protocol handling.
/// </summary>
public sealed class LoginListener : TcpListenerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginListener"/> class with the specified protocol handler.
    /// </summary>
    /// <param name="protocol">The protocol handler used for processing incoming connections.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public LoginListener(IProtocol protocol) : base(protocol)
    {
    }

    // You can override methods or add additional behaviors here
    // For example, override Dispose or add custom events.
}