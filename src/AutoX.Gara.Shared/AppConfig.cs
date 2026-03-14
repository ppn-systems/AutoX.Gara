// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Auth;
using AutoX.Gara.Shared.Protocol.Billings;
using AutoX.Gara.Shared.Protocol.Customers;
using AutoX.Gara.Shared.Protocol.Employees;
using AutoX.Gara.Shared.Protocol.Inventory;
using AutoX.Gara.Shared.Protocol.Suppliers;
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

        _ = factory.RegisterPacket<EmployeeDto>();
        _ = factory.RegisterPacket<EmployeeQueryRequest>();
        _ = factory.RegisterPacket<EmployeeQueryResponse>();

        _ = factory.RegisterPacket<CustomerDto>();
        _ = factory.RegisterPacket<CustomerQueryResponse>();
        _ = factory.RegisterPacket<CustomerQueryRequest>();

        _ = factory.RegisterPacket<VehicleDto>();
        _ = factory.RegisterPacket<VehiclesQueryResponse>();

        _ = factory.RegisterPacket<SupplierDto>();
        _ = factory.RegisterPacket<SupplierQueryRequest>();
        _ = factory.RegisterPacket<SupplierQueryResponse>();

        _ = factory.RegisterPacket<PartDto>();
        _ = factory.RegisterPacket<PartQueryRequest>();
        _ = factory.RegisterPacket<PartQueryResponse>();

        _ = factory.RegisterPacket<InvoiceDto>();
        _ = factory.RegisterPacket<InvoiceQueryRequest>();
        _ = factory.RegisterPacket<InvoiceQueryResponse>();

        _ = factory.RegisterPacket<RepairOrderDto>();
        _ = factory.RegisterPacket<RepairOrderQueryRequest>();
        _ = factory.RegisterPacket<RepairOrderQueryResponse>();

        _ = factory.RegisterPacket<RepairOrderItemDto>();
        _ = factory.RegisterPacket<RepairOrderItemQueryRequest>();
        _ = factory.RegisterPacket<RepairOrderItemQueryResponse>();

        _ = factory.RegisterPacket<RepairTaskDto>();
        _ = factory.RegisterPacket<RepairTaskQueryRequest>();
        _ = factory.RegisterPacket<RepairTaskQueryResponse>();

        _ = factory.RegisterPacket<ServiceItemDto>();
        _ = factory.RegisterPacket<ServiceItemQueryRequest>();
        _ = factory.RegisterPacket<ServiceItemQueryResponse>();

        _ = factory.RegisterPacket<TransactionDto>();
        _ = factory.RegisterPacket<TransactionQueryRequest>();
        _ = factory.RegisterPacket<TransactionQueryResponse>();

        _ = factory.RegisterPacket<EmployeeSalaryDto>();
        _ = factory.RegisterPacket<EmployeeSalaryQueryRequest>();
        _ = factory.RegisterPacket<EmployeeSalaryQueryResponse>();

        PacketRegistry catalog = factory.CreateCatalog();

        // 2) Expose catalog through your current service locator.
        InstanceManager.Instance.Register<IPacketRegistry>(catalog);
    }
}
