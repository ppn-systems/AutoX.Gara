// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Customers;

namespace AutoX.Gara.Infrastructure.Abstractions;

/// <summary>
/// Repository interface for Vehicles (CRUD & query APIs).
/// Application layer chỉ gọi qua interface này (không access DbContext trực tiếp).
/// </summary>
public interface IVehicleRepository
{
    /// <summary>
    /// Lấy thông tin xe theo Id (chỉ lấy xe chưa bị xóa mềm).
    /// </summary>
    /// <param name="id">Vehicle Id.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Xe tìm được hoặc null nếu không có.</returns>
    System.Threading.Tasks.Task<Vehicle> GetByIdAsync(System.Int32 id, System.Threading.CancellationToken ct = default);

    /// <summary>
    /// Kiểm tra xe đã tồn tại chưa (dựa trên biển số hoặc số khung/số máy).
    /// </summary>
    /// <param name="licensePlate">Biển số xe.</param>
    /// <param name="engineNumber">Số máy (optional).</param>
    /// <param name="frameNumber">Số khung (optional).</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>true nếu đã tồn tại, false nếu chưa.</returns>
    System.Threading.Tasks.Task<System.Boolean> ExistsAsync(System.String licensePlate, System.String engineNumber = null, System.String frameNumber = null, System.Threading.CancellationToken ct = default);

    /// <summary>
    /// Thêm mới một xe.
    /// </summary>
    System.Threading.Tasks.Task AddAsync(Vehicle vehicle, System.Threading.CancellationToken ct = default);

    /// <summary>
    /// Cập nhật thông tin xe.
    /// </summary>
    void Update(Vehicle vehicle);

    /// <summary>
    /// Lưu mọi thay đổi xuống database.
    /// </summary>
    System.Threading.Tasks.Task SaveChangesAsync(System.Threading.CancellationToken ct = default);
}