using AutoX.Gara.Shared.Enums;
using Nalix.Common.Networking.Protocols;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Services;
using AutoX.Gara.Domain.Entities.Repairs;
using Nalix.Common.Networking.Protocols;
using AutoX.Gara.Shared.Protocol.Repairs;
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

namespace AutoX.Gara.Api.Handlers.Repairs;

/// <summary>
/// Packet Handler for repair order item related operations.
/// </summary>
[PacketController]
public sealed class RepairOrderItemHandler(IRepairOrderItemAppService itemService)
{
    private readonly IRepairOrderItemAppService _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.REPAIR_ORDER_ITEM_GET)]
    public async ValueTask GetAsync(IPacketContext<RepairOrderItemDto> context)
    {
        RepairOrderItemDto packet = context.Packet;
        IConnection connection = context.Connection;

        if (packet.RepairOrderId <= 0)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }

        var result = await _itemService.GetByOrderIdAsync(packet.RepairOrderId).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        using var lease = PacketPool<RepairOrderItemQueryResponse>.Rent();
        var response = lease.Value;
        response.SequenceId = (ushort)packet.SequenceId;
        response.TotalCount = result.Data!.totalCount;
        response.RepairOrderItems = result.Data!.items.ConvertAll(i => MapToPacket(i, 0));

        await connection.TCP.SendAsync(response).ConfigureAwait(false);

    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.REPAIR_ORDER_ITEM_CREATE)]
    public async ValueTask CreateAsync(IPacketContext<RepairOrderItemDto> context)
    {
        RepairOrderItemDto packet = context.Packet;
        IConnection connection = context.Connection;

        if (packet.RepairOrderId <= 0)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }

        var item = new RepairOrderItem
        {
            RepairOrderId = packet.RepairOrderId,
            PartId = packet.PartId,
            Quantity = packet.Quantity
        };

        var result = await _itemService.CreateAsync(item).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        await connection.TCP.SendAsync(MapToPacket(result.Data!, packet.SequenceId)).ConfigureAwait(false);

    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.REPAIR_ORDER_ITEM_UPDATE)]
    public async ValueTask UpdateAsync(IPacketContext<RepairOrderItemDto> context)
    {
        RepairOrderItemDto packet = context.Packet;
        IConnection connection = context.Connection;

        if (packet.RepairOrderItemId == null)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }

        var item = new RepairOrderItem
        {
            Id = packet.RepairOrderItemId.Value,
            Quantity = packet.Quantity
        };

        var result = await _itemService.UpdateAsync(item).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        await connection.TCP.SendAsync(MapToPacket(result.Data!, packet.SequenceId)).ConfigureAwait(false);

    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.REPAIR_ORDER_ITEM_DELETE)]
    public async ValueTask DeleteAsync(IPacketContext<RepairOrderItemDto> context)
    {
        RepairOrderItemDto packet = context.Packet;
        IConnection connection = context.Connection;

        if (packet.RepairOrderItemId == null)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }

        var result = await _itemService.DeleteAsync(packet.RepairOrderItemId.Value).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        await context.OkAsync().ConfigureAwait(false);

    }

    private static RepairOrderItemDto MapToPacket(RepairOrderItem i, uint sequenceId) => new()
    {
        SequenceId = sequenceId,
        RepairOrderItemId = i.Id,
        RepairOrderId = i.RepairOrderId,
        PartId = i.PartId,
        Quantity = i.Quantity
    };

    
}