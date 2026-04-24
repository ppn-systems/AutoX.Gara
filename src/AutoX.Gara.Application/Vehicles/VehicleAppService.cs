// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Application.Abstractions.Services;
using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Shared.Models;
using Microsoft.Extensions.Logging;
using Nalix.Common.Networking.Protocols;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoX.Gara.Application.Vehicles;

public sealed class VehicleAppService(IDataSessionFactory dataSessionFactory, ILogger<VehicleAppService> logger) : IVehicleAppService
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
        try
        {
            await using var session = _dataSessionFactory.Create();
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
        try
        {
            await using var session = _dataSessionFactory.Create();
            var existing = await session.Vehicles.GetByIdAsync(vehicle.Id).ConfigureAwait(false);
            if (existing is null || existing.DeletedAt != null)
            {
                return ServiceResult<Vehicle>.Failure("Không tìm thấy xe.", ProtocolReason.NOT_FOUND);
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
}
