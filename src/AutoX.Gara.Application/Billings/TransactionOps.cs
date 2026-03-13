// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Domain.Entities.Invoices;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Infrastructure.Repositories;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Protocol.Billings;
using Nalix.Common.Networking.Abstractions;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Attributes;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security.Enums;
using Nalix.Framework.Injection;
using Nalix.Network.Connections;
using Nalix.Shared.Memory.Pooling;
using Nalix.Shared.Serialization;

namespace AutoX.Gara.Application.Billings;

[PacketController]
public sealed class TransactionOps(AutoXDbContextFactory dbContextFactory)
{
    private readonly AutoXDbContextFactory _dbContextFactory = dbContextFactory
        ?? throw new System.ArgumentNullException(nameof(dbContextFactory));

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.TRANSACTION_GET)]
    public async System.Threading.Tasks.Task GetAsync(IPacket p, IConnection connection)
    {
        if (p is not TransactionQueryRequest packet)
        {
            System.UInt32 fallbackSeq = p is IPacketSequenced ps ? ps.SequenceId : 0;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);
            return;
        }

        TransactionQueryResponse response = null;

        try
        {
            TransactionListQuery query = new(
                Page: packet.Page,
                PageSize: packet.PageSize,
                SearchTerm: packet.SearchTerm,
                SortBy: packet.SortBy,
                SortDescending: packet.SortDescending,
                FilterInvoiceId: packet.FilterInvoiceId <= 0 ? null : packet.FilterInvoiceId,
                FilterType: packet.FilterType,
                FilterStatus: packet.FilterStatus,
                FilterPaymentMethod: packet.FilterPaymentMethod,
                FilterMinAmount: packet.FilterMinAmount,
                FilterMaxAmount: packet.FilterMaxAmount,
                FilterFromDate: packet.FilterFromDate,
                FilterToDate: packet.FilterToDate);

            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var repo = new TransactionRepository(db);

            (System.Collections.Generic.List<Transaction> items, System.Int32 totalCount) =
                await repo.GetPageAsync(query).ConfigureAwait(false);

            response = new()
            {
                TotalCount = totalCount,
                SequenceId = packet.SequenceId,
                Transactions = items.ConvertAll(t => MapToPacket(t, sequenceId: 0))
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
            if (response?.Transactions != null)
            {
                var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
                foreach (TransactionDto dto in response.Transactions)
                {
                    pool.Return(dto);
                }
            }
        }
    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.TRANSACTION_CREATE)]
    public async System.Threading.Tasks.Task CreateAsync(IPacket p, IConnection connection)
    {
        if (!TryParseTransactionPacket(p, out TransactionDto packet, out System.UInt32 fallbackSeq) || packet.TransactionId is not null)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);
            return;
        }

        TransactionDto confirmed = null;
        try
        {
            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var repo = new TransactionRepository(db);
            var invoices = new InvoiceRepository(db);

            Invoice inv = await invoices.GetByIdWithDetailsAsync(packet.InvoiceId).ConfigureAwait(false);
            if (inv is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
                return;
            }

            Transaction entity = new()
            {
                InvoiceId = packet.InvoiceId,
                Type = packet.Type,
                PaymentMethod = packet.PaymentMethod,
                Status = packet.Status,
                Amount = packet.Amount,
                TransactionDate = packet.TransactionDate,
                CreatedBy = packet.CreatedBy,
                ModifiedBy = packet.ModifiedBy,
                UpdatedAt = packet.UpdatedAt,
                IsReversed = packet.IsReversed,
                Description = packet.Description?.Trim() ?? System.String.Empty,
            };

            await repo.AddAsync(entity).ConfigureAwait(false);
            await repo.SaveChangesAsync().ConfigureAwait(false);

            // Recalculate invoice totals after adding a transaction.
            inv = await invoices.GetByIdWithDetailsAsync(packet.InvoiceId).ConfigureAwait(false);
            if (inv is not null)
            {
                inv.Recalculate();
                invoices.Update(inv);
                await invoices.SaveChangesAsync().ConfigureAwait(false);
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
    [PacketOpcode((System.UInt16)OpCommand.TRANSACTION_UPDATE)]
    public async System.Threading.Tasks.Task UpdateAsync(IPacket p, IConnection connection)
    {
        if (!TryParseTransactionPacket(p, out TransactionDto packet, out System.UInt32 fallbackSeq) || packet.TransactionId is null)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);
            return;
        }

        TransactionDto confirmed = null;
        try
        {
            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var repo = new TransactionRepository(db);
            var invoices = new InvoiceRepository(db);

            Transaction existing = await repo.GetByIdAsync(packet.TransactionId.Value).ConfigureAwait(false);
            if (existing is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
                return;
            }

            System.Int32 oldInvoiceId = existing.InvoiceId;

            existing.InvoiceId = packet.InvoiceId;
            existing.Type = packet.Type;
            existing.PaymentMethod = packet.PaymentMethod;
            existing.Status = packet.Status;
            existing.Amount = packet.Amount;
            existing.TransactionDate = packet.TransactionDate;
            existing.CreatedBy = packet.CreatedBy;
            existing.ModifiedBy = packet.ModifiedBy;
            existing.UpdatedAt = packet.UpdatedAt;
            existing.IsReversed = packet.IsReversed;
            existing.Description = packet.Description?.Trim() ?? System.String.Empty;

            repo.Update(existing);
            await repo.SaveChangesAsync().ConfigureAwait(false);

            if (oldInvoiceId != existing.InvoiceId)
            {
                Invoice invOld = await invoices.GetByIdWithDetailsAsync(oldInvoiceId).ConfigureAwait(false);
                if (invOld is not null)
                {
                    invOld.Recalculate();
                    invoices.Update(invOld);
                    await invoices.SaveChangesAsync().ConfigureAwait(false);
                }
            }

            Invoice inv = await invoices.GetByIdWithDetailsAsync(existing.InvoiceId).ConfigureAwait(false);
            if (inv is not null)
            {
                inv.Recalculate();
                invoices.Update(inv);
                await invoices.SaveChangesAsync().ConfigureAwait(false);
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
    [PacketOpcode((System.UInt16)OpCommand.TRANSACTION_DELETE)]
    public async System.Threading.Tasks.Task DeleteAsync(IPacket p, IConnection connection)
    {
        if (p is not TransactionDto packet || packet.TransactionId is null)
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
            var repo = new TransactionRepository(db);
            var invoices = new InvoiceRepository(db);

            Transaction existing = await repo.GetByIdAsync(packet.TransactionId.Value).ConfigureAwait(false);
            if (existing is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
                return;
            }

            System.Int32 invoiceId = existing.InvoiceId;

            repo.Delete(existing);
            await repo.SaveChangesAsync().ConfigureAwait(false);

            Invoice inv = await invoices.GetByIdWithDetailsAsync(invoiceId).ConfigureAwait(false);
            if (inv is not null)
            {
                inv.Recalculate();
                invoices.Update(inv);
                await invoices.SaveChangesAsync().ConfigureAwait(false);
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

    private static System.Boolean TryParseTransactionPacket(
        IPacket p,
        out TransactionDto packet,
        out System.UInt32 fallbackSeqId)
    {
        fallbackSeqId = p is IPacketSequenced ps ? ps.SequenceId : 0;

        if (p is not TransactionDto dto)
        {
            packet = null;
            return false;
        }

        if (dto.InvoiceId <= 0)
        {
            packet = null;
            return false;
        }

        if (dto.Amount <= 0)
        {
            packet = null;
            return false;
        }

        if (dto.Description != null && dto.Description.Length > 255)
        {
            packet = null;
            return false;
        }

        packet = dto;
        return true;
    }

    private static TransactionDto MapToPacket(Transaction transaction, System.UInt32 sequenceId)
    {
        TransactionDto dto = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Get<TransactionDto>();

        dto.SequenceId = sequenceId;
        dto.TransactionId = transaction.Id;
        dto.InvoiceId = transaction.InvoiceId;
        dto.Type = transaction.Type;
        dto.PaymentMethod = transaction.PaymentMethod;
        dto.Status = transaction.Status;
        dto.Amount = transaction.Amount;
        dto.TransactionDate = transaction.TransactionDate;
        dto.CreatedBy = transaction.CreatedBy;
        dto.ModifiedBy = transaction.ModifiedBy;
        dto.UpdatedAt = transaction.UpdatedAt;
        dto.IsReversed = transaction.IsReversed;
        dto.Description = transaction.Description ?? System.String.Empty;

        return dto;
    }
}

