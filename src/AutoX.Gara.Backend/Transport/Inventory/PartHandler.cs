using AutoX.Gara.Application.Inventory;
using AutoX.Gara.Backend.Transport.Common;
// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Entities.Inventory;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Models;
using AutoX.Gara.Contracts.Inventory;
using Nalix.Common.Networking;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security;
using Nalix.Framework.DataFrames.Pooling;
using Nalix.Framework.Injection;
using Nalix.Framework.Memory.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace AutoX.Gara.Backend.Transport.Inventory;
/// <summary>
/// Packet Handler for inventory part related operations.
/// </summary>
[PacketController]
public sealed class PartHandler(PartAppService partService)
{
    private readonly PartAppService _partService = partService ?? throw new ArgumentNullException(nameof(partService));
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.PART_GET)]
    public async ValueTask GetAsync(IPacketContext<PartQueryRequest> context)
    {
        PartQueryRequest packet = context.Packet;
        IConnection connection = context.Connection;
        var query = new PartListQuery(packet.Page, packet.PageSize, packet.SearchTerm, packet.SortBy, packet.SortDescending,
            packet.FilterSupplierId == 0 ? null : packet.FilterSupplierId, packet.FilterCategory, packet.FilterInStock,
            packet.FilterDefective, packet.FilterExpired, packet.FilterDiscontinued);
        var result = await _partService.GetPageAsync(query).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }
        using var lease = PacketPool<PartQueryResponse>.Rent();
        var response = lease.Value;
        response.TotalCount = result.Data!.totalCount;
        response.SequenceId = packet.SequenceId;
        response.Parts = result.Data.items.ConvertAll(pt => MapToPacket(pt, 0));
        try
        {
            await connection.TCP.SendAsync(response).ConfigureAwait(false);
        }
        finally
        {
            ReturnDtos(response.Parts);
        }
    }
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.PART_CREATE)]
    public async ValueTask CreateAsync(IPacketContext<PartDto> context)
    {
        PartDto packet = context.Packet;
        IConnection connection = context.Connection;
        if (packet.SupplierId <= 0 || string.IsNullOrWhiteSpace(packet.PartCode))
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }
        var part = new Part
        {
            SupplierId = packet.SupplierId,
            PartCode = packet.PartCode,
            PartName = packet.PartName,
            Manufacturer = packet.Manufacturer ?? string.Empty,
            PartCategory = packet.PartCategory ?? AutoX.Gara.Domain.Enums.Parts.PartCategory.Other,
            PurchasePrice = packet.PurchasePrice,
            SellingPrice = packet.SellingPrice,
            InventoryQuantity = packet.InventoryQuantity,
            DateAdded = packet.DateAdded,
            ExpiryDate = packet.ExpiryDate
        };
        var result = await _partService.CreateAsync(part).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }
        var confirmed = MapToPacket(result.Data!, packet.SequenceId);
        try
        {
            await connection.TCP.SendAsync(confirmed).ConfigureAwait(false);
        }
        finally
        {
            ReturnToPool(confirmed);
        }
    }
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.PART_UPDATE)]
    public async ValueTask UpdateAsync(IPacketContext<PartDto> context)
    {
        PartDto packet = context.Packet;
        IConnection connection = context.Connection;
        if (packet.PartId == null)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }
        var part = new Part
        {
            Id = packet.PartId.Value,
            PartName = packet.PartName,
            Manufacturer = packet.Manufacturer ?? string.Empty,
            PartCategory = packet.PartCategory ?? AutoX.Gara.Domain.Enums.Parts.PartCategory.Other,
            PurchasePrice = packet.PurchasePrice,
            SellingPrice = packet.SellingPrice,
            InventoryQuantity = packet.InventoryQuantity,
            DateAdded = packet.DateAdded,
            ExpiryDate = packet.ExpiryDate,
            IsDiscontinued = packet.IsDiscontinued,
            IsDefective = packet.IsDefective
        };
        var result = await _partService.UpdateAsync(part).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }
        var confirmed = MapToPacket(result.Data!, packet.SequenceId);
        try
        {
            await connection.TCP.SendAsync(confirmed).ConfigureAwait(false);
        }
        finally
        {
            ReturnToPool(confirmed);
        }
    }
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((ushort)OpCommand.PART_DELETE)]
    public async ValueTask DeleteAsync(IPacketContext<PartDto> context)
    {
        PartDto packet = context.Packet;
        IConnection connection = context.Connection;
        if (packet.PartId == null)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }
        var result = await _partService.DeleteAsync(packet.PartId.Value).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }
        await context.OkAsync().ConfigureAwait(false);
    }
    private static PartDto MapToPacket(Part pt, ushort sequenceId)
    {
        var dto = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Get<PartDto>();
        dto.SequenceId = sequenceId;
        dto.PartId = pt.Id;
        dto.SupplierId = pt.SupplierId;
        dto.PartCode = pt.PartCode;
        dto.PartName = pt.PartName;
        dto.Manufacturer = pt.Manufacturer;
        dto.PartCategory = pt.PartCategory;
        dto.PurchasePrice = pt.PurchasePrice;
        dto.SellingPrice = pt.SellingPrice;
        dto.InventoryQuantity = pt.InventoryQuantity;
        dto.IsDefective = pt.IsDefective;
        dto.IsDiscontinued = pt.IsDiscontinued;
        dto.DateAdded = pt.DateAdded;
        dto.ExpiryDate = pt.ExpiryDate;
        return dto;
    }
    private static void ReturnDtos(IEnumerable<PartDto> dtos)
    {
        if (dtos == null)
        {
            return;
        }
        var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
        foreach (var dto in dtos)
        {
            pool.Return(dto);
        }
    }
    private static void ReturnToPool(PartDto dto)
    {
        if (dto != null)
        {
            InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Return(dto);
        }
    }
}


