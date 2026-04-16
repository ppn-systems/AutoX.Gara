// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Infrastructure.Repositories;
using AutoX.Gara.Shared.Enums;
using Microsoft.Extensions.Logging;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Protocol.Billings;
using Microsoft.EntityFrameworkCore;

using Nalix.Common.Networking;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security;
using Nalix.Framework.Injection;
using Nalix.Network.Connections;
using Nalix.Framework.Memory.Objects;
using Nalix.Framework.Serialization;
using System.Diagnostics;
using System.Linq;

namespace AutoX.Gara.Application.Billings;

/// <summary>
/// Packet controller xu ly CRUD cho Invoice.
/// </summary>
[PacketController]
public sealed class InvoiceOps(AutoXDbContextFactory dbContextFactory)
{
    private readonly AutoXDbContextFactory _dbContextFactory = dbContextFactory
        ?? throw new System.ArgumentNullException(nameof(dbContextFactory));

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.INVOICE_GET)]
    public async System.Threading.Tasks.Task GetAsync(IPacket p, IConnection connection)
    {
        ILogger logger = InstanceManager.Instance.GetOrCreateInstance<ILogger>();
        Stopwatch sw = Stopwatch.StartNew();

        if (p is not InvoiceQueryRequest packet)
        {
            System.UInt32 fallbackSeq = p is IPacketSequenced ps ? ps.SequenceId : 0;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);

            return;
        }

        InvoiceQueryResponse response = null;

        try
        {
            InvoiceListQuery query = new(
                Page: packet.Page,
                PageSize: packet.PageSize,
                SearchTerm: packet.SearchTerm,
                SortBy: packet.SortBy,
                SortDescending: packet.SortDescending,
                FilterCustomerId: packet.FilterCustomerId <= 0 ? null : packet.FilterCustomerId,
                FilterPaymentStatus: packet.FilterPaymentStatus,
                FilterFromDate: packet.FilterFromDate,
                FilterToDate: packet.FilterToDate);

            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var invoices = new InvoiceRepository(db);

            (System.Collections.Generic.List<Invoice> items, System.Int32 totalCount) =
                await invoices.GetPageAsync(query).ConfigureAwait(false);

            response = new()
            {
                TotalCount = totalCount,
                SequenceId = packet.SequenceId,
                Invoices = items.ConvertAll(i => MapToPacket(i, sequenceId: 0))
            };

            System.Boolean sent = await connection.TCP.SendAsync(LiteSerializer.Serialize(response)).ConfigureAwait(false);

            if (!sent)
            {
                logger?.Warn(
                    $"[APP.{nameof(InvoiceOps)}:{nameof(GetAsync)}] send-failed ep={connection.NetworkEndpoint} seq={packet.SequenceId} ms={sw.ElapsedMilliseconds}");
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
            }
            else
            {
                logger?.Info(
                    $"[APP.{nameof(InvoiceOps)}:{nameof(GetAsync)}] ok ep={connection.NetworkEndpoint} seq={packet.SequenceId} ms={sw.ElapsedMilliseconds} items={items.Count} total={totalCount} cust={packet.FilterCustomerId} term='{packet.SearchTerm}'");
            }
        }
        catch (System.Exception ex)
        {
            logger?.Error(
                $"[APP.{nameof(InvoiceOps)}:{nameof(GetAsync)}] failed ep={connection.NetworkEndpoint} seq={packet.SequenceId} ms={sw.ElapsedMilliseconds}\n{ex}");
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.RETRY, packet.SequenceId).ConfigureAwait(false);
        }
        finally
        {
            if (response?.Invoices != null)
            {
                var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
                foreach (InvoiceDto dto in response.Invoices)
                {
                    pool.Return(dto);
                }
            }
        }
    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.INVOICE_CREATE)]
    public async System.Threading.Tasks.Task CreateAsync(IPacket p, IConnection connection)
    {
        ILogger logger = InstanceManager.Instance.GetOrCreateInstance<ILogger>();
        Stopwatch sw = Stopwatch.StartNew();

        if (!TryParseInvoicePacket(p, out InvoiceDto packet, out System.UInt32 fallbackSeq) || packet.InvoiceId is not null)
        {
            logger?.Warn(
                $"[APP.{nameof(InvoiceOps)}:{nameof(CreateAsync)}] malformed-packet ep={connection.NetworkEndpoint} seq={fallbackSeq}");
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);

            return;
        }

        InvoiceDto confirmed = null;
        System.Int64 tDb = 0;
        System.Int64 tExists = 0;
        System.Int64 tSaveInvoice = 0;
        System.Int64 tLink = 0;
        System.Int64 tReload = 0;
        System.Int64 tRecalcSave = 0;
        try
        {
            logger?.Info(
                $"[APP.{nameof(InvoiceOps)}:{nameof(CreateAsync)}] start ep={connection.NetworkEndpoint} seq={packet.SequenceId} cust={packet.CustomerId} invNo='{packet.InvoiceNumber}' roId={packet.RepairOrderId} tax={packet.TaxRate} discType={packet.DiscountType} disc={packet.Discount}");

            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            InvoiceRepository invoices = new(db);
            tDb = sw.ElapsedMilliseconds;

            System.Boolean existed = await invoices
                .ExistsByInvoiceNumberAsync(packet.InvoiceNumber)
                .ConfigureAwait(false);
            tExists = sw.ElapsedMilliseconds;

            if (existed)
            {
                logger?.Warn(
                    $"[APP.{nameof(InvoiceOps)}:{nameof(CreateAsync)}] already-exists ep={connection.NetworkEndpoint} seq={packet.SequenceId} ms={sw.ElapsedMilliseconds} invNo='{packet.InvoiceNumber}'");
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.ALREADY_EXISTS,
                    ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);

                return;
            }

            Invoice invoice = new()
            {
                CustomerId = packet.CustomerId,
                InvoiceNumber = packet.InvoiceNumber,
                InvoiceDate = packet.InvoiceDate,
                PaymentStatus = packet.PaymentStatus,
                TaxRate = packet.TaxRate,
                DiscountType = packet.DiscountType,
                Discount = packet.Discount,
            };

            await invoices.AddAsync(invoice).ConfigureAwait(false);
            await invoices.SaveChangesAsync().ConfigureAwait(false);
            tSaveInvoice = sw.ElapsedMilliseconds;
            System.Int32 invoiceId = invoice.Id;

            // If client requests linking an order, do it server-side (one-step UX).
            if (packet.RepairOrderId > 0)
            {
                // Enforce: one repair order belongs to at most one invoice.
                var roInfo = await db.RepairOrders
                    .AsNoTracking()
                    .Where(r => r.Id == packet.RepairOrderId)
                    .Select(r => new { r.CustomerId, r.InvoiceId })
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);

                if (roInfo is null || roInfo.CustomerId != packet.CustomerId)
                {
                    logger?.Warn(
                        $"[APP.{nameof(InvoiceOps)}:{nameof(CreateAsync)}] ro-not-found-or-mismatch ep={connection.NetworkEndpoint} seq={packet.SequenceId} ms={sw.ElapsedMilliseconds} roId={packet.RepairOrderId} cust={packet.CustomerId}");

                    // Rollback created invoice to avoid orphan records.
                    db.Invoices.Remove(invoice);
                    await db.SaveChangesAsync().ConfigureAwait(false);

                    await connection.SendAsync(
                        ControlType.ERROR,
                        ProtocolReason.NOT_FOUND,
                        ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);

                    return;
                }

                if (roInfo.InvoiceId.HasValue)
                {
                    logger?.Warn(
                        $"[APP.{nameof(InvoiceOps)}:{nameof(CreateAsync)}] ro-already-invoiced ep={connection.NetworkEndpoint} seq={packet.SequenceId} ms={sw.ElapsedMilliseconds} roId={packet.RepairOrderId} roInvoiceId={roInfo.InvoiceId.Value}");

                    db.Invoices.Remove(invoice);
                    await db.SaveChangesAsync().ConfigureAwait(false);

                    await connection.SendAsync(
                        ControlType.ERROR,
                        ProtocolReason.ALREADY_EXISTS,
                        ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);

                    return;
                }

                // Fast + safe in a NoTracking DbContext: update FK directly in database.
                System.Int32 affected = await db.RepairOrders
                    .Where(r => r.Id == packet.RepairOrderId
                                && r.CustomerId == packet.CustomerId
                                && !r.InvoiceId.HasValue)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.InvoiceId, invoiceId))
                    .ConfigureAwait(false);

                if (affected <= 0)
                {
                    logger?.Warn(
                        $"[APP.{nameof(InvoiceOps)}:{nameof(CreateAsync)}] ro-not-found-or-mismatch ep={connection.NetworkEndpoint} seq={packet.SequenceId} ms={sw.ElapsedMilliseconds} roId={packet.RepairOrderId} cust={packet.CustomerId}");

                    // Rollback created invoice to avoid orphan records.
                    db.Invoices.Remove(invoice);
                    await db.SaveChangesAsync().ConfigureAwait(false);

                    await connection.SendAsync(
                        ControlType.ERROR,
                        ProtocolReason.ALREADY_EXISTS,
                        ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);

                    return;
                }
                tLink = sw.ElapsedMilliseconds;
            }

            // Reload and recalculate with linked data.
            db.ChangeTracker.Clear();
            Invoice withDetails = await invoices.GetInvoiceWithFullGraphTrackedAsync(invoiceId).ConfigureAwait(false);
            tReload = sw.ElapsedMilliseconds;
            if (withDetails is not null)
            {
                withDetails.Recalculate();
                await db.SaveChangesAsync().ConfigureAwait(false);
                tRecalcSave = sw.ElapsedMilliseconds;
                confirmed = MapToPacket(withDetails, packet.SequenceId);
            }
            else
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);

                return;
            }

            System.Boolean sent = await connection.TCP.SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);

            if (!sent)
            {
                logger?.Warn(
                    $"[APP.{nameof(InvoiceOps)}:{nameof(CreateAsync)}] send-failed ep={connection.NetworkEndpoint} seq={packet.SequenceId} ms={sw.ElapsedMilliseconds} invoiceId={invoice.Id}");
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);

                return;
            }

            logger?.Info(
                $"[APP.{nameof(InvoiceOps)}:{nameof(CreateAsync)}] ok ep={connection.NetworkEndpoint} seq={packet.SequenceId} ms={sw.ElapsedMilliseconds} invoiceId={confirmed.InvoiceId} total={confirmed.TotalAmount} due={confirmed.BalanceDue} tDb={tDb} tExists={tExists} tSave={tSaveInvoice} tLink={tLink} tReload={tReload} tRecalcSave={tRecalcSave}");
        }
        catch (System.ArgumentException)
        {
            logger?.Warn(
                $"[APP.{nameof(InvoiceOps)}:{nameof(CreateAsync)}] validation-failed ep={connection.NetworkEndpoint} seq={packet.SequenceId} ms={sw.ElapsedMilliseconds}");
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.VALIDATION_FAILED,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);
        }
        catch (System.Exception ex)
        {
            logger?.Error(
                $"[APP.{nameof(InvoiceOps)}:{nameof(CreateAsync)}] failed ep={connection.NetworkEndpoint} seq={packet.SequenceId} ms={sw.ElapsedMilliseconds}\n{ex}");
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
    [PacketOpcode((System.UInt16)OpCommand.INVOICE_UPDATE)]
    public async System.Threading.Tasks.Task UpdateAsync(IPacket p, IConnection connection)
    {
        ILogger logger = InstanceManager.Instance.GetOrCreateInstance<ILogger>();
        Stopwatch sw = Stopwatch.StartNew();

        if (!TryParseInvoicePacket(p, out InvoiceDto packet, out System.UInt32 fallbackSeq) || packet.InvoiceId is null)
        {
            logger?.Warn(
                $"[APP.{nameof(InvoiceOps)}:{nameof(UpdateAsync)}] malformed-packet ep={connection.NetworkEndpoint} seq={fallbackSeq}");
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);

            return;
        }

        InvoiceDto confirmed = null;
        System.Int64 tDb = 0;
        System.Int64 tLoad = 0;
        System.Int64 tExistsCheck = 0;
        System.Int64 tLink = 0;
        System.Int64 tReload = 0;
        System.Int64 tRecalcSave = 0;
        try
        {
            logger?.Info(
                $"[APP.{nameof(InvoiceOps)}:{nameof(UpdateAsync)}] start ep={connection.NetworkEndpoint} seq={packet.SequenceId} invoiceId={packet.InvoiceId} cust={packet.CustomerId} invNo='{packet.InvoiceNumber}' roId={packet.RepairOrderId} tax={packet.TaxRate} discType={packet.DiscountType} disc={packet.Discount} status={packet.PaymentStatus}");

            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var invoices = new InvoiceRepository(db);
            await using var tx = await db.Database.BeginTransactionAsync().ConfigureAwait(false);
            tDb = sw.ElapsedMilliseconds;

            Invoice existing = await invoices.GetInvoiceWithFullGraphTrackedAsync(packet.InvoiceId.Value).ConfigureAwait(false);
            tLoad = sw.ElapsedMilliseconds;

            if (existing is null)
            {
                logger?.Warn(
                    $"[APP.{nameof(InvoiceOps)}:{nameof(UpdateAsync)}] not-found ep={connection.NetworkEndpoint} seq={packet.SequenceId} ms={sw.ElapsedMilliseconds} invoiceId={packet.InvoiceId}");
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);

                return;
            }

            if (!System.String.Equals(existing.InvoiceNumber, packet.InvoiceNumber, System.StringComparison.Ordinal))
            {
                System.Boolean existed = await invoices
                    .ExistsByInvoiceNumberAsync(packet.InvoiceNumber, excludeId: existing.Id)
                    .ConfigureAwait(false);
                tExistsCheck = sw.ElapsedMilliseconds;

                if (existed)
                {
                    logger?.Warn(
                        $"[APP.{nameof(InvoiceOps)}:{nameof(UpdateAsync)}] already-exists ep={connection.NetworkEndpoint} seq={packet.SequenceId} ms={sw.ElapsedMilliseconds} invoiceId={existing.Id} invNo='{packet.InvoiceNumber}'");
                    await connection.SendAsync(
                        ControlType.ERROR,
                        ProtocolReason.ALREADY_EXISTS,
                        ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);

                    return;
                }
            }

            if (existing.CustomerId != packet.CustomerId)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.VALIDATION_FAILED,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);

                return;
            }

            existing.CustomerId = packet.CustomerId;
            existing.InvoiceNumber = packet.InvoiceNumber;
            existing.InvoiceDate = packet.InvoiceDate;
            existing.PaymentStatus = packet.PaymentStatus;
            existing.TaxRate = packet.TaxRate;
            existing.DiscountType = packet.DiscountType;
            existing.Discount = packet.Discount;

            // Link a repair order if requested.
            if (packet.RepairOrderId > 0)
            {
                // Enforce: 1 invoice links to at most 1 repair order.
                System.Boolean hasOtherOrders = await db.RepairOrders
                    .AsNoTracking()
                    .AnyAsync(r => r.InvoiceId == existing.Id && r.Id != packet.RepairOrderId)
                    .ConfigureAwait(false);

                if (hasOtherOrders)
                {
                    await connection.SendAsync(
                        ControlType.ERROR,
                        ProtocolReason.VALIDATION_FAILED,
                        ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);
                    return;
                }

                var roInfo = await db.RepairOrders
                    .AsNoTracking()
                    .Where(r => r.Id == packet.RepairOrderId)
                    .Select(r => new { r.CustomerId, r.InvoiceId })
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);

                if (roInfo is null || roInfo.CustomerId != packet.CustomerId)
                {
                    await connection.SendAsync(
                        ControlType.ERROR,
                        ProtocolReason.NOT_FOUND,
                        ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
                    return;
                }

                if (roInfo.InvoiceId.HasValue && roInfo.InvoiceId.Value != existing.Id)
                {
                    await connection.SendAsync(
                        ControlType.ERROR,
                        ProtocolReason.ALREADY_EXISTS,
                        ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
                    return;
                }

                System.Int32 affected = await db.RepairOrders
                    .Where(r => r.Id == packet.RepairOrderId
                                && r.CustomerId == packet.CustomerId
                                && (!r.InvoiceId.HasValue || r.InvoiceId == existing.Id))
                    .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.InvoiceId, existing.Id))
                    .ConfigureAwait(false);

                if (affected > 0)
                {
                    tLink = sw.ElapsedMilliseconds;
                }
                else
                {
                    logger?.Warn(
                        $"[APP.{nameof(InvoiceOps)}:{nameof(UpdateAsync)}] ro-race ep={connection.NetworkEndpoint} seq={packet.SequenceId} ms={sw.ElapsedMilliseconds} roId={packet.RepairOrderId} cust={packet.CustomerId}");

                    await tx.RollbackAsync().ConfigureAwait(false);
                    await connection.SendAsync(
                        ControlType.ERROR,
                        ProtocolReason.ALREADY_EXISTS,
                        ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
                    return;
                }
            }

            // Reload to ensure RepairOrders collection reflects any new link, then recalc.
            await db.SaveChangesAsync().ConfigureAwait(false);

            db.ChangeTracker.Clear();
            Invoice recalcing = await invoices.GetInvoiceWithFullGraphTrackedAsync(existing.Id).ConfigureAwait(false);
            tReload = sw.ElapsedMilliseconds;
            if (recalcing is not null)
            {
                recalcing.Recalculate();
                await db.SaveChangesAsync().ConfigureAwait(false);
                tRecalcSave = sw.ElapsedMilliseconds;
                confirmed = MapToPacket(recalcing, packet.SequenceId);
            }
            else
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);

                return;
            }

            await tx.CommitAsync().ConfigureAwait(false);

            System.Boolean sent = await connection.TCP.SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);
            if (!sent)
            {
                logger?.Warn(
                    $"[APP.{nameof(InvoiceOps)}:{nameof(UpdateAsync)}] send-failed ep={connection.NetworkEndpoint} seq={packet.SequenceId} ms={sw.ElapsedMilliseconds} invoiceId={packet.InvoiceId}");
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);

                return;
            }

            logger?.Info(
                $"[APP.{nameof(InvoiceOps)}:{nameof(UpdateAsync)}] ok ep={connection.NetworkEndpoint} seq={packet.SequenceId} ms={sw.ElapsedMilliseconds} invoiceId={confirmed.InvoiceId} total={confirmed.TotalAmount} due={confirmed.BalanceDue} tDb={tDb} tLoad={tLoad} tExists={tExistsCheck} tLink={tLink} tReload={tReload} tRecalcSave={tRecalcSave}");
        }
        catch (System.ArgumentException)
        {
            logger?.Warn(
                $"[APP.{nameof(InvoiceOps)}:{nameof(UpdateAsync)}] validation-failed ep={connection.NetworkEndpoint} seq={packet.SequenceId} ms={sw.ElapsedMilliseconds}");
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.VALIDATION_FAILED,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            logger?.Warn(
                $"[APP.{nameof(InvoiceOps)}:{nameof(UpdateAsync)}] concurrency ep={connection.NetworkEndpoint} seq={packet.SequenceId} ms={sw.ElapsedMilliseconds}");
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.VALIDATION_FAILED,
                ProtocolAdvice.RETRY, packet.SequenceId).ConfigureAwait(false);
            return;
        }
        catch (System.Exception ex)
        {
            logger?.Error(
                $"[APP.{nameof(InvoiceOps)}:{nameof(UpdateAsync)}] failed ep={connection.NetworkEndpoint} seq={packet.SequenceId} ms={sw.ElapsedMilliseconds}\n{ex}");
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
    [PacketOpcode((System.UInt16)OpCommand.INVOICE_DELETE)]
    public async System.Threading.Tasks.Task DeleteAsync(IPacket p, IConnection connection)
    {
        if (p is not InvoiceDto packet || packet.InvoiceId is null)
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
            System.Int32 invoiceId = packet.InvoiceId.Value;
            await using var tx = await db.Database.BeginTransactionAsync().ConfigureAwait(false);

            // Unlink repair orders first (nullable FK), then delete transactions, then delete invoice.
            await db.RepairOrders
                .Where(r => r.InvoiceId == invoiceId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.InvoiceId, (System.Int32?)null))
                .ConfigureAwait(false);

            await db.Transactions
                .Where(t => t.InvoiceId == invoiceId)
                .ExecuteDeleteAsync()
                .ConfigureAwait(false);

            Invoice invoice = await db.Invoices
                .FirstOrDefaultAsync(i => i.Id == invoiceId)
                .ConfigureAwait(false);

            if (invoice is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);

                return;
            }

            db.Invoices.Remove(invoice);
            System.Int32 saved = await db.SaveChangesAsync().ConfigureAwait(false);

            if (saved <= 0)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);

                return;
            }

            await tx.CommitAsync().ConfigureAwait(false);

            await connection.SendAsync(
                ControlType.NONE,
                ProtocolReason.NONE,
                ProtocolAdvice.NONE, packet.SequenceId).ConfigureAwait(false);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.VALIDATION_FAILED,
                ProtocolAdvice.RETRY, packet.SequenceId).ConfigureAwait(false);
        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
        }
    }

    private static System.Boolean TryParseInvoicePacket(
        IPacket p,
        out InvoiceDto packet,
        out System.UInt32 fallbackSeqId)
    {
        fallbackSeqId = p is IPacketSequenced ps ? ps.SequenceId : 0;

        if (p is not InvoiceDto dto)
        {
            packet = null;
            return false;
        }

        if (dto.CustomerId <= 0)
        {
            packet = null;
            return false;
        }

        if (System.String.IsNullOrWhiteSpace(dto.InvoiceNumber) || dto.InvoiceNumber.Trim().Length > 30)
        {
            packet = null;
            return false;
        }

        // Validate discount rules (Domain will enforce too)
        if (dto.Discount < 0)
        {
            packet = null;
            return false;
        }

        if (dto.DiscountType == DiscountType.Percentage && dto.Discount > 100)
        {
            packet = null;
            return false;
        }

        packet = dto;
        return true;
    }

    private static InvoiceDto MapToPacket(Invoice invoice, System.UInt32 sequenceId)
    {
        InvoiceDto dto = InstanceManager.Instance
            .GetOrCreateInstance<ObjectPoolManager>()
            .Get<InvoiceDto>();

        dto.SequenceId = sequenceId;
        dto.InvoiceId = invoice.Id;
        dto.CustomerId = invoice.CustomerId;
        dto.InvoiceNumber = invoice.InvoiceNumber ?? System.String.Empty;
        dto.InvoiceDate = invoice.InvoiceDate;
        dto.PaymentStatus = invoice.PaymentStatus;
        dto.TaxRate = invoice.TaxRate;
        dto.DiscountType = invoice.DiscountType;
        dto.Discount = invoice.Discount;

        dto.Subtotal = invoice.Subtotal;
        dto.DiscountAmount = invoice.DiscountAmount;
        dto.TaxAmount = invoice.TaxAmount;
        dto.TotalAmount = invoice.TotalAmount;
        dto.BalanceDue = invoice.BalanceDue;
        dto.ServiceSubtotal = invoice.ServiceSubtotal;
        dto.PartsSubtotal = invoice.PartsSubtotal;
        dto.IsFullyPaid = invoice.IsFullyPaid;
        // If there's exactly 1 linked order, expose it for FE convenience.
        // Otherwise keep 0 (unknown / multiple).
        System.Int32 roId = 0;
        if (invoice.RepairOrders is not null)
        {
            System.Int32[] ids = invoice.RepairOrders
                .Select(r => r.Id)
                .Distinct()
                .Take(2)
                .ToArray();

            if (ids.Length == 1)
            {
                roId = ids[0];
            }
        }

        dto.RepairOrderId = roId;

        return dto;
    }
}


