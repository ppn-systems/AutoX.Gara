// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Contracts.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nalix.Common.Networking.Protocols;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace AutoX.Gara.Application.Vehicles;
public sealed class VehicleAppService(IDataSessionFactory dataSessionFactory, ILogger<VehicleAppService> logger)
{
    private readonly IDataSessionFactory _dataSessionFactory = dataSessionFactory ?? throw new ArgumentNullException(nameof(dataSessionFactory));
    private readonly ILogger<VehicleAppService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    public async Task<ServiceResult<Vehicle>> GetByIdAsync(int vehicleId)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var vehicle = await session.Vehicles.GetByIdAsync(vehicleId).ConfigureAwait(false);
            return vehicle == null || vehicle.DeletedAt != null
                ? ServiceResult<Vehicle>.Failure("Không tìm thấy xe.", ProtocolReason.NOT_FOUND)
                : ServiceResult<Vehicle>.Success(vehicle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vehicle {Id}.", vehicleId);
            return ServiceResult<Vehicle>.Failure("Lỗi khi lấy thông tin xe.");
        }
    }
    public async Task<ServiceResult<(List<Vehicle> items, int totalCount)>> GetByCustomerIdAsync(int customerId, int page, int pageSize)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var result = await session.Vehicles.GetByCustomerIdAsync(customerId, page, pageSize).ConfigureAwait(false);
            return ServiceResult<(List<Vehicle> items, int totalCount)>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vehicles for customer {Id}.", customerId);
            return ServiceResult<(List<Vehicle> items, int totalCount)>.Failure("Lỗi khi lấy danh sách xe của khách hàng.");
        }
    }
    public async Task<ServiceResult<Vehicle>> CreateAsync(Vehicle vehicle)
    {
        var validation = ValidateVehiclePayload(vehicle);
        if (!validation.IsSuccess)
        {
            return ServiceResult<Vehicle>.Failure(validation.ErrorMessage!, validation.Reason);
        }
        try
        {
            await using var session = _dataSessionFactory.Create();
            bool customerExists = await session.Context.Set<Customer>()
                .AsNoTracking()
                .AnyAsync(c => c.Id == vehicle.CustomerId)
                .ConfigureAwait(false);
            if (!customerExists)
            {
                return ServiceResult<Vehicle>.Failure("Không tìm thấy khách hàng của xe.", ProtocolReason.NOT_FOUND);
            }
            if (await session.Vehicles.ExistsAsync(vehicle.LicensePlate, vehicle.EngineNumber, vehicle.FrameNumber).ConfigureAwait(false))
            {
                return ServiceResult<Vehicle>.Failure("Xe đã tồn tại (trùng BKS hoặc số máy/số khung).", ProtocolReason.ALREADY_EXISTS);
            }
            await session.Vehicles.AddAsync(vehicle).ConfigureAwait(false);
            await session.Vehicles.SaveChangesAsync().ConfigureAwait(false);
            return ServiceResult<Vehicle>.Success(vehicle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vehicle.");
            return ServiceResult<Vehicle>.Failure("Lỗi khi tạo xe mới.");
        }
    }
    public async Task<ServiceResult<Vehicle>> UpdateAsync(Vehicle vehicle)
    {
        var validation = ValidateVehiclePayload(vehicle);
        if (!validation.IsSuccess)
        {
            return ServiceResult<Vehicle>.Failure(validation.ErrorMessage!, validation.Reason);
        }
        try
        {
            await using var session = _dataSessionFactory.Create();
            var existing = await session.Vehicles.GetByIdAsync(vehicle.Id).ConfigureAwait(false);
            if (existing is null || existing.DeletedAt != null)
            {
                return ServiceResult<Vehicle>.Failure("Không tìm thấy xe.", ProtocolReason.NOT_FOUND);
            }
            bool customerExists = await session.Context.Set<Customer>()
                .AsNoTracking()
                .AnyAsync(c => c.Id == vehicle.CustomerId)
                .ConfigureAwait(false);
            if (!customerExists)
            {
                return ServiceResult<Vehicle>.Failure("Không tìm thấy khách hàng của xe.", ProtocolReason.NOT_FOUND);
            }
            bool duplicateExists = await session.Context.Set<Vehicle>()
                .AsNoTracking()
                .AnyAsync(v => v.Id != vehicle.Id
                    && ((!string.IsNullOrWhiteSpace(vehicle.LicensePlate) && v.LicensePlate == vehicle.LicensePlate)
                        || (!string.IsNullOrWhiteSpace(vehicle.EngineNumber) && v.EngineNumber == vehicle.EngineNumber)
                        || (!string.IsNullOrWhiteSpace(vehicle.FrameNumber) && v.FrameNumber == vehicle.FrameNumber)))
                .ConfigureAwait(false);
            if (duplicateExists)
            {
                return ServiceResult<Vehicle>.Failure("Xe đã tồn tại (trùng BKS hoặc số máy/số khung).", ProtocolReason.ALREADY_EXISTS);
            }
            existing.CustomerId = vehicle.CustomerId;
            existing.Type = vehicle.Type;
            existing.Color = vehicle.Color;
            existing.Brand = vehicle.Brand;
            existing.Year = vehicle.Year;
            existing.Mileage = vehicle.Mileage;
            existing.Model = vehicle.Model;
            existing.LicensePlate = vehicle.LicensePlate;
            existing.EngineNumber = vehicle.EngineNumber;
            existing.FrameNumber = vehicle.FrameNumber;
            existing.RegistrationDate = vehicle.RegistrationDate;
            existing.InsuranceExpiryDate = vehicle.InsuranceExpiryDate;
            session.Vehicles.Update(existing);
            await session.Vehicles.SaveChangesAsync().ConfigureAwait(false);
            return ServiceResult<Vehicle>.Success(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle {Id}.", vehicle.Id);
            return ServiceResult<Vehicle>.Failure("Lỗi khi cập nhật thông tin xe.");
        }
    }
    public async Task<ServiceResult<bool>> DeleteAsync(int vehicleId)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var existing = await session.Vehicles.GetByIdAsync(vehicleId).ConfigureAwait(false);
            if (existing is null || existing.DeletedAt != null)
            {
                return ServiceResult<bool>.Failure("Không tìm thấy xe.", ProtocolReason.NOT_FOUND);
            }
            session.Vehicles.Delete(existing);
            await session.Vehicles.SaveChangesAsync().ConfigureAwait(false);
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vehicle {Id}.", vehicleId);
            return ServiceResult<bool>.Failure("Lỗi khi xóa xe.");
        }
    }
    private static ServiceResult<bool> ValidateVehiclePayload(Vehicle vehicle)
    {
        if (vehicle is null || vehicle.CustomerId <= 0 || string.IsNullOrWhiteSpace(vehicle.LicensePlate))
        {
            return ServiceResult<bool>.Failure("Dữ liệu xe không hợp lệ.", ProtocolReason.MALFORMED_PACKET);
        }
        if (vehicle.Year < 1900 || vehicle.Year > DateTime.UtcNow.Year + 1)
        {
            return ServiceResult<bool>.Failure("Năm sản xuất xe không hợp lệ.", ProtocolReason.VALIDATION_FAILED);
        }
        if (vehicle.Mileage < 0)
        {
            return ServiceResult<bool>.Failure("Số km đã đi không hợp lệ.", ProtocolReason.VALIDATION_FAILED);
        }
        if (vehicle.InsuranceExpiryDate.HasValue && vehicle.InsuranceExpiryDate.Value < vehicle.RegistrationDate)
        {
            return ServiceResult<bool>.Failure("Ngày hết hạn bảo hiểm không thể trước ngày đăng ký.", ProtocolReason.VALIDATION_FAILED);
        }
        return ServiceResult<bool>.Success(true);
    }
}

