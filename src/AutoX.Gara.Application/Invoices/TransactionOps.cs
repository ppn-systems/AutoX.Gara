// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Domain.Entities.Invoices;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Infrastructure.Repositories;
using AutoX.Gara.Shared.Enums;
using Microsoft.Extensions.Logging;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Protocol.Invoices;

using Nalix.Common.Networking;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security;
using Nalix.Framework.Injection;
using Nalix.Network.Connections;
using Nalix.Framework.Memory.Objects;
using Nalix.Framework.Serialization;
using System.Diagnostics;

namespace AutoX.Gara.Application.Invoices;

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
            System.UInt32 fallbackSeq = p.SequenceId;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, fallbackSeq, 0u, 0u, 0)).ConfigureAwait(false);
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
        ILogger logger = InstanceManager.Instance.GetExistingInstance<ILogger>();
        Stopwatch sw = Stopwatch.StartNew();

        if (!TryParseTransactionPacket(p, out TransactionDto packet, out System.UInt32 fallbackSeq) || packet.TransactionId is not null)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, fallbackSeq, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        TransactionDto confirmed = null;
        try
        {
            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var repo = new TransactionRepository(db);
            var invoices = new InvoiceRepository(db);

            // Validate invoice exists (lightweight query).
            if (await invoices.GetByIdAsync(packet.InvoiceId).ConfigureAwait(false) is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
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
            await RecalculateInvoiceAsync(db, invoices, packet.InvoiceId).ConfigureAwait(false);

            confirmed = MapToPacket(entity, packet.SequenceId);
            await connection.TCP.SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);
                logger?.Info(
                    $"[APP.{nameof(TransactionOps)}:{nameof(CreateAsync)}] ok ep={connection.NetworkEndpoint} seq={packet.SequenceId} ms={sw.ElapsedMilliseconds} txId={entity.Id} invoiceId={packet.InvoiceId} amt={packet.Amount} type={packet.Type} status={packet.Status}");
        }
        catch (System.ArgumentException)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.VALIDATION_FAILED,
                ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
        catch (System.Exception ex)
        {
            logger?.Error(
                $"[APP.{nameof(TransactionOps)}:{nameof(CreateAsync)}] failed ep={connection.NetworkEndpoint} seq={packet.SequenceId} ms={sw.ElapsedMilliseconds}\n{ex}");
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
    [PacketOpcode((System.UInt16)OpCommand.TRANSACTION_UPDATE)]
    public async System.Threading.Tasks.Task UpdateAsync(IPacket p, IConnection connection)
    {
        ILogger logger = InstanceManager.Instance.GetExistingInstance<ILogger>();
        Stopwatch sw = Stopwatch.StartNew();

        if (!TryParseTransactionPacket(p, out TransactionDto packet, out System.UInt32 fallbackSeq) || packet.TransactionId is null)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, fallbackSeq, 0u, 0u, 0)).ConfigureAwait(false);
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
                    ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
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
                await RecalculateInvoiceAsync(db, invoices, oldInvoiceId).ConfigureAwait(false);
            }

            await RecalculateInvoiceAsync(db, invoices, existing.InvoiceId).ConfigureAwait(false);

            confirmed = MapToPacket(existing, packet.SequenceId);
            await connection.TCP.SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);
                logger?.Info(
                    $"[APP.{nameof(TransactionOps)}:{nameof(UpdateAsync)}] ok ep={connection.NetworkEndpoint} seq={packet.SequenceId} ms={sw.ElapsedMilliseconds} txId={existing.Id} invoiceId={existing.InvoiceId} amt={existing.Amount} type={existing.Type} status={existing.Status}");
        }
        catch (System.ArgumentException)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.VALIDATION_FAILED,
                ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
        catch (System.Exception ex)
        {
            logger?.Error(
                $"[APP.{nameof(TransactionOps)}:{nameof(UpdateAsync)}] failed ep={connection.NetworkEndpoint} seq={packet.SequenceId} ms={sw.ElapsedMilliseconds}\n{ex}");
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
    [PacketOpcode((System.UInt16)OpCommand.TRANSACTION_DELETE)]
    public async System.Threading.Tasks.Task DeleteAsync(IPacket p, IConnection connection)
    {
        ILogger logger = InstanceManager.Instance.GetExistingInstance<ILogger>();
        Stopwatch sw = Stopwatch.StartNew();

        if (p is not TransactionDto packet || packet.TransactionId is null)
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
            var repo = new TransactionRepository(db);
            var invoices = new InvoiceRepository(db);

            Transaction existing = await repo.GetByIdAsync(packet.TransactionId.Value).ConfigureAwait(false);
            if (existing is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
                return;
            }

            System.Int32 invoiceId = existing.InvoiceId;

            repo.Delete(existing);
            await repo.SaveChangesAsync().ConfigureAwait(false);

            await RecalculateInvoiceAsync(db, invoices, invoiceId).ConfigureAwait(false);

            await connection.SendAsync(
                ControlType.NONE,
                ProtocolReason.NONE,
                ProtocolAdvice.NONE, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
        catch (System.Exception ex)
        {
            logger?.Error(
                $"[APP.{nameof(TransactionOps)}:{nameof(DeleteAsync)}] failed ep={connection.NetworkEndpoint} seq={packet.SequenceId} ms={sw.ElapsedMilliseconds}\n{ex}");
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
    }

    private static async System.Threading.Tasks.Task RecalculateInvoiceAsync(
        AutoXDbContext db,
        InvoiceRepository invoices,
        System.Int32 invoiceId)
    {
        // AutoXDbContextFactory configures QueryTrackingBehavior.NoTracking.
        // For recalculation we must use a tracked (identity-resolved) graph,
        // otherwise calling Update() on an untracked graph can throw duplicate key tracking exceptions.
        db.ChangeTracker.Clear();
        Invoice invTracked = await invoices.GetInvoiceWithFullGraphTrackedAsync(invoiceId).ConfigureAwait(false);
        if (invTracked is null)
        {
            return;
        }

        invTracked.Recalculate();
        await db.SaveChangesAsync().ConfigureAwait(false);
    }

    private static System.Boolean TryParseTransactionPacket(
        IPacket p,
        out TransactionDto packet,
        out System.UInt32 fallbackSeqId)
    {
        fallbackSeqId = p.SequenceId;

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




