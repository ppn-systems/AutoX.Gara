// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Customers;

namespace AutoX.Gara.Frontend.Services.Customers;

/// <summary>
/// Key duy nhất cho một tập tham số truy vấn.
/// C# record tự sinh <c>Equals</c> + <c>GetHashCode</c> đúng —
/// dùng được trực tiếp làm key của <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// </summary>
public sealed record CustomerCacheKey(
    System.Int32 Page,
    System.Int32 PageSize,
    System.String SearchTerm,
    CustomerSortField SortBy,
    System.Boolean SortDescending,
    CustomerType FilterType,
    MembershipLevel FilterMembership);