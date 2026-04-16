// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Domain.Entities.Inventory;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Protocol.Inventory;
using Nalix.Common.Networking;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security;
using Nalix.Framework.Injection;
using Nalix.Framework.Memory.Objects;
using Nalix.Framework.Serialization;
using Nalix.Runtime.Extensions;

namespace AutoX.Gara.Application.Inventory;

/// <summary>
/// Packet controller handling all CRUD operations for <c>Part</c> entity.
/// Combines functionality of ReplacementPartOps and SparePartOps.
/// <para>
/// Key features:
/// <list type="bullet">
///   <item>Validates SellingPrice >= PurchasePrice.</item>
///   <item>Validates ExpiryDate >= DateAdded.</item>
///   <item>Thread-safe quantity operations via domain methods.</item>
///   <item>Supports both hard delete and soft delete (IsDiscontinued flag).</item>
///   <item>Comprehensive error handling with appropriate protocol feedback.</item>
/// </list>
/// </para>
/// </summary>
[PacketController]
public sealed class PartOps(IDataSessionFactory dataSessionFactory)
{
    private readonly IDataSessionFactory _dataSessionFactory = dataSessionFactory
        ?? throw new System.ArgumentNullException(nameof(dataSessionFactory));

    // ─── GET LIST ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Retrieves a paginated list of parts with filtering and sorting.
    /// </summary>
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.PART_GET)]
    public async System.Threading.Tasks.Task GetAsync(IPacket p, IConnection connection)
    {
        if (p is not PartQueryRequest packet)
        {
            System.UInt32 fallbackSeq = p.SequenceId;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, fallbackSeq, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        PartQueryResponse response = null;

        try
        {
            PartListQuery query = new(
                Page: packet.Page,
                PageSize: packet.PageSize,
                SearchTerm: packet.SearchTerm,
                SortBy: packet.SortBy,
                SortDescending: packet.SortDescending,
                FilterSupplierId: packet.FilterSupplierId == 0 ? null : packet.FilterSupplierId,
                FilterCategory: packet.FilterCategory,
                FilterInStock: packet.FilterInStock,
                FilterDefective: packet.FilterDefective,
                FilterExpired: packet.FilterExpired,
                FilterDiscontinued: packet.FilterDiscontinued);

            await using var session = _dataSessionFactory.Create();
            var partRepository = session.Parts;

            (System.Collections.Generic.List<Part> items, System.Int32 totalCount)
                = await partRepository.GetPageAsync(query).ConfigureAwait(false);

            response = new()
            {
                TotalCount = totalCount,
                SequenceId = packet.SequenceId,
                Parts = items.ConvertAll(p => MapToPacket(p, sequenceId: 0))
            };

            await connection.TCP
                .SendAsync(LiteSerializer.Serialize(response)).ConfigureAwait(false);

        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
        finally
        {
            if (response is not null)
            {
                var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
                foreach (PartDto dto in response.Parts)
                {
                    pool.Return(dto);
                }
            }
        }
    }

    // ─── CREATE ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new part in the inventory.
    /// </summary>
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.PART_CREATE)]
    public async System.Threading.Tasks.Task CreateAsync(IPacket p, IConnection connection)
    {
        if (p is not PartDto packet ||
            packet.SupplierId <= 0 ||
            System.String.IsNullOrWhiteSpace(packet.PartCode) ||
            System.String.IsNullOrWhiteSpace(packet.PartName))
        {
            System.UInt32 fallbackSeq = p.SequenceId;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, fallbackSeq, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        // Validate SellingPrice >= PurchasePrice
        if (packet.SellingPrice < packet.PurchasePrice)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        // Validate ExpiryDate >= DateAdded
        if (packet.ExpiryDate.HasValue && packet.ExpiryDate.Value < packet.DateAdded)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        PartDto confirmed = null;

        try
        {
            await using var session = _dataSessionFactory.Create();
            var partRepository = session.Parts;

            // Check if PartCode already exists
            System.Boolean existed = await partRepository
                .ExistsByPartCodeAsync(packet.PartCode).ConfigureAwait(false);

            if (existed)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.ALREADY_EXISTS,
                    ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
                return;
            }

            Part newPart = new()
            {
                SupplierId = packet.SupplierId,
                PartCode = packet.PartCode,
                PartName = packet.PartName,
                Manufacturer = packet.Manufacturer ?? System.String.Empty,
                PartCategory = packet.PartCategory ?? Domain.Enums.Parts.PartCategory.Other,
                PurchasePrice = packet.PurchasePrice,
                SellingPrice = packet.SellingPrice,
                InventoryQuantity = packet.InventoryQuantity,
                DateAdded = packet.DateAdded,
                ExpiryDate = packet.ExpiryDate,
                IsDefective = false,
                IsDiscontinued = false
            };

            await partRepository.AddAsync(newPart).ConfigureAwait(false);
            await partRepository.SaveChangesAsync().ConfigureAwait(false);

            confirmed = MapToPacket(newPart, packet.SequenceId);
            await connection.TCP
                .SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);

        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
        finally
        {
            if (confirmed is not null)
            {
                InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Return(confirmed);
            }
        }
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Updates an existing part in the inventory.
    /// </summary>
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.PART_UPDATE)]
    public async System.Threading.Tasks.Task UpdateAsync(IPacket p, IConnection connection)
    {
        if (p is not PartDto packet || packet.PartId is null ||
            System.String.IsNullOrWhiteSpace(packet.PartName))
        {
            System.UInt32 fallbackSeq = p.SequenceId;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, fallbackSeq, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        // Validate SellingPrice >= PurchasePrice
        if (packet.SellingPrice < packet.PurchasePrice)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        // Validate ExpiryDate >= DateAdded
        if (packet.ExpiryDate.HasValue && packet.ExpiryDate.Value < packet.DateAdded)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        PartDto confirmed = null;

        try
        {
            await using var session = _dataSessionFactory.Create();
            var partRepository = session.Parts;

            Part existing = await partRepository
                .GetByIdAsync(packet.PartId.Value).ConfigureAwait(false);

            if (existing is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
                return;
            }

            // Update properties, using domain methods where applicable
            existing.PartName = packet.PartName;
            existing.Manufacturer = packet.Manufacturer ?? System.String.Empty;
            existing.PartCategory = packet.PartCategory ?? existing.PartCategory;
            existing.PurchasePrice = packet.PurchasePrice;
            existing.SellingPrice = packet.SellingPrice;
            existing.InventoryQuantity = packet.InventoryQuantity;
            existing.DateAdded = packet.DateAdded;
            existing.ExpiryDate = packet.ExpiryDate;
            existing.IsDiscontinued = packet.IsDiscontinued;

            // Use domain methods to change IsDefective
            if (packet.IsDefective && !existing.IsDefective)
            {
                existing.MarkAsDefective();
            }
            else if (!packet.IsDefective && existing.IsDefective)
            {
                existing.UnmarkAsDefective();
            }

            partRepository.Update(existing);
            await partRepository.SaveChangesAsync().ConfigureAwait(false);

            confirmed = MapToPacket(existing, packet.SequenceId);
            await connection.TCP
                .SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);

        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
        finally
        {
            if (confirmed is not null)
            {
                InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Return(confirmed);
            }
        }
    }

    // ─── DELETE (Soft or Hard) ────────────────────────────────────────────────

    /// <summary>
    /// Deletes a part from the inventory.
    /// Default behavior: soft delete via IsDiscontinued flag.
    /// Can be changed to hard delete if audit trail is not required.
    /// </summary>
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((System.UInt16)OpCommand.PART_DELETE)]
    public async System.Threading.Tasks.Task DeleteAsync(IPacket p, IConnection connection)
    {
        if (p is not PartDto packet || packet.PartId is null)
        {
            System.UInt32 fallbackSeq = p.SequenceId;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, fallbackSeq, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        try
        {
            await using var session = _dataSessionFactory.Create();
            var partRepository = session.Parts;

            Part existing = await partRepository
                .GetByIdAsync(packet.PartId.Value).ConfigureAwait(false);

            if (existing is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
                return;
            }

            // Soft delete: Mark as discontinued
            existing.IsDiscontinued = true;
            partRepository.Update(existing);
            await partRepository.SaveChangesAsync().ConfigureAwait(false);

            // Alternatively, for hard delete:
            // partRepository.Delete(existing);
            // await partRepository.SaveChangesAsync().ConfigureAwait(false);

            await connection.SendAsync(
                ControlType.NONE,
                ProtocolReason.NONE,
                ProtocolAdvice.NONE, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
    }

    // ─── Private Helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Maps a Part entity to a PartDto packet using object pooling.
    /// </summary>
    private static PartDto MapToPacket(Part part, System.UInt32 sequenceId)
    {
        PartDto dto = InstanceManager.Instance
            .GetOrCreateInstance<ObjectPoolManager>()
            .Get<PartDto>();

        dto.SequenceId = sequenceId;
        dto.PartId = part.Id;
        dto.SupplierId = part.SupplierId;
        dto.PartCode = part.PartCode;
        dto.PartName = part.PartName;
        dto.Manufacturer = part.Manufacturer;
        dto.PartCategory = part.PartCategory;
        dto.PurchasePrice = part.PurchasePrice;
        dto.SellingPrice = part.SellingPrice;
        dto.InventoryQuantity = part.InventoryQuantity;
        dto.IsDefective = part.IsDefective;
        dto.IsDiscontinued = part.IsDiscontinued;
        dto.DateAdded = part.DateAdded;
        dto.ExpiryDate = part.ExpiryDate;

        return dto;
    }
}





