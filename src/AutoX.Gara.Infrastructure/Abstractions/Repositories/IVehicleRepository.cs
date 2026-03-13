// Copyright (c) 2026 PPN Corporation. All rights reserved.


// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Customers;

namespace AutoX.Gara.Infrastructure.Abstractions.Repositories;

/// <summary>
/// Repository interface for Vehicles (CRUD &amp; query APIs).
/// Application layer chỉ gọi qua interface này (không access DbContext trực tiếp).
/// </summary>
public interface IVehicleRepository
{
    /// <summary>
    /// Lấy thông tin xe theo Id (chỉ lấy xe chưa bị xóa mềm).
    /// </summary>
    System.Threading.Tasks.Task<Vehicle> GetByIdAsync(System.Int32 id, System.Threading.CancellationToken ct = default);

    /// <summary>
    /// Lấy danh sách xe theo CustomerId, hỗ trợ phân trang.
    /// Chỉ trả về xe chưa bị xóa mềm.
    /// </summary>
    /// <param name="customerId">Id của khách hàng.</param>
    /// <param name="page">Trang hiện tại (bắt đầu từ 1).</param>
    /// <param name="pageSize">Số lượng xe mỗi trang.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Tuple gồm danh sách xe và tổng số xe của customer.</returns>
    System.Threading.Tasks.Task<(System.Collections.Generic.List<Vehicle> Items, System.Int32 TotalCount)> GetByCustomerIdAsync(
        System.Int32 customerId,
        System.Int32 page,
        System.Int32 pageSize,
        System.Threading.CancellationToken ct = default);

    /// <summary>
    /// Kiểm tra xe đã tồn tại chưa (dựa trên biển số hoặc số khung/số máy).
    /// </summary>
    System.Threading.Tasks.Task<System.Boolean> ExistsAsync(System.String licensePlate, System.String engineNumber = null, System.String frameNumber = null, System.Threading.CancellationToken ct = default);

    /// <summary>Thêm mới một xe.</summary>
    System.Threading.Tasks.Task AddAsync(Vehicle vehicle, System.Threading.CancellationToken ct = default);

    /// <summary>Cập nhật thông tin xe.</summary>
    void Update(Vehicle vehicle);

    /// <summary>Lưu mọi thay đổi xuống database.</summary>
    System.Threading.Tasks.Task SaveChangesAsync(System.Threading.CancellationToken ct = default);
}