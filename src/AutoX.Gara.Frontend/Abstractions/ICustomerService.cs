// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Customers;
using AutoX.Gara.Frontend.Models.Results.Customer;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Customers;
namespace AutoX.Gara.Frontend.Abstractions;
/// <summary>
/// Abstraction cho T?t c? network operations li�n quan d?n Customer.
/// ViewModel ch? ph? thu?c v�o interface n�y � kh�ng bi?t v? <c>TcpSession</c>.
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// L?y trang danh s�ch kh�ch h�ng.
    /// Cache 30 gi�y � kh�ng g?i request n?u d� c� k?t qu? C�n h�ng.
    /// </summary>
    System.Threading.Tasks.Task<CustomerListResult> GetListAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        CustomerSortField sortBy = CustomerSortField.CreatedAt,
        bool sortDescending = true,
        CustomerType filterType = CustomerType.None,
        MembershipLevel filterMembership = MembershipLevel.None,
        System.Threading.CancellationToken ct = default);
    /// <summary>T?o m?i kh�ch h�ng. Server echo l?i entity d� luu trong <c>UpdatedEntity</c>.</summary>
    System.Threading.Tasks.Task<CustomerWriteResult> CreateAsync(
        CustomerDto data,
        System.Threading.CancellationToken ct = default);
    /// <summary>C?p nh?t kh�ch h�ng. Server echo l?i entity d� luu trong <c>UpdatedEntity</c>.</summary>
    System.Threading.Tasks.Task<CustomerWriteResult> UpdateAsync(
        CustomerDto data,
        System.Threading.CancellationToken ct = default);
    /// <summary>X�a m?m kh�ch h�ng. Server tr? v? Directive NONE khi th�nh c�ng.</summary>
    System.Threading.Tasks.Task<CustomerWriteResult> DeleteAsync(
        CustomerDto data,
        System.Threading.CancellationToken ct = default);
}


