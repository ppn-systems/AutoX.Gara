using AutoX.Gara.Shared.Packets;
using Nalix.Common.Diagnostics;
using Nalix.Common.Messaging.Packets.Abstractions;
using Nalix.Framework.Injection;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Logging;
using Nalix.Shared.Messaging.Catalog;

namespace AutoX.Gara.Backend;

public static class AppConfig
{
    /// <summary>
    /// Initializes client components. Call this once at startup.
    /// </summary>
    public static void Load()
    {
        InstanceManager.Instance.Register<ILogger>(NLogix.Host.Instance);

        // 1) Build packet catalog.
        PacketCatalogFactory factory = new();

        // REGISTER packets here (single source of truth).
        _ = factory.RegisterPacket<AccountPacket>();

        _ = factory.RegisterPacket<CustomerPacket>();
        _ = factory.RegisterPacket<CustomerListPacket>();
        _ = factory.RegisterPacket<CustomerListRequestPacket>();

        IPacketCatalog catalog = factory.CreateCatalog();

        // 2) Expose catalog through your current service locator.
        InstanceManager.Instance.Register<IPacketCatalog>(catalog);
    }
}
