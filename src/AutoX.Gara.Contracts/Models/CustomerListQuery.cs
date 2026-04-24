// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Customers;
using AutoX.Gara.Contracts.Enums;
namespace AutoX.Gara.Contracts.Models;
/// <summary>
/// Value object ch?a t?t c? tham s? truy v?n danh s�ch kh�ch h�ng.
/// D�ng C# record d? t? sinh Equals/GetHashCode � d�ng được l�m cache key.
/// </summary>
public sealed record CustomerListQuery(
    int Page,
    int PageSize,
    string SearchTerm,
    CustomerSortField SortBy,
    bool SortDescending,
    CustomerType FilterType,
    MembershipLevel FilterMembership);

