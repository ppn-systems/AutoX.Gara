// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Domain.Enums.Transactions;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Billings;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutoX.Gara.Frontend.Services.Invoices;

public sealed record TransactionCacheKey(
    System.Int32 Page,
    System.Int32 PageSize,
    System.String SearchTerm,
    TransactionSortField SortBy,
    System.Boolean SortDescending,
    System.Int32 FilterInvoiceId,
    TransactionType? FilterType,
    TransactionStatus? FilterStatus,
    PaymentMethod? FilterPaymentMethod);

public sealed class TransactionCacheEntry
{
    public required List<TransactionDto> Transactions { get; init; }
    public required System.Int32 TotalCount { get; init; }
    public required System.DateTime ExpiresAt { get; init; }
    public System.Boolean IsExpired => System.DateTime.UtcNow >= ExpiresAt;
}

public sealed class TransactionQueryCache
{
    private static readonly System.TimeSpan Ttl = System.TimeSpan.FromSeconds(30);
    private readonly ConcurrentDictionary<TransactionCacheKey, TransactionCacheEntry> _store = new();

    public System.Boolean TryGet(TransactionCacheKey key, out TransactionCacheEntry? entry)
    {
        if (_store.TryGetValue(key, out entry) && !entry.IsExpired)
        {
            return true;
        }

        if (entry is not null)
        {
            _store.TryRemove(key, out _);
        }

        entry = null;
        return false;
    }

    public void Set(TransactionCacheKey key, List<TransactionDto> transactions, int totalCount)
    {
        _store[key] = new TransactionCacheEntry
        {
            Transactions = transactions,
            TotalCount = totalCount,
            ExpiresAt = System.DateTime.UtcNow.Add(Ttl)
        };
    }

    public void Invalidate() => _store.Clear();
}

