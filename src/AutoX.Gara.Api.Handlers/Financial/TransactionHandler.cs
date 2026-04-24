using AutoX.Gara.Api.Handlers.Common;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Services;
using AutoX.Gara.Domain.Entities.Invoices;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Protocol.Invoices;
using Nalix.Common.Networking;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security;
using Nalix.Framework.DataFrames.Pooling;

namespace AutoX.Gara.Api.Handlers.Financial;

/// <summary>
/// Packet Handler for financial transaction related operations.
/// </summary>
[PacketController]
public sealed class TransactionHandler(ITransactionAppService transactionService)
{
    private readonly ITransactionAppService _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.TRANSACTION_GET)]
    public async ValueTask GetAsync(IPacketContext<TransactionQueryRequest> context)
    {
        TransactionQueryRequest packet = context.Packet;
        IConnection connection = context.Connection;

        var query = new TransactionListQuery(
            packet.Page,
            packet.PageSize,
            packet.SearchTerm ?? string.Empty,
            packet.SortBy,
            packet.SortDescending,
            packet.FilterInvoiceId > 0 ? packet.FilterInvoiceId : null,
            packet.FilterType,
            packet.FilterStatus,
            packet.FilterPaymentMethod,
            packet.FilterMinAmount,
            packet.FilterMaxAmount,
            packet.FilterFromDate,
            packet.FilterToDate
        );

        var result = await _transactionService.GetPageAsync(query).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        using var lease = PacketPool<TransactionQueryResponse>.Rent();
        var response = lease.Value;
        response.TotalCount = result.Data!.totalCount;
        response.SequenceId = packet.SequenceId;
        response.Transactions = result.Data.items.ConvertAll(t => MapToPacket(t, 0));

        await connection.TCP.SendAsync(response).ConfigureAwait(false);

    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.TRANSACTION_CREATE)]
    public async ValueTask CreateAsync(IPacketContext<TransactionDto> context)
    {
        TransactionDto packet = context.Packet;
        IConnection connection = context.Connection;

        if (packet.InvoiceId <= 0 || packet.Amount <= 0)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }

        var transaction = new Transaction
        {
            InvoiceId = packet.InvoiceId,
            Amount = packet.Amount,
            TransactionDate = packet.TransactionDate == default ? DateTime.UtcNow : packet.TransactionDate,
            Type = packet.Type,
            PaymentMethod = packet.PaymentMethod,
            Status = packet.Status,
            Description = packet.Description ?? string.Empty,
            IsReversed = packet.IsReversed
        };

        var result = await _transactionService.CreateAsync(transaction).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        await connection.TCP.SendAsync(MapToPacket(result.Data!, packet.SequenceId)).ConfigureAwait(false);

    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((ushort)OpCommand.TRANSACTION_DELETE)]
    public async ValueTask DeleteAsync(IPacketContext<TransactionDto> context)
    {
        TransactionDto packet = context.Packet;
        IConnection connection = context.Connection;

        if (packet.TransactionId == null)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }

        var result = await _transactionService.DeleteAsync(packet.TransactionId.Value).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        await context.OkAsync().ConfigureAwait(false);

    }

    private static TransactionDto MapToPacket(Transaction t, ushort sequenceId) => new()
    {
        SequenceId = sequenceId,
        TransactionId = t.Id,
        InvoiceId = t.InvoiceId,
        Amount = t.Amount,
        TransactionDate = t.TransactionDate,
        Type = t.Type,
        PaymentMethod = t.PaymentMethod,
        Status = t.Status,
        Description = t.Description,
        IsReversed = t.IsReversed
    };


}
