using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Domain.Entities.Inventory;
using AutoX.Gara.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoX.Gara.Application.Abstractions.Services;

public interface ICustomerAppService
{
    Task<ServiceResult<(List<Customer> items, int totalCount)>> GetPageAsync(CustomerListQuery query);
    Task<ServiceResult<Customer>> CreateAsync(Customer customer);
    Task<ServiceResult<Customer>> UpdateAsync(Customer customer);
    Task<ServiceResult<bool>> DeleteAsync(int customerId);
}

public interface IVehicleAppService
{
    Task<ServiceResult<Vehicle>> GetByIdAsync(int vehicleId);
    Task<ServiceResult<(List<Vehicle> items, int totalCount)>> GetByCustomerIdAsync(int customerId, int page, int pageSize);
    Task<ServiceResult<Vehicle>> CreateAsync(Vehicle vehicle);
    Task<ServiceResult<Vehicle>> UpdateAsync(Vehicle vehicle);
    Task<ServiceResult<bool>> DeleteAsync(int vehicleId);
}

public interface IPartAppService
{
    Task<ServiceResult<(List<Part> items, int totalCount)>> GetPageAsync(PartListQuery query);
    Task<ServiceResult<Part>> CreateAsync(Part part);
    Task<ServiceResult<Part>> UpdateAsync(Part part);
    Task<ServiceResult<bool>> DeleteAsync(int partId);
}
