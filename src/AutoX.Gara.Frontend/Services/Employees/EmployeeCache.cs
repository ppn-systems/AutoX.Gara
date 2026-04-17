using AutoX.Gara.Shared.Enums;
using System;
using System.Collections.Generic;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;

using AutoX.Gara.Domain.Enums.Employees;

using Nalix.Common.Networking.Protocols;

using AutoX.Gara.Shared.Protocol.Employees;

using System.Collections.Concurrent;

namespace AutoX.Gara.Frontend.Services.Employees;

/// <summary>

/// Cache key for employee query parameters.

/// </summary>

public sealed record EmployeeCacheKey(

    int Page,

    int PageSize,

    string SearchTerm,

    EmployeeSortField SortBy,

    bool SortDescending,

    Position FilterPosition,

    EmploymentStatus FilterStatus,

    Gender FilterGender);

/// <summary>

/// Cache entry for employee query results.

/// </summary>

public sealed class EmployeeCacheEntry

{
    public required List<EmployeeDto> Employees { get; init; }

    public required int TotalCount { get; init; }

    public required DateTime ExpiresAt { get; init; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}

/// <summary>

/// Thread-safe in-memory cache for employee queries with 30-second TTL.

/// </summary>

public sealed class EmployeeQueryCache : IEmployeeQueryCache

{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);

    private readonly ConcurrentDictionary<EmployeeCacheKey, EmployeeCacheEntry> _store = new();

    public bool TryGet(EmployeeCacheKey key, out EmployeeCacheEntry? entry)

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

    public void Set(EmployeeCacheKey key, List<EmployeeDto> employees, int totalCount)

    {
        _store[key] = new EmployeeCacheEntry

        {
            Employees = employees,

            TotalCount = totalCount,

            ExpiresAt = DateTime.UtcNow.Add(Ttl)

        };

    }

    public void Invalidate() => _store.Clear();
}

/// <summary>

/// Abstraction for employee query cache.

/// </summary>

public interface IEmployeeQueryCache

{
    bool TryGet(EmployeeCacheKey key, out EmployeeCacheEntry? entry);

    void Set(EmployeeCacheKey key, List<EmployeeDto> employees, int totalCount);

    void Invalidate();
}
