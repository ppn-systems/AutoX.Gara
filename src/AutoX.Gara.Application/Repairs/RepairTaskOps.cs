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
using Nalix.Framework.Memory.Objects;
using Nalix.Framework.Serialization;

namespace AutoX.Gara.Application.Repairs;

[PacketController]
public sealed class RepairTaskOps(AutoXDbContextFactory dbContextFactory)
{
    private readonly AutoXDbContextFactory _dbContextFactory = dbContextFactory
        ?? throw new System.ArgumentNullException(nameof(dbContextFactory));

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.REPAIR_TASK_GET)]
    public async System.Threading.Tasks.Task GetAsync(IPacket p, IConnection connection)
    {
        if (p is not RepairTaskQueryRequest packet)
        {
            System.UInt32 fallbackSeq = p.SequenceId;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, fallbackSeq, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        RepairTaskQueryResponse response = null;

        try
        {
            RepairTaskListQuery query = new(
                Page: packet.Page,
                PageSize: packet.PageSize,
                SearchTerm: packet.SearchTerm,
                SortBy: packet.SortBy,
                SortDescending: packet.SortDescending,
                FilterRepairOrderId: packet.FilterRepairOrderId <= 0 ? null : packet.FilterRepairOrderId,
                FilterEmployeeId: packet.FilterEmployeeId <= 0 ? null : packet.FilterEmployeeId,
                FilterServiceItemId: packet.FilterServiceItemId <= 0 ? null : packet.FilterServiceItemId,
                FilterStatus: packet.FilterStatus,
                FilterFromDate: packet.FilterFromDate,
                FilterToDate: packet.FilterToDate);

            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var repo = new RepairTaskRepository(db);

            (System.Collections.Generic.List<RepairTask> items, System.Int32 totalCount) =
                await repo.GetPageAsync(query).ConfigureAwait(false);

            response = new()
            {
                TotalCount = totalCount,
                SequenceId = packet.SequenceId,
                RepairTasks = items.ConvertAll(t => MapToPacket(t, sequenceId: 0))
            };

            await connection.TCP.SendAsync(LiteSerializer.Serialize(response)).ConfigureAwait(false);
        }
        catch (System.ArgumentException)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.VALIDATION_FAILED,
                ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
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
            if (response?.RepairTasks != null)
            {
                var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
                foreach (RepairTaskDto dto in response.RepairTasks)
                {
                    pool.Return(dto);
                }
            }
        }
    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.REPAIR_TASK_CREATE)]
    public async System.Threading.Tasks.Task CreateAsync(IPacket p, IConnection connection)
    {
        if (!TryParseRepairTaskPacket(p, out RepairTaskDto packet, out System.UInt32 fallbackSeq) || packet.RepairTaskId is not null)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, fallbackSeq, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        RepairTaskDto confirmed = null;
        try
        {
            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var repo = new RepairTaskRepository(db);
            var repairOrders = new RepairOrderRepository(db);
            var invoices = new InvoiceRepository(db);

            RepairTask entity = new()
            {
                RepairOrderId = packet.RepairOrderId,
                EmployeeId = packet.EmployeeId,
                ServiceItemId = packet.ServiceItemId,
                Status = packet.Status,
                StartDate = packet.StartDate,
                EstimatedDuration = packet.EstimatedDuration,
                CompletionDate = packet.CompletionDate,
            };

            await repo.AddAsync(entity).ConfigureAwait(false);
            await repo.SaveChangesAsync().ConfigureAwait(false);

            // Recalculate invoice totals if this repair order is attached to an invoice.
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
            await connection.TCP.SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);
        }
        catch (System.ArgumentException)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.VALIDATION_FAILED,
                ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
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
            if (confirmed != null)
            {
                InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Return(confirmed);
            }
        }
    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.REPAIR_TASK_UPDATE)]
    public async System.Threading.Tasks.Task UpdateAsync(IPacket p, IConnection connection)
    {
        if (!TryParseRepairTaskPacket(p, out RepairTaskDto packet, out System.UInt32 fallbackSeq) || packet.RepairTaskId is null)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, fallbackSeq, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        RepairTaskDto confirmed = null;
        try
        {
            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var repo = new RepairTaskRepository(db);
            var repairOrders = new RepairOrderRepository(db);
            var invoices = new InvoiceRepository(db);

            RepairTask existing = await repo.GetByIdAsync(packet.RepairTaskId.Value).ConfigureAwait(false);
            if (existing is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
                return;
            }

            System.Int32 oldRepairOrderId = existing.RepairOrderId;

            existing.RepairOrderId = packet.RepairOrderId;
            existing.EmployeeId = packet.EmployeeId;
            existing.ServiceItemId = packet.ServiceItemId;
            existing.Status = packet.Status;
            existing.StartDate = packet.StartDate;
            existing.EstimatedDuration = packet.EstimatedDuration;
            existing.CompletionDate = packet.CompletionDate;

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
            await connection.TCP.SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);
        }
        catch (System.ArgumentException)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.VALIDATION_FAILED,
                ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
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
            if (confirmed != null)
            {
                InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Return(confirmed);
            }
        }
    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((System.UInt16)OpCommand.REPAIR_TASK_DELETE)]
    public async System.Threading.Tasks.Task DeleteAsync(IPacket p, IConnection connection)
    {
        if (p is not RepairTaskDto packet || packet.RepairTaskId is null)
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
            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var repo = new RepairTaskRepository(db);
            var repairOrders = new RepairOrderRepository(db);
            var invoices = new InvoiceRepository(db);

            RepairTask existing = await repo.GetByIdAsync(packet.RepairTaskId.Value).ConfigureAwait(false);
            if (existing is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
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

    private static System.Boolean TryParseRepairTaskPacket(IPacket p, out RepairTaskDto packet, out System.UInt32 fallbackSeqId)
    {
        fallbackSeqId = p.SequenceId;

        if (p is not RepairTaskDto dto)
        {
            packet = null;
            return false;
        }

        if (dto.RepairOrderId <= 0 || dto.EmployeeId <= 0 || dto.ServiceItemId <= 0)
        {
            packet = null;
            return false;
        }

        if (dto.EstimatedDuration is < 0 or > 1000)
        {
            packet = null;
            return false;
        }

        if (dto.StartDate.HasValue && dto.StartDate.Value > System.DateTime.UtcNow)
        {
            packet = null;
            return false;
        }

        if (dto.CompletionDate.HasValue && dto.StartDate.HasValue && dto.CompletionDate.Value < dto.StartDate.Value)
        {
            packet = null;
            return false;
        }

        packet = dto;
        return true;
    }

    private static RepairTaskDto MapToPacket(RepairTask task, System.UInt32 sequenceId)
    {
        RepairTaskDto dto = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Get<RepairTaskDto>();

        dto.SequenceId = sequenceId;
        dto.RepairTaskId = task.Id;
        dto.RepairOrderId = task.RepairOrderId;
        dto.EmployeeId = task.EmployeeId;
        dto.ServiceItemId = task.ServiceItemId;
        dto.Status = task.Status;
        dto.StartDate = task.StartDate;
        dto.EstimatedDuration = task.EstimatedDuration;
        dto.CompletionDate = task.CompletionDate;
        dto.IsCompleted = task.IsCompleted;

        return dto;
    }
}



