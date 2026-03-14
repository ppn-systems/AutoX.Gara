// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Customers;
using AutoX.Gara.Frontend.Models.Results;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Customers;

namespace AutoX.Gara.Frontend.Abstractions;

/// <summary>
/// Abstraction cho t?t c? network operations liên quan d?n Customer.
/// ViewModel ch? ph? thu?c vào interface này — không bi?t v? <c>ReliableClient</c>.
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// L?y trang danh sách khách hàng.
    /// Cache 30 giây — không g?i request n?u dã có k?t qu? còn h?n.
    /// </summary>
    System.Threading.Tasks.Task<CustomerListResult> GetListAsync(
        System.Int32 page,
        System.Int32 pageSize,
        System.String? searchTerm = null,
        CustomerSortField sortBy = CustomerSortField.CreatedAt,
        System.Boolean sortDescending = true,
        CustomerType filterType = CustomerType.None,
        MembershipLevel filterMembership = MembershipLevel.None,
        System.Threading.CancellationToken ct = default);

    /// <summary>T?o m?i khách hàng. Server echo l?i entity dã luu trong <c>UpdatedEntity</c>.</summary>
    System.Threading.Tasks.Task<CustomerWriteResult> CreateAsync(
        CustomerDto data,
        System.Threading.CancellationToken ct = default);

    /// <summary>C?p nh?t khách hàng. Server echo l?i entity dã luu trong <c>UpdatedEntity</c>.</summary>
    System.Threading.Tasks.Task<CustomerWriteResult> UpdateAsync(
        CustomerDto data,
        System.Threading.CancellationToken ct = default);

    /// <summary>Xóa m?m khách hàng. Server tr? v? Directive NONE khi thành công.</summary>
    System.Threading.Tasks.Task<CustomerWriteResult> DeleteAsync(
        CustomerDto data,
        System.Threading.CancellationToken ct = default);
}
