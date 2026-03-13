// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Inventory;
using AutoX.Gara.Domain.Enums.Parts;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Infrastructure.Repositories;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Protocol.Inventory;
using Nalix.Common.Networking.Abstractions;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Attributes;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security.Enums;
using Nalix.Framework.Injection;
using Nalix.Network.Connections;
using Nalix.Shared.Memory.Pooling;
using Nalix.Shared.Serialization;

namespace AutoX.Gara.Application.Inventory;

/// <summary>
/// Packet controller xử lý tất cả nghiệp vụ CRUD cho <c>SparePart</c> (phụ tùng bán).
/// <para>
/// Tuân theo cùng pattern với <c>CustomerOps</c>:
/// <list type="bullet">
///   <item>Inject factory, tạo repository từ context cho từng request.</item>
///   <item>Translate packet → <see cref="SparePartListQuery"/> value object trước khi query.</item>
///   <item>Mapping tập trung tại <see cref="MapToPacket"/>.</item>
/// </list>
/// </para>
/// </summary>
[PacketController]
public sealed class SparePartOps(AutoXDbContextFactory dbContextFactory)
{
    private readonly AutoXDbContextFactory _dbContextFactory = dbContextFactory
        ?? throw new System.ArgumentNullException(nameof(dbContextFactory));

    // ─── GET LIST ─────────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.SPARE_PART_GET)]
    public async System.Threading.Tasks.Task GetAsync(IPacket p, IConnection connection)
    {
        if (p is not SparePartQueryRequest packet)
        {
            System.UInt32 fallbackSeq = p is IPacketSequenced ps ? ps.SequenceId : 0;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);
            return;
        }

        SparePartQueryResponse response = null;

        try
        {
            SparePartListQuery query = new(
                Page: packet.Page,
                PageSize: packet.PageSize,
                SearchTerm: packet.SearchTerm,
                SortBy: packet.SortBy,
                SortDescending: packet.SortDescending,
                FilterSupplierId: packet.FilterSupplierId == 0 ? null : packet.FilterSupplierId,
                FilterCategory: packet.FilterCategory,
                FilterDiscontinued: packet.FilterDiscontinued);

            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var spareParts = new SparePartRepository(db);

            (System.Collections.Generic.List<SparePart> items, System.Int32 totalCount)
                = await spareParts.GetPageAsync(query).ConfigureAwait(false);

            response = new()
            {
                TotalCount = totalCount,
                SequenceId = packet.SequenceId,
                Parts = items.ConvertAll(s => MapToPacket(s, sequenceId: 0))
            };

            System.Boolean sent = await connection.TCP
                .SendAsync(LiteSerializer.Serialize(response)).ConfigureAwait(false);

            if (!sent)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
            }
        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.RETRY, packet.SequenceId).ConfigureAwait(false);
        }
        finally
        {
            if (response is not null)
            {
                var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
                foreach (SparePartDto dto in response.Parts)
                {
                    pool.Return(dto);
                }
            }
        }
    }

    // ─── CREATE ───────────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.SPARE_PART_CREATE)]
    public async System.Threading.Tasks.Task CreateAsync(IPacket p, IConnection connection)
    {
        if (p is not SparePartDto packet || packet.SupplierId <= 0 ||
            System.String.IsNullOrWhiteSpace(packet.PartName))
        {
            System.UInt32 fallbackSeq = p is IPacketSequenced ps ? ps.SequenceId : 0;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);

            return;
        }

        if (packet.SellingPrice < packet.PurchasePrice)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);

            return;
        }

        SparePartDto confirmed = null;

        try
        {
            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var spareParts = new SparePartRepository(db);

            System.Boolean existed = await spareParts
                .ExistsByNameAndSupplierAsync(packet.PartName, packet.SupplierId)
                .ConfigureAwait(false);

            if (existed)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.ALREADY_EXISTS,
                    ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);

                return;
            }

            SparePart newPart = new()
            {
                SupplierId = packet.SupplierId,
                PartName = packet.PartName,
                PartCategory = packet.PartCategory ?? PartCategory.Other,
                PurchasePrice = packet.PurchasePrice,
                SellingPrice = packet.SellingPrice,
                InventoryQuantity = packet.InventoryQuantity,
                IsDiscontinued = false
            };

            await spareParts.AddAsync(newPart).ConfigureAwait(false);
            await spareParts.SaveChangesAsync().ConfigureAwait(false);

            confirmed = MapToPacket(newPart, packet.SequenceId);
            System.Boolean sent = await connection.TCP
                .SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);

            if (!sent)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
            }
        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
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

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.SPARE_PART_UPDATE)]
    public async System.Threading.Tasks.Task UpdateAsync(IPacket p, IConnection connection)
    {
        if (p is not SparePartDto packet || packet.SparePartId is null ||
            System.String.IsNullOrWhiteSpace(packet.PartName))
        {
            System.UInt32 fallbackSeq = p is IPacketSequenced ps ? ps.SequenceId : 0;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);

            return;
        }

        if (packet.SellingPrice < packet.PurchasePrice)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);

            return;
        }

        SparePartDto confirmed = null;

        try
        {
            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var spareParts = new SparePartRepository(db);

            SparePart existing = await spareParts
                .GetByIdAsync(packet.SparePartId.Value).ConfigureAwait(false);

            if (existing is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);

                return;
            }

            existing.PartName = packet.PartName;
            existing.PartCategory = packet.PartCategory ?? existing.PartCategory;
            existing.PurchasePrice = packet.PurchasePrice;
            existing.SellingPrice = packet.SellingPrice;
            existing.InventoryQuantity = packet.InventoryQuantity;
            existing.IsDiscontinued = packet.IsDiscontinued;

            spareParts.Update(existing);
            await spareParts.SaveChangesAsync().ConfigureAwait(false);

            confirmed = MapToPacket(existing, packet.SequenceId);
            System.Boolean sent = await connection.TCP
                .SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);

            if (!sent)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
            }
        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
        }
        finally
        {
            if (confirmed is not null)
            {
                InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Return(confirmed);
            }
        }
    }

    // ─── DISCONTINUE (Soft Delete) ────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((System.UInt16)OpCommand.SPARE_PART_DELETE)]
    public async System.Threading.Tasks.Task DiscontinueAsync(IPacket p, IConnection connection)
    {
        if (p is not SparePartDto packet || packet.SparePartId is null)
        {
            System.UInt32 fallbackSeq = p is IPacketSequenced ps ? ps.SequenceId : 0;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);

            return;
        }

        try
        {
            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var spareParts = new SparePartRepository(db);

            SparePart existing = await spareParts
                .GetByIdAsync(packet.SparePartId.Value).ConfigureAwait(false);

            if (existing is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);

                return;
            }

            existing.IsDiscontinued = true;

            spareParts.Update(existing);
            await spareParts.SaveChangesAsync().ConfigureAwait(false);

            await connection.SendAsync(
                ControlType.NONE,
                ProtocolReason.NONE,
                ProtocolAdvice.NONE, packet.SequenceId).ConfigureAwait(false);
        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
        }
    }

    // ─── Private Helpers ─────────────────────────────────────────────────────

    private static SparePartDto MapToPacket(SparePart s, System.UInt32 sequenceId)
    {
        SparePartDto dto = InstanceManager.Instance
            .GetOrCreateInstance<ObjectPoolManager>()
            .Get<SparePartDto>();

        dto.SequenceId = sequenceId;
        dto.SparePartId = s.Id;
        dto.SupplierId = s.SupplierId;
        dto.PartName = s.PartName;
        dto.PartCategory = s.PartCategory;
        dto.PurchasePrice = s.PurchasePrice;
        dto.SellingPrice = s.SellingPrice;
        dto.InventoryQuantity = s.InventoryQuantity;
        dto.IsDiscontinued = s.IsDiscontinued;

        return dto;
    }
}