// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Domain.Entities.Invoices;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Domain.Enums.Transactions;
using AutoX.Gara.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nalix.Common.Networking.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoX.Gara.Application.Invoices;

public sealed class TransactionAppService(IDataSessionFactory dataSessionFactory, ILogger<TransactionAppService> logger)
{
    private readonly IDataSessionFactory _dataSessionFactory = dataSessionFactory ?? throw new ArgumentNullException(nameof(dataSessionFactory));
    private readonly ILogger<TransactionAppService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<ServiceResult<(List<Transaction> items, int totalCount)>> GetPageAsync(TransactionListQuery query)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var result = await session.Transactions.GetPageAsync(query).ConfigureAwait(false);
            return ServiceResult<(List<Transaction> items, int totalCount)>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction page.");
            return ServiceResult<(List<Transaction> items, int totalCount)>.Failure("Lỗi khi lấy danh sách giao dịch.");
        }
    }

    public async Task<ServiceResult<Transaction>> CreateAsync(Transaction transaction)
    {
        if (transaction.Amount <= 0)
        {
            return ServiceResult<Transaction>.Failure("Số tiền giao dịch phải lớn hơn 0.", ProtocolReason.MALFORMED_PACKET);
        }

        try
        {
            await using var session = _dataSessionFactory.Create();



            await using var tx = await session.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                // 1. Record Transaction
                transaction.TransactionDate = DateTime.UtcNow;
                await session.Transactions.AddAsync(transaction).ConfigureAwait(false);
                await session.SaveChangesAsync().ConfigureAwait(false);

                // 2. Update Invoice Status
                var invoice = await session.Invoices.GetInvoiceWithFullGraphTrackedAsync(transaction.InvoiceId).ConfigureAwait(false);
                if (invoice != null)
                {
                    // Calculate total paid across all successful transactions
                    var allTransactions = await session.Context.Set<Transaction>()
                        .Where(t => t.InvoiceId == invoice.Id && t.Status == TransactionStatus.Completed && !t.IsReversed)
                        .ToListAsync().ConfigureAwait(false);

                    var totalPaid = allTransactions.Sum(t => t.Amount);



                    if (totalPaid >= invoice.TotalAmount)
                    {
                        invoice.PaymentStatus = PaymentStatus.Paid;
                    }
                    else if (totalPaid > 0)
                    {
                        invoice.PaymentStatus = PaymentStatus.PartiallyPaid;
                    }

                    await session.SaveChangesAsync().ConfigureAwait(false);
                }

                await tx.CommitAsync().ConfigureAwait(false);
                return ServiceResult<Transaction>.Success(transaction);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync().ConfigureAwait(false);
                _logger.LogError(ex, "Transaction failed for invoice {InvoiceId}", transaction.InvoiceId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction.");
            return ServiceResult<Transaction>.Failure("Lỗi hệ thống khi tạo giao dịch.");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int transactionId)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            await using var tx = await session.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                var existing = await session.Transactions.GetByIdAsync(transactionId).ConfigureAwait(false);
                if (existing is null)
                {
                    return ServiceResult<bool>.Failure("Không tìm thấy giao dịch.", ProtocolReason.NOT_FOUND);
                }

                int invoiceId = existing.InvoiceId;
                session.Transactions.Delete(existing);
                await session.SaveChangesAsync().ConfigureAwait(false);

                // Re-calculate invoice status after deletion
                var invoice = await session.Invoices.GetInvoiceWithFullGraphTrackedAsync(invoiceId).ConfigureAwait(false);
                if (invoice != null)
                {
                    var allTransactions = await session.Context.Set<Transaction>()
                        .Where(t => t.InvoiceId == invoiceId && t.Status == TransactionStatus.Completed && !t.IsReversed)
                        .ToListAsync().ConfigureAwait(false);

                    var totalPaid = allTransactions.Sum(t => t.Amount);



                    invoice.PaymentStatus = totalPaid >= invoice.TotalAmount ? PaymentStatus.Paid : totalPaid > 0 ? PaymentStatus.PartiallyPaid : PaymentStatus.Unpaid;

                    await session.SaveChangesAsync().ConfigureAwait(false);
                }

                await tx.CommitAsync().ConfigureAwait(false);
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync().ConfigureAwait(false);
                _logger.LogError(ex, "Error deleting transaction {Id}.", transactionId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting transaction {Id}.", transactionId);
            return ServiceResult<bool>.Failure("Lỗi khi xóa giao dịch.");
        }
    }
}


