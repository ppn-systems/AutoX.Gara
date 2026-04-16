using AutoX.Gara.Shared.Enums;
using Nalix.Common.Networking.Protocols;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Services;
using AutoX.Gara.Domain.Entities.Billings;
using Nalix.Common.Networking.Protocols;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Protocol.Billings;
using Microsoft.Extensions.Logging;
using Nalix.Common.Networking;
using Nalix.Common.Networking.Packets;
using AutoX.Gara.Api.Handlers.Common;
using Nalix.Framework.DataFrames.SignalFrames;
using Nalix.Framework.DataFrames.Pooling;
using Nalix.Common.Security;
using Nalix.Framework.Injection;
using Nalix.Framework.Memory.Objects;
using Nalix.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoX.Gara.Api.Handlers.Financial;

/// <summary>
/// Packet Handler for invoice related operations.
/// </summary>
[PacketController]
public sealed class InvoiceHandler(IInvoiceAppService invoiceService)
{
    private readonly IInvoiceAppService _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.INVOICE_GET)]
    public async ValueTask GetAsync(IPacketContext<InvoiceQueryRequest> context)
    {
        InvoiceQueryRequest packet = context.Packet;
        IConnection connection = context.Connection;

        var query = new InvoiceListQuery(packet.Page, packet.PageSize, packet.SearchTerm, packet.SortBy, packet.SortDescending, 
            packet.FilterCustomerId <= 0 ? null : packet.FilterCustomerId, packet.FilterPaymentStatus, packet.FilterFromDate, packet.FilterToDate);

        var result = await _invoiceService.GetPageAsync(query).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        using var lease = PacketPool<InvoiceQueryResponse>.Rent();
        var response = lease.Value;
        response.TotalCount = result.Data!.totalCount;
        response.SequenceId = packet.SequenceId;
        response.Invoices = result.Data.items.ConvertAll(i => MapToPacket(i, 0));

        try
        {
            await connection.TCP.SendAsync(response).ConfigureAwait(false);

        }
        finally
        {
            ReturnDtos(response.Invoices);
        }
    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.INVOICE_CREATE)]
    public async ValueTask CreateAsync(IPacketContext<InvoiceDto> context)
    {
        InvoiceDto packet = context.Packet;
        IConnection connection = context.Connection;

        if (packet.CustomerId <= 0 || string.IsNullOrWhiteSpace(packet.InvoiceNumber))
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }

        var invoice = new Invoice
        {
            CustomerId = packet.CustomerId,
            InvoiceNumber = packet.InvoiceNumber,
            InvoiceDate = packet.InvoiceDate,
            PaymentStatus = packet.PaymentStatus,
            TaxRate = packet.TaxRate,
            DiscountType = packet.DiscountType,
            Discount = packet.Discount,
            Notes = packet.Notes ?? string.Empty
        };

        var result = await _invoiceService.CreateAsync(invoice, packet.RepairOrderId > 0 ? packet.RepairOrderId : null).ConfigureAwait(false);
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
    [PacketOpcode((ushort)OpCommand.INVOICE_UPDATE)]
    public async ValueTask UpdateAsync(IPacketContext<InvoiceDto> context)
    {
        InvoiceDto packet = context.Packet;
        IConnection connection = context.Connection;

        if (packet.InvoiceId == null)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }

        var invoice = new Invoice
        {
            Id = packet.InvoiceId.Value,
            CustomerId = packet.CustomerId,
            InvoiceNumber = packet.InvoiceNumber,
            InvoiceDate = packet.InvoiceDate,
            PaymentStatus = packet.PaymentStatus,
            TaxRate = packet.TaxRate,
            DiscountType = packet.DiscountType,
            Discount = packet.Discount,
            Notes = packet.Notes ?? string.Empty
        };

        var result = await _invoiceService.UpdateAsync(invoice, packet.RepairOrderId > 0 ? packet.RepairOrderId : null).ConfigureAwait(false);
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
    [PacketOpcode((ushort)OpCommand.INVOICE_DELETE)]
    public async ValueTask DeleteAsync(IPacketContext<InvoiceDto> context)
    {
        InvoiceDto packet = context.Packet;
        IConnection connection = context.Connection;

        if (packet.InvoiceId == null)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }

        var result = await _invoiceService.DeleteAsync(packet.InvoiceId.Value).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        await context.OkAsync().ConfigureAwait(false);

    }

    private static InvoiceDto MapToPacket(Invoice invoice, uint sequenceId)
    {
        var dto = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Get<InvoiceDto>();
        dto.SequenceId = sequenceId;
        dto.InvoiceId = invoice.Id;
        dto.CustomerId = invoice.CustomerId;
        dto.InvoiceNumber = invoice.InvoiceNumber ?? string.Empty;
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

        int roId = 0;
        if (invoice.RepairOrders != null && invoice.RepairOrders.Any())
        {
            roId = invoice.RepairOrders.First().Id;
        }
        dto.RepairOrderId = roId;

        return dto;
    }

    private static void ReturnDtos(IEnumerable<InvoiceDto> dtos)
    {
        if (dtos == null) return;
        var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
        foreach (var dto in dtos) pool.Return(dto);
    }

    private static void ReturnToPool(InvoiceDto dto)
    {
        if (dto != null) InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Return(dto);
    }

    
}