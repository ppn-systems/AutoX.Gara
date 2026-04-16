using AutoX.Gara.Shared.Enums;
using Nalix.Common.Networking.Protocols;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Services;
using AutoX.Gara.Domain.Entities.Billings;
using Nalix.Common.Networking.Protocols;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Protocol.Billings;
using Microsoft.Extensions.Logging;
using Nalix.Common.Networking;
using Nalix.Common.Networking.Packets;
using AutoX.Gara.Api.Handlers.Common;
using Nalix.Framework.DataFrames.SignalFrames;
using Nalix.Framework.DataFrames.Pooling;
using Nalix.Common.Security;
using Nalix.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoX.Gara.Api.Handlers.Financial;

/// <summary>
/// Packet Handler for service item related operations.
/// </summary>
[PacketController]
public sealed class ServiceItemHandler(IServiceItemAppService serviceItemService)
{
    private readonly IServiceItemAppService _serviceItemService = serviceItemService ?? throw new ArgumentNullException(nameof(serviceItemService));

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.SERVICE_ITEM_GET)]
    public async ValueTask GetAsync(IPacketContext<ServiceItemQueryRequest> context)
    {
        ServiceItemQueryRequest packet = context.Packet;
        IConnection connection = context.Connection;

        var query = new ServiceItemListQuery(
            packet.Page, 
            packet.PageSize, 
            packet.SearchTerm, 
            packet.SortBy, 
            packet.SortDescending, 
            packet.FilterType > 0 ? packet.FilterType : null,
            null,
            null
        );
        var result = await _serviceItemService.GetPageAsync(query).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        using var lease = PacketPool<ServiceItemQueryResponse>.Rent();
        var response = lease.Value;
        response.TotalCount = result.Data!.totalCount;
        response.SequenceId = packet.SequenceId;
        response.ServiceItems = result.Data.items.ConvertAll(i => MapToPacket(i, 0));

        await connection.TCP.SendAsync(response).ConfigureAwait(false);

    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.SERVICE_ITEM_CREATE)]
    public async ValueTask CreateAsync(IPacketContext<ServiceItemDto> context)
    {
        ServiceItemDto packet = context.Packet;
        IConnection connection = context.Connection;

        var item = new ServiceItem
        {
            Type = packet.Type,
            UnitPrice = packet.UnitPrice,
            Description = packet.Description ?? string.Empty
        };

        var result = await _serviceItemService.CreateAsync(item).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        await connection.TCP.SendAsync(MapToPacket(result.Data!, packet.SequenceId)).ConfigureAwait(false);

    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.SERVICE_ITEM_UPDATE)]
    public async ValueTask UpdateAsync(IPacketContext<ServiceItemDto> context)
    {
        ServiceItemDto packet = context.Packet;
        IConnection connection = context.Connection;

        if (packet.ServiceItemId == null)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }

        var item = new ServiceItem
        {
            Id = packet.ServiceItemId.Value,
            Type = packet.Type,
            UnitPrice = packet.UnitPrice,
            Description = packet.Description ?? string.Empty
        };

        var result = await _serviceItemService.UpdateAsync(item).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        await connection.TCP.SendAsync(MapToPacket(result.Data!, packet.SequenceId)).ConfigureAwait(false);

    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((ushort)OpCommand.SERVICE_ITEM_DELETE)]
    public async ValueTask DeleteAsync(IPacketContext<ServiceItemDto> context)
    {
        ServiceItemDto packet = context.Packet;
        IConnection connection = context.Connection;

        if (packet.ServiceItemId == null)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }

        var result = await _serviceItemService.DeleteAsync(packet.ServiceItemId.Value).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        await context.OkAsync().ConfigureAwait(false);

    }

    private static ServiceItemDto MapToPacket(ServiceItem i, uint sequenceId) => new()
    {
        SequenceId = sequenceId,
        ServiceItemId = i.Id,
        Type = i.Type,
        UnitPrice = i.UnitPrice,
        Description = i.Description
    };

    
}