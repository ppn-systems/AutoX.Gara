// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Employees;
using System.Collections.Concurrent;

namespace AutoX.Gara.Frontend.Services.Employees;

/// <summary>
/// Cache key for employee query parameters.
/// </summary>
public sealed record EmployeeCacheKey(
    System.Int32 Page,
    System.Int32 PageSize,
    System.String SearchTerm,
    EmployeeSortField SortBy,
    System.Boolean SortDescending,
    Position FilterPosition,
    EmploymentStatus FilterStatus,
    Gender FilterGender);

/// <summary>
/// Cache entry for employee query results.
/// </summary>
public sealed class EmployeeCacheEntry
{
    public required System.Collections.Generic.List<EmployeeDto> Employees { get; init; }
    public required System.Int32 TotalCount { get; init; }
    public required System.DateTime ExpiresAt { get; init; }
    public System.Boolean IsExpired => System.DateTime.UtcNow >= ExpiresAt;
}

/// <summary>
/// Thread-safe in-memory cache for employee queries with 30-second TTL.
/// </summary>
public sealed class EmployeeQueryCache : IEmployeeQueryCache
{
    private static readonly System.TimeSpan Ttl = System.TimeSpan.FromSeconds(30);
    private readonly ConcurrentDictionary<EmployeeCacheKey, EmployeeCacheEntry> _store = new();

    public System.Boolean TryGet(EmployeeCacheKey key, out EmployeeCacheEntry? entry)
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

    public void Set(EmployeeCacheKey key, System.Collections.Generic.List<EmployeeDto> employees, System.Int32 totalCount)
    {
        _store[key] = new EmployeeCacheEntry
        {
            Employees = employees,
            TotalCount = totalCount,
            ExpiresAt = System.DateTime.UtcNow.Add(Ttl)
        };
    }

    public void Invalidate() => _store.Clear();
}

/// <summary>
/// Abstraction for employee query cache.
/// </summary>
public interface IEmployeeQueryCache
{
    System.Boolean TryGet(EmployeeCacheKey key, out EmployeeCacheEntry? entry);
    void Set(EmployeeCacheKey key, System.Collections.Generic.List<EmployeeDto> employees, System.Int32 totalCount);
    void Invalidate();
}