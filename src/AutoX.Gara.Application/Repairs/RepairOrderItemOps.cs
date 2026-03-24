// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Domain.Entities.Repairs;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Infrastructure.Repositories;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Protocol.Repairs;
using Nalix.Common.Networking;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security;
using Nalix.Framework.Injection;
using Nalix.Network.Connections;
using Nalix.Shared.Memory.Objects;
using Nalix.Shared.Serialization;

namespace AutoX.Gara.Application.Repairs;

[PacketController]
public sealed class RepairOrderItemOps(AutoXDbContextFactory dbContextFactory)
{
    private readonly AutoXDbContextFactory _dbContextFactory = dbContextFactory
        ?? throw new System.ArgumentNullException(nameof(dbContextFactory));

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.REPAIR_ORDER_ITEM_GET)]
    public async System.Threading.Tasks.Task GetAsync(IPacket p, IConnection connection)
    {
        if (p is not RepairOrderItemQueryRequest packet)
        {
            System.UInt32 fallbackSeq = p is IPacketSequenced ps ? ps.SequenceId : 0;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);
            return;
        }

        RepairOrderItemQueryResponse response = null;

        try
        {
            RepairOrderItemListQuery query = new(
                Page: packet.Page,
                PageSize: packet.PageSize,
                SearchTerm: packet.SearchTerm,
                SortBy: packet.SortBy,
                SortDescending: packet.SortDescending,
                FilterRepairOrderId: packet.FilterRepairOrderId <= 0 ? null : packet.FilterRepairOrderId,
                FilterPartId: packet.FilterPartId <= 0 ? null : packet.FilterPartId);

            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var repo = new RepairOrderItemRepository(db);

            (System.Collections.Generic.List<RepairOrderItem> items, System.Int32 totalCount) =
                await repo.GetPageAsync(query).ConfigureAwait(false);

            response = new()
            {
                TotalCount = totalCount,
                SequenceId = packet.SequenceId,
                RepairOrderItems = items.ConvertAll(i => MapToPacket(i, sequenceId: 0))
            };

            System.Boolean sent = await connection.TCP.SendAsync(LiteSerializer.Serialize(response)).ConfigureAwait(false);
            if (!sent)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
            }
        }
        catch (System.ArgumentException)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.VALIDATION_FAILED,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);
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
            if (response?.RepairOrderItems != null)
            {
                var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
                foreach (RepairOrderItemDto dto in response.RepairOrderItems)
                {
                    pool.Return(dto);
                }
            }
        }
    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.REPAIR_ORDER_ITEM_CREATE)]
    public async System.Threading.Tasks.Task CreateAsync(IPacket p, IConnection connection)
    {
        if (!TryParseRepairOrderItemPacket(p, out RepairOrderItemDto packet, out System.UInt32 fallbackSeq) || packet.RepairOrderItemId is not null)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);
            return;
        }

        RepairOrderItemDto confirmed = null;
        try
        {
            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var repo = new RepairOrderItemRepository(db);
            var repairOrders = new RepairOrderRepository(db);
            var invoices = new InvoiceRepository(db);

            RepairOrderItem entity = new()
            {
                RepairOrderId = packet.RepairOrderId,
                PartId = packet.PartId,
                Quantity = packet.Quantity
            };

            await repo.AddAsync(entity).ConfigureAwait(false);
            await repo.SaveChangesAsync().ConfigureAwait(false);

            var ro = await repairOrders.GetByIdAsync(entity.RepairOrderId).ConfigureAwait(false);
            if (ro?.InvoiceId is not null)
            {
                Invoice inv = await invoices.GetByIdWithDetailsAsync(ro.InvoiceId.Value).ConfigureAwait(false);
                if (inv is not null)
                {
                    inv.Recalculate();
                    invoices.Update(inv);
                    await invoices.SaveChangesAsync().ConfigureAwait(false);
                }
            }

            confirmed = MapToPacket(entity, packet.SequenceId);
            System.Boolean sent = await connection.TCP.SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);
            if (!sent)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
            }
        }
        catch (System.ArgumentException)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.VALIDATION_FAILED,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);
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
            if (confirmed != null)
            {
                InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Return(confirmed);
            }
        }
    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.REPAIR_ORDER_ITEM_UPDATE)]
    public async System.Threading.Tasks.Task UpdateAsync(IPacket p, IConnection connection)
    {
        if (!TryParseRepairOrderItemPacket(p, out RepairOrderItemDto packet, out System.UInt32 fallbackSeq) || packet.RepairOrderItemId is null)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);
            return;
        }

        RepairOrderItemDto confirmed = null;
        try
        {
            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var repo = new RepairOrderItemRepository(db);
            var repairOrders = new RepairOrderRepository(db);
            var invoices = new InvoiceRepository(db);

            RepairOrderItem existing = await repo.GetByIdAsync(packet.RepairOrderItemId.Value).ConfigureAwait(false);
            if (existing is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
                return;
            }

            System.Int32 oldRepairOrderId = existing.RepairOrderId;

            existing.RepairOrderId = packet.RepairOrderId;
            existing.PartId = packet.PartId;
            existing.Quantity = packet.Quantity;

            repo.Update(existing);
            await repo.SaveChangesAsync().ConfigureAwait(false);

            if (oldRepairOrderId != existing.RepairOrderId)
            {
                var roOld = await repairOrders.GetByIdAsync(oldRepairOrderId).ConfigureAwait(false);
                if (roOld?.InvoiceId is not null)
                {
                    Invoice invOld = await invoices.GetByIdWithDetailsAsync(roOld.InvoiceId.Value).ConfigureAwait(false);
                    if (invOld is not null)
                    {
                        invOld.Recalculate();
                        invoices.Update(invOld);
                        await invoices.SaveChangesAsync().ConfigureAwait(false);
                    }
                }
            }

            var ro = await repairOrders.GetByIdAsync(existing.RepairOrderId).ConfigureAwait(false);
            if (ro?.InvoiceId is not null)
            {
                Invoice inv = await invoices.GetByIdWithDetailsAsync(ro.InvoiceId.Value).ConfigureAwait(false);
                if (inv is not null)
                {
                    inv.Recalculate();
                    invoices.Update(inv);
                    await invoices.SaveChangesAsync().ConfigureAwait(false);
                }
            }

            confirmed = MapToPacket(existing, packet.SequenceId);
            System.Boolean sent = await connection.TCP.SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);
            if (!sent)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
            }
        }
        catch (System.ArgumentException)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.VALIDATION_FAILED,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);
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
            if (confirmed != null)
            {
                InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Return(confirmed);
            }
        }
    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((System.UInt16)OpCommand.REPAIR_ORDER_ITEM_DELETE)]
    public async System.Threading.Tasks.Task DeleteAsync(IPacket p, IConnection connection)
    {
        if (p is not RepairOrderItemDto packet || packet.RepairOrderItemId is null)
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
            var repo = new RepairOrderItemRepository(db);
            var repairOrders = new RepairOrderRepository(db);
            var invoices = new InvoiceRepository(db);

            RepairOrderItem existing = await repo.GetByIdAsync(packet.RepairOrderItemId.Value).ConfigureAwait(false);
            if (existing is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
                return;
            }

            System.Int32 repairOrderId = existing.RepairOrderId;

            repo.Delete(existing);
            await repo.SaveChangesAsync().ConfigureAwait(false);

            var ro = await repairOrders.GetByIdAsync(repairOrderId).ConfigureAwait(false);
            if (ro?.InvoiceId is not null)
            {
                Invoice inv = await invoices.GetByIdWithDetailsAsync(ro.InvoiceId.Value).ConfigureAwait(false);
                if (inv is not null)
                {
                    inv.Recalculate();
                    invoices.Update(inv);
                    await invoices.SaveChangesAsync().ConfigureAwait(false);
                }
            }

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

    private static System.Boolean TryParseRepairOrderItemPacket(IPacket p, out RepairOrderItemDto packet, out System.UInt32 fallbackSeqId)
    {
        fallbackSeqId = p is IPacketSequenced ps ? ps.SequenceId : 0;

        if (p is not RepairOrderItemDto dto)
        {
            packet = null;
            return false;
        }

        if (dto.RepairOrderId <= 0 || dto.PartId <= 0 || dto.Quantity <= 0)
        {
            packet = null;
            return false;
        }

        packet = dto;
        return true;
    }

    private static RepairOrderItemDto MapToPacket(RepairOrderItem item, System.UInt32 sequenceId)
    {
        RepairOrderItemDto dto = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Get<RepairOrderItemDto>();

        dto.SequenceId = sequenceId;
        dto.RepairOrderItemId = item.Id;
        dto.RepairOrderId = item.RepairOrderId;
        dto.PartId = item.PartId;
        dto.Quantity = item.Quantity;

        return dto;
    }
}
