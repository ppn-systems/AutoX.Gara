// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Protocol.Billings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
namespace AutoX.Gara.Frontend.Services.Billings;
public sealed record InvoiceCacheKey(
    int Page,
    int PageSize,
    string SearchTerm,
    InvoiceSortField SortBy,
    bool SortDescending,
    int FilterCustomerId,
    PaymentStatus? FilterPaymentStatus,
    DateTime? FilterFromDate,
    DateTime? FilterToDate);
public sealed class InvoiceCacheEntry
{
    public required List<InvoiceDto> Invoices { get; init; }
    public required int TotalCount { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}
public sealed class InvoiceQueryCache
{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);
    private readonly ConcurrentDictionary<InvoiceCacheKey, InvoiceCacheEntry> _store = new();
    public bool TryGet(InvoiceCacheKey key, out InvoiceCacheEntry? entry)
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
            ExpiresAt = DateTime.UtcNow.Add(Ttl)
        };
    }
    public void Invalidate() => _store.Clear();
}

