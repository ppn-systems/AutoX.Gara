// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Billings;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutoX.Gara.Frontend.Services.Billings;

public sealed record InvoiceCacheKey(
    System.Int32 Page,
    System.Int32 PageSize,
    System.String SearchTerm,
    InvoiceSortField SortBy,
    System.Boolean SortDescending,
    System.Int32 FilterCustomerId,
    PaymentStatus? FilterPaymentStatus,
    System.DateTime? FilterFromDate,
    System.DateTime? FilterToDate);

public sealed class InvoiceCacheEntry
{
    public required List<InvoiceDto> Invoices { get; init; }
    public required System.Int32 TotalCount { get; init; }
    public required System.DateTime ExpiresAt { get; init; }
    public System.Boolean IsExpired => System.DateTime.UtcNow >= ExpiresAt;
}

public sealed class InvoiceQueryCache
{
    private static readonly System.TimeSpan Ttl = System.TimeSpan.FromSeconds(30);
    private readonly ConcurrentDictionary<InvoiceCacheKey, InvoiceCacheEntry> _store = new();

    public System.Boolean TryGet(InvoiceCacheKey key, out InvoiceCacheEntry? entry)
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

    public void Set(InvoiceCacheKey key, List<InvoiceDto> invoices, int totalCount)
    {
        _store[key] = new InvoiceCacheEntry
        {
            Invoices = invoices,
            TotalCount = totalCount,
            ExpiresAt = System.DateTime.UtcNow.Add(Ttl)
        };
    }

    public void Invalidate() => _store.Clear();
}
