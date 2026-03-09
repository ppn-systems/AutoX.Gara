// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.ViewModels.Results;
using AutoX.Gara.Shared.Packets.Customers;

namespace AutoX.Gara.Frontend.Abstractions;

/// <summary>
/// Abstraction for all customer CRUD network operations.
/// Decoupled from ViewModel for testability and replaceability.
/// </summary>
public interface ICustomerService
{
    /// <summary>Fetches a paginated list of customers from the server.</summary>
    System.Threading.Tasks.Task<CustomerListResult> GetListAsync(System.Int32 page, System.Int32 pageSize, System.Threading.CancellationToken ct = default);

    /// <summary>Creates a new customer record on the server.</summary>
    System.Threading.Tasks.Task<CustomerWriteResult> CreateAsync(CustomerDataPacket data, System.Threading.CancellationToken ct = default);

    /// <summary>Updates an existing customer record on the server.</summary>
    System.Threading.Tasks.Task<CustomerWriteResult> UpdateAsync(CustomerDataPacket data, System.Threading.CancellationToken ct = default);

    /// <summary>Deletes a customer record from the server by ID.</summary>
    System.Threading.Tasks.Task<CustomerWriteResult> DeleteAsync(CustomerDataPacket data, System.Threading.CancellationToken ct = default);
}