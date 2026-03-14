// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Employees;
using System.Collections.Concurrent;

namespace AutoX.Gara.Frontend.Services.Employees;

public sealed record EmployeeSalaryCacheKey(
    System.Int32 Page,
    System.Int32 PageSize,
    System.String SearchTerm,
    EmployeeSalarySortField SortBy,
    System.Boolean SortDescending,
    System.Int32 FilterEmployeeId,
    SalaryType? FilterSalaryType,
    System.DateTime? FilterFromDate,
    System.DateTime? FilterToDate);

public sealed class EmployeeSalaryCacheEntry
{
    public required System.Collections.Generic.List<EmployeeSalaryDto> Salaries { get; init; }
    public required System.Int32 TotalCount { get; init; }
    public required System.DateTime ExpiresAt { get; init; }
    public System.Boolean IsExpired => System.DateTime.UtcNow >= ExpiresAt;
}

public sealed class EmployeeSalaryQueryCache : IEmployeeSalaryQueryCache
{
    private static readonly System.TimeSpan Ttl = System.TimeSpan.FromSeconds(30);
    private readonly ConcurrentDictionary<EmployeeSalaryCacheKey, EmployeeSalaryCacheEntry> _store = new();

    public System.Boolean TryGet(EmployeeSalaryCacheKey key, out EmployeeSalaryCacheEntry? entry)
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

    public void Set(EmployeeSalaryCacheKey key, System.Collections.Generic.List<EmployeeSalaryDto> salaries, System.Int32 totalCount)
    {
        _store[key] = new EmployeeSalaryCacheEntry
        {
            Salaries = salaries,
            TotalCount = totalCount,
            ExpiresAt = System.DateTime.UtcNow.Add(Ttl)
        };
    }

    public void Invalidate() => _store.Clear();
}

public interface IEmployeeSalaryQueryCache
{
    System.Boolean TryGet(EmployeeSalaryCacheKey key, out EmployeeSalaryCacheEntry? entry);
    void Set(EmployeeSalaryCacheKey key, System.Collections.Generic.List<EmployeeSalaryDto> salaries, System.Int32 totalCount);
    void Invalidate();
}

