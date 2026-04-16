using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Domain.Entities.Repairs;
using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoX.Gara.Application.Abstractions.Services;

public interface IServiceItemAppService
{
    Task<ServiceResult<(List<ServiceItem> items, int totalCount)>> GetPageAsync(ServiceItemListQuery query);
    Task<ServiceResult<ServiceItem>> CreateAsync(ServiceItem item);
    Task<ServiceResult<ServiceItem>> UpdateAsync(ServiceItem item);
    Task<ServiceResult<bool>> DeleteAsync(int itemId);
}

public interface IRepairTaskAppService
{
    Task<ServiceResult<(List<RepairTask> items, int totalCount)>> GetPageAsync(RepairTaskListQuery query);
    Task<ServiceResult<RepairTask>> CreateAsync(RepairTask task);
    Task<ServiceResult<RepairTask>> UpdateAsync(RepairTask task);
    Task<ServiceResult<bool>> DeleteAsync(int taskId);
}

public interface IRepairOrderItemAppService
{
    Task<ServiceResult<(List<RepairOrderItem> items, int totalCount)>> GetByOrderIdAsync(int orderId);
    Task<ServiceResult<RepairOrderItem>> CreateAsync(RepairOrderItem item);
    Task<ServiceResult<RepairOrderItem>> UpdateAsync(RepairOrderItem item);
    Task<ServiceResult<bool>> DeleteAsync(int itemId);
}