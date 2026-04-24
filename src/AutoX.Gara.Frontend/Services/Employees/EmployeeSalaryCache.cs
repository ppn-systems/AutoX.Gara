// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Employees;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
namespace AutoX.Gara.Frontend.Services.Employees;
public sealed record EmployeeSalaryCacheKey(
    int Page,
    int PageSize,
    string SearchTerm,
    EmployeeSalarySortField SortBy,
    bool SortDescending,
    int FilterEmployeeId,
    SalaryType? FilterSalaryType,
    DateTime? FilterFromDate,
    DateTime? FilterToDate);
public sealed class EmployeeSalaryCacheEntry
{
    public required List<EmployeeSalaryDto> Salaries { get; init; }
    public required int TotalCount { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}
public sealed class EmployeeSalaryQueryCache : IEmployeeSalaryQueryCache
{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);
    private readonly ConcurrentDictionary<EmployeeSalaryCacheKey, EmployeeSalaryCacheEntry> _store = new();
    public bool TryGet(EmployeeSalaryCacheKey key, out EmployeeSalaryCacheEntry? entry)
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
    public void Set(EmployeeSalaryCacheKey key, List<EmployeeSalaryDto> salaries, int totalCount)
    {
        _store[key] = new EmployeeSalaryCacheEntry
        {
            Salaries = salaries,
            TotalCount = totalCount,
            ExpiresAt = DateTime.UtcNow.Add(Ttl)
        };
    }
    public void Invalidate() => _store.Clear();
}
public interface IEmployeeSalaryQueryCache
{
    bool TryGet(EmployeeSalaryCacheKey key, out EmployeeSalaryCacheEntry? entry);
    void Set(EmployeeSalaryCacheKey key, List<EmployeeSalaryDto> salaries, int totalCount);
    void Invalidate();
}


