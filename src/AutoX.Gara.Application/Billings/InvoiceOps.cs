// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Domain.Enums;
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
        if (!TryParseInvoicePacket(p, out InvoiceDto packet, out System.UInt32 fallbackSeq) || packet.InvoiceId is not null)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);

            return;
        }

        InvoiceDto confirmed = null;
        try
        {
            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var invoices = new InvoiceRepository(db);

            System.Boolean existed = await invoices
                .ExistsByInvoiceNumberAsync(packet.InvoiceNumber)
                .ConfigureAwait(false);

            if (existed)
            {
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

            invoice.Recalculate();

            await invoices.AddAsync(invoice).ConfigureAwait(false);
            await invoices.SaveChangesAsync().ConfigureAwait(false);

            confirmed = MapToPacket(invoice, packet.SequenceId);

            System.Boolean sent = await connection.TCP.SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);

            if (!sent)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);

                return;
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
    [PacketOpcode((System.UInt16)OpCommand.INVOICE_UPDATE)]
    public async System.Threading.Tasks.Task UpdateAsync(IPacket p, IConnection connection)
    {
        if (!TryParseInvoicePacket(p, out InvoiceDto packet, out System.UInt32 fallbackSeq) || packet.InvoiceId is null)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);

            return;
        }

        InvoiceDto confirmed = null;
        try
        {
            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var invoices = new InvoiceRepository(db);

            Invoice existing = await invoices
                .GetByIdWithDetailsAsync(packet.InvoiceId.Value)
                .ConfigureAwait(false);

            if (existing is null)
            {
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

                if (existed)
                {
                    await connection.SendAsync(
                        ControlType.ERROR,
                        ProtocolReason.ALREADY_EXISTS,
                        ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);

                    return;
                }
            }

            existing.CustomerId = packet.CustomerId;
            existing.InvoiceNumber = packet.InvoiceNumber;
            existing.InvoiceDate = packet.InvoiceDate;
            existing.PaymentStatus = packet.PaymentStatus;
            existing.TaxRate = packet.TaxRate;
            existing.DiscountType = packet.DiscountType;
            existing.Discount = packet.Discount;

            existing.Recalculate();

            invoices.Update(existing);
            await invoices.SaveChangesAsync().ConfigureAwait(false);

            confirmed = MapToPacket(existing, packet.SequenceId);

            System.Boolean sent = await connection.TCP.SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);
            if (!sent)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);

                return;
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
            var invoices = new InvoiceRepository(db);

            Invoice existing = await invoices
                .GetByIdWithDetailsAsync(packet.InvoiceId.Value)
                .ConfigureAwait(false);

            if (existing is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);

                return;
            }

            invoices.Delete(existing);
            await invoices.SaveChangesAsync().ConfigureAwait(false);

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

        return dto;
    }
}
