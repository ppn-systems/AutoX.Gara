using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Payments;

using AutoX.Gara.Domain.Enums.Transactions;

using Nalix.Common.Networking.Protocols;

using AutoX.Gara.Shared.Protocol.Invoices;

using System.Collections.Concurrent;

using System.Collections.Generic;

namespace AutoX.Gara.Frontend.Services.Invoices;

public sealed record TransactionCacheKey(

    int Page,

    int PageSize,

    string SearchTerm,

    TransactionSortField SortBy,

    bool SortDescending,

    int FilterInvoiceId,

    TransactionType? FilterType,

    TransactionStatus? FilterStatus,

    PaymentMethod? FilterPaymentMethod);

public sealed class TransactionCacheEntry

{
    public required List<TransactionDto> Transactions { get; init; }

    public required int TotalCount { get; init; }

    public required DateTime ExpiresAt { get; init; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}

public sealed class TransactionQueryCache

{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);

    private readonly ConcurrentDictionary<TransactionCacheKey, TransactionCacheEntry> _store = new();

    public bool TryGet(TransactionCacheKey key, out TransactionCacheEntry? entry)

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

            ExpiresAt = DateTime.UtcNow.Add(Ttl)

        };

    }

    public void Invalidate() => _store.Clear();
}
