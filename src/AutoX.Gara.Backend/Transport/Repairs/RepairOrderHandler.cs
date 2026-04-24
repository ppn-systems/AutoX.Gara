using AutoX.Gara.Application.Inventory;
using AutoX.Gara.Backend.Transport.Common;
// Copyright (c) 2026 PPN Corporation. All rights reserved.




using AutoX.Gara.Domain.Entities.Invoices;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Protocol.Invoices;
using Nalix.Common.Networking;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security;
using Nalix.Framework.DataFrames.Pooling;
using System;
using System.Threading.Tasks;


namespace AutoX.Gara.Backend.Transport.Repairs;

/// <summary>
/// Packet Handler for repair order related operations.
/// </summary>
[PacketController]
public sealed class RepairOrderHandler(RepairOrderAppService repairOrderService)
{
    private readonly RepairOrderAppService _repairOrderService = repairOrderService ?? throw new ArgumentNullException(nameof(repairOrderService));

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.REPAIR_ORDER_GET)]
    public async ValueTask GetAsync(IPacketContext<RepairOrderQueryRequest> context)
    {
        RepairOrderQueryRequest packet = context.Packet;
        IConnection connection = context.Connection;

        var query = new RepairOrderListQuery(packet.Page, packet.PageSize, packet.SearchTerm, packet.SortBy, packet.SortDescending,

            packet.FilterCustomerId <= 0 ? null : packet.FilterCustomerId, packet.FilterVehicleId <= 0 ? null : packet.FilterVehicleId,

            packet.FilterInvoiceId <= 0 ? null : packet.FilterInvoiceId, packet.FilterStatus, packet.FilterFromDate, packet.FilterToDate);

        var result = await _repairOrderService.GetPageAsync(query).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        using var lease = PacketPool<RepairOrderQueryResponse>.Rent();
        var response = lease.Value;
        response.TotalCount = result.Data.totalCount;
        response.SequenceId = packet.SequenceId;
        response.RepairOrders = result.Data.items.ConvertAll(o => MapToPacket(o, 0));

        await connection.TCP.SendAsync(response).ConfigureAwait(false);

    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.REPAIR_ORDER_CREATE)]
    public async ValueTask CreateAsync(IPacketContext<RepairOrderDto> context)
    {
        RepairOrderDto packet = context.Packet;
        IConnection connection = context.Connection;

        if (packet.VehicleId <= 0 || packet.CustomerId <= 0)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }

        var order = new RepairOrder
        {
            CustomerId = packet.CustomerId,
            VehicleId = packet.VehicleId,
            EmployeeId = packet.EmployeeId,
            OrderDate = packet.OrderDate == default ? DateTime.UtcNow : packet.OrderDate,
            Status = packet.Status,
            Priority = packet.OrderPriority,
            Description = packet.Description ?? string.Empty,
            ExpectedCompletionDate = packet.ExpectedCompletionDate
        };

        var result = await _repairOrderService.CreateAsync(order).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        await connection.TCP.SendAsync(MapToPacket(result.Data!, packet.SequenceId)).ConfigureAwait(false);

    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.REPAIR_ORDER_UPDATE)]
    public async ValueTask UpdateAsync(IPacketContext<RepairOrderDto> context)
    {
        RepairOrderDto packet = context.Packet;
        IConnection connection = context.Connection;

        if (packet.RepairOrderId == null)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }

        var order = new RepairOrder
        {
            Id = packet.RepairOrderId.Value,
            CustomerId = packet.CustomerId,
            VehicleId = packet.VehicleId,
            EmployeeId = packet.EmployeeId,
            Status = packet.Status,
            Priority = packet.OrderPriority,
            Description = packet.Description ?? string.Empty,
            ExpectedCompletionDate = packet.ExpectedCompletionDate,
            CompletionDate = packet.CompletionDate
        };

        var result = await _repairOrderService.UpdateAsync(order).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        await connection.TCP.SendAsync(MapToPacket(result.Data!, packet.SequenceId)).ConfigureAwait(false);

    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((ushort)OpCommand.REPAIR_ORDER_DELETE)]
    public async ValueTask DeleteAsync(IPacketContext<RepairOrderDto> context)
    {
        RepairOrderDto packet = context.Packet;
        IConnection connection = context.Connection;

        if (packet.RepairOrderId == null)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }

        var result = await _repairOrderService.DeleteAsync(packet.RepairOrderId.Value).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        await context.OkAsync().ConfigureAwait(false);

    }

    private static RepairOrderDto MapToPacket(RepairOrder o, ushort sequenceId) => new()
    {
        SequenceId = sequenceId,
        RepairOrderId = o.Id,
        CustomerId = o.CustomerId,
        VehicleId = o.VehicleId,
        EmployeeId = o.EmployeeId,
        OrderDate = o.OrderDate,
        Status = o.Status,
        OrderPriority = o.Priority,
        Description = o.Description,
        TotalRepairCost = o.TotalRepairCost,
        ExpectedCompletionDate = o.ExpectedCompletionDate,
        CompletionDate = o.CompletionDate
    };





}



