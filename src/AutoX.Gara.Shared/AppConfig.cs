// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Auth;
using AutoX.Gara.Shared.Protocol.Customers;
using AutoX.Gara.Shared.Protocol.Vehicles;
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

        _ = factory.RegisterPacket<CustomerDto>();
        _ = factory.RegisterPacket<CustomerQueryResponse>();
        _ = factory.RegisterPacket<CustomerQueryRequest>();

        _ = factory.RegisterPacket<VehicleDto>();
        _ = factory.RegisterPacket<VehiclesQueryResponse>();

        PacketRegistry catalog = factory.CreateCatalog();

        // 2) Expose catalog through your current service locator.
        InstanceManager.Instance.Register<IPacketRegistry>(catalog);
    }
}
