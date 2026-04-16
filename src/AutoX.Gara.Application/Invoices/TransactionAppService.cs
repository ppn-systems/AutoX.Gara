using Nalix.Common.Networking.Protocols;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Application.Abstractions.Services;
using AutoX.Gara.Domain.Entities.Invoices;
using AutoX.Gara.Shared.Models;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoX.Gara.Application.Invoices;

public sealed class TransactionAppService(IDataSessionFactory dataSessionFactory, ILogger<TransactionAppService> logger) : ITransactionAppService
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
        try
        {
            await using var session = _dataSessionFactory.Create();
            
            transaction.TransactionDate = DateTime.UtcNow;
            if (transaction.Amount <= 0)
                return ServiceResult<Transaction>.Failure("Số tiền giao dịch phải lớn hơn 0.", ProtocolReason.MALFORMED_PACKET);

            await session.Transactions.AddAsync(transaction).ConfigureAwait(false);
            await session.Transactions.SaveChangesAsync().ConfigureAwait(false);

            return ServiceResult<Transaction>.Success(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction.");
            return ServiceResult<Transaction>.Failure("Lỗi khi tạo giao dịch mới.");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int transactionId)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var existing = await session.Transactions.GetByIdAsync(transactionId).ConfigureAwait(false);
            if (existing is null) return ServiceResult<bool>.Failure("Không tìm thấy giao dịch.", ProtocolReason.NOT_FOUND);

            session.Transactions.Delete(existing);
            await session.Transactions.SaveChangesAsync().ConfigureAwait(false);

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting transaction {Id}.", transactionId);
            return ServiceResult<bool>.Failure("Lỗi khi xóa giao dịch.");
        }
    }
}