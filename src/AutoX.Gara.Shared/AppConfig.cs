// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Packets.Auth;
using AutoX.Gara.Shared.Packets.Customers;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Framework.Injection;
using Nalix.Shared.Registry;

namespace AutoX.Gara.Shared;

public static class AppConfig
{
    /// <summary>
    /// Initializes client components. Call this once at startup.
    /// </summary>
    public static void Register()
    {
        // 1) Build packet catalog.
        PacketRegistryFactory factory = new();

        // REGISTER packets here (single source of truth).
        _ = factory.RegisterPacket<LoginPacket>();

        _ = factory.RegisterPacket<CustomersPacket>();
        _ = factory.RegisterPacket<CustomerDataPacket>();
        _ = factory.RegisterPacket<CustomersQueryPacket>();

        PacketRegistry catalog = factory.CreateCatalog();

        // 2) Expose catalog through your current service locator.
        InstanceManager.Instance.Register<IPacketCatalog>(catalog);
    }
}
