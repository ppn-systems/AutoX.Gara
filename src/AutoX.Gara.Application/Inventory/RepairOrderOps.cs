// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Domain.Entities.Invoices;
using AutoX.Gara.Domain.Enums.Repairs;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Protocol.Invoices;
using Nalix.Common.Networking;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security;
using Nalix.Framework.Injection;
using Nalix.Framework.Memory.Objects;
using Nalix.Framework.Serialization;
using Nalix.Runtime.Extensions;

namespace AutoX.Gara.Application.Inventory;

[PacketController]
public sealed class RepairOrderOps(IDataSessionFactory dataSessionFactory)
{
    private readonly IDataSessionFactory _dataSessionFactory = dataSessionFactory
        ?? throw new System.ArgumentNullException(nameof(dataSessionFactory));

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.REPAIR_ORDER_GET)]
    public async System.Threading.Tasks.Task GetAsync(IPacket p, IConnection connection)
    {
        if (p is not RepairOrderQueryRequest packet)
        {
            System.UInt32 fallbackSeq = p.SequenceId;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, fallbackSeq, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        RepairOrderQueryResponse response = null;

        try
        {
            RepairOrderListQuery query = new(
                Page: packet.Page,
                PageSize: packet.PageSize,
                SearchTerm: packet.SearchTerm,
                SortBy: packet.SortBy,
                SortDescending: packet.SortDescending,
                FilterCustomerId: packet.FilterCustomerId <= 0 ? null : packet.FilterCustomerId,
                FilterVehicleId: packet.FilterVehicleId <= 0 ? null : packet.FilterVehicleId,
                FilterInvoiceId: packet.FilterInvoiceId <= 0 ? null : packet.FilterInvoiceId,
                FilterStatus: packet.FilterStatus,
                FilterFromDate: packet.FilterFromDate,
                FilterToDate: packet.FilterToDate);

            await using var session = _dataSessionFactory.Create();
            var repo = session.RepairOrders;

            (System.Collections.Generic.List<RepairOrder> items, System.Int32 totalCount) =
                await repo.GetPageAsync(query).ConfigureAwait(false);

            response = new()
            {
                TotalCount = totalCount,
                SequenceId = packet.SequenceId,
                RepairOrders = items.ConvertAll(ro => MapToPacket(ro, sequenceId: 0))
            };

            await connection.TCP.SendAsync(LiteSerializer.Serialize(response)).ConfigureAwait(false);
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
            if (response?.RepairOrders != null)
            {
                var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
                foreach (RepairOrderDto dto in response.RepairOrders)
                {
                    pool.Return(dto);
                }
            }
        }
    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.REPAIR_ORDER_CREATE)]
    public async System.Threading.Tasks.Task CreateAsync(IPacket p, IConnection connection)
    {
        if (!TryParseRepairOrderPacket(p, out RepairOrderDto packet, out System.UInt32 fallbackSeq) || packet.RepairOrderId is not null)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, fallbackSeq, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        RepairOrderDto confirmed = null;
        try
        {
            await using var session = _dataSessionFactory.Create();
            var repo = session.RepairOrders;
            var invoices = session.Invoices;
            await using var tx = await session.Context.Database.BeginTransactionAsync().ConfigureAwait(false);

            RepairOrder ro = new()
            {
                CustomerId = packet.CustomerId,
                VehicleId = packet.VehicleId,
                InvoiceId = packet.InvoiceId,
                OrderDate = packet.OrderDate,
                CompletionDate = packet.CompletionDate,
                Status = packet.Status == RepairOrderStatus.None ? RepairOrderStatus.Pending : packet.Status,
            };

            await repo.AddAsync(ro).ConfigureAwait(false);
            await session.Context.SaveChangesAsync().ConfigureAwait(false);

            if (ro.InvoiceId.HasValue)
            {
                Invoice inv = await invoices.GetByIdWithDetailsAsync(ro.InvoiceId.Value).ConfigureAwait(false);
                if (inv is not null)
                {
                    if (inv.CustomerId != ro.CustomerId)
                    {
                        await connection.SendAsync(
                            ControlType.ERROR,
                            ProtocolReason.VALIDATION_FAILED,
                            ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
                        return;
                    }

                    inv.Recalculate();
                    invoices.Update(inv);
                    await session.Context.SaveChangesAsync().ConfigureAwait(false);
                }
            }

            await tx.CommitAsync().ConfigureAwait(false);

            confirmed = MapToPacket(ro, packet.SequenceId);
            await connection.TCP.SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);
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
    [PacketOpcode((System.UInt16)OpCommand.REPAIR_ORDER_UPDATE)]
    public async System.Threading.Tasks.Task UpdateAsync(IPacket p, IConnection connection)
    {
        if (!TryParseRepairOrderPacket(p, out RepairOrderDto packet, out System.UInt32 fallbackSeq) || packet.RepairOrderId is null)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, fallbackSeq, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        RepairOrderDto confirmed = null;
        try
        {
            await using var session = _dataSessionFactory.Create();
            var repo = session.RepairOrders;
            var invoices = session.Invoices;
            await using var tx = await session.Context.Database.BeginTransactionAsync().ConfigureAwait(false);

            RepairOrder existing = await repo.GetByIdAsync(packet.RepairOrderId.Value).ConfigureAwait(false);
            if (existing is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
                return;
            }

            if (existing.CustomerId != packet.CustomerId)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.VALIDATION_FAILED,
                    ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
                return;
            }

            System.Int32? oldInvoiceId = existing.InvoiceId;

            existing.CustomerId = packet.CustomerId;
            existing.VehicleId = packet.VehicleId;
            existing.InvoiceId = packet.InvoiceId;
            existing.OrderDate = packet.OrderDate;
            existing.CompletionDate = packet.CompletionDate;
            existing.Status = packet.Status;

            repo.Update(existing);
            await session.Context.SaveChangesAsync().ConfigureAwait(false);

            if (oldInvoiceId.HasValue && (!existing.InvoiceId.HasValue || existing.InvoiceId.Value != oldInvoiceId.Value))
            {
                Invoice invOld = await invoices.GetByIdWithDetailsAsync(oldInvoiceId.Value).ConfigureAwait(false);
                if (invOld is not null)
                {
                    invOld.Recalculate();
                    invoices.Update(invOld);
                    await session.Context.SaveChangesAsync().ConfigureAwait(false);
                }
            }

            if (existing.InvoiceId.HasValue)
            {
                Invoice inv = await invoices.GetByIdWithDetailsAsync(existing.InvoiceId.Value).ConfigureAwait(false);
                if (inv is not null)
                {
                    if (inv.CustomerId != existing.CustomerId)
                    {
                        await connection.SendAsync(
                            ControlType.ERROR,
                            ProtocolReason.VALIDATION_FAILED,
                            ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
                        return;
                    }

                    inv.Recalculate();
                    invoices.Update(inv);
                    await session.Context.SaveChangesAsync().ConfigureAwait(false);
                }
            }

            await tx.CommitAsync().ConfigureAwait(false);

            confirmed = MapToPacket(existing, packet.SequenceId);
            await connection.TCP.SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.VALIDATION_FAILED,
                ProtocolAdvice.RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
            return;
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
    [PacketOpcode((System.UInt16)OpCommand.REPAIR_ORDER_DELETE)]
    public async System.Threading.Tasks.Task DeleteAsync(IPacket p, IConnection connection)
    {
        if (p is not RepairOrderDto packet || packet.RepairOrderId is null)
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
            var repo = session.RepairOrders;
            var invoices = session.Invoices;
            await using var tx = await session.Context.Database.BeginTransactionAsync().ConfigureAwait(false);

            RepairOrder existing = await repo.GetByIdAsync(packet.RepairOrderId.Value).ConfigureAwait(false);
            if (existing is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
                return;
            }

            System.Int32? invoiceId = existing.InvoiceId;

            repo.Delete(existing);
            await session.Context.SaveChangesAsync().ConfigureAwait(false);

            if (invoiceId.HasValue)
            {
                Invoice inv = await invoices.GetByIdWithDetailsAsync(invoiceId.Value).ConfigureAwait(false);
                if (inv is not null)
                {
                    inv.Recalculate();
                    invoices.Update(inv);
                    await session.Context.SaveChangesAsync().ConfigureAwait(false);
                }
            }

            await tx.CommitAsync().ConfigureAwait(false);

            await connection.SendAsync(
                ControlType.NONE,
                ProtocolReason.NONE,
                ProtocolAdvice.NONE, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.VALIDATION_FAILED,
                ProtocolAdvice.RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
    }

    private static System.Boolean TryParseRepairOrderPacket(IPacket p, out RepairOrderDto packet, out System.UInt32 fallbackSeqId)
    {
        fallbackSeqId = p.SequenceId;

        if (p is not RepairOrderDto dto)
        {
            packet = null;
            return false;
        }

        if (dto.CustomerId <= 0)
        {
            packet = null;
            return false;
        }

        packet = dto;
        return true;
    }

    private static RepairOrderDto MapToPacket(RepairOrder ro, System.UInt32 sequenceId)
    {
        RepairOrderDto dto = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Get<RepairOrderDto>();

        dto.SequenceId = sequenceId;
        dto.RepairOrderId = ro.Id;
        dto.CustomerId = ro.CustomerId;
        dto.VehicleId = ro.VehicleId;
        dto.InvoiceId = ro.InvoiceId;
        dto.OrderDate = ro.OrderDate;
        dto.CompletionDate = ro.CompletionDate;
        dto.Status = ro.Status;
        dto.TotalRepairCost = ro.TotalRepairCost;
        dto.IsCompleted = ro.IsCompleted;

        return dto;
    }
}






