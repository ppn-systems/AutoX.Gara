using System;
using System.Collections.Generic;
// Copyright (c) 2026 PPN Corporation. All rights reserved.


// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Customers;

namespace AutoX.Gara.Infrastructure.Abstractions.Repositories;

/// <summary>
/// Repository interface for Vehicles (CRUD &amp; query APIs).
/// Application layer ch? g?i qua interface n�y (kh�ng access DbContext tr?c ti?p).
/// </summary>
public interface IVehicleRepository
{
    /// <summary>
    /// L?y th�ng tin xe theo Id (ch? l?y xe chua b? x�a m?m).
    /// </summary>
    System.Threading.Tasks.Task<Vehicle> GetByIdAsync(int id, System.Threading.CancellationToken ct = default);

    /// <summary>
    /// L?y danh s�ch xe theo CustomerId, h? tr? ph�n trang.
    /// Ch? tr? v? xe chua b? x�a m?m.
    /// </summary>
    /// <param name="customerId">Id c?a kh�ch h�ng.</param>
    /// <param name="page">Trang hi?n t?i (b?t d?u t? 1).</param>
    /// <param name="pageSize">S? lu?ng xe m?i trang.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Tuple g?m danh s�ch xe v� t?ng s? xe c?a customer.</returns>
    System.Threading.Tasks.Task<(List<Vehicle> Items, int TotalCount)> GetByCustomerIdAsync(
        int customerId,
        int page,
        int pageSize,
        System.Threading.CancellationToken ct = default);

    /// <summary>
    /// Ki?m tra xe d� t?n t?i chua (d?a tr�n bi?n s? ho?c s? khung/s? m�y).
    /// </summary>
    System.Threading.Tasks.Task<bool> ExistsAsync(string licensePlate, string engineNumber = null, string frameNumber = null, System.Threading.CancellationToken ct = default);

    /// <summary>Th�m m?i m?t xe.</summary>
    System.Threading.Tasks.Task AddAsync(Vehicle vehicle, System.Threading.CancellationToken ct = default);

    /// <summary>C?p nh?t th�ng tin xe.</summary>
    void Update(Vehicle vehicle);

    /// <summary>Luu m?i thay d?i xu?ng database.</summary>
    System.Threading.Tasks.Task SaveChangesAsync(System.Threading.CancellationToken ct = default);
}
