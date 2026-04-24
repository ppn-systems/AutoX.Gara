// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Application.Abstractions.Services;
using AutoX.Gara.Domain.Entities.Repairs;
using AutoX.Gara.Shared.Models;
using Microsoft.Extensions.Logging;
using Nalix.Common.Networking.Protocols;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoX.Gara.Application.Repairs;

public sealed class RepairTaskAppService(IDataSessionFactory dataSessionFactory, ILogger<RepairTaskAppService> logger) : IRepairTaskAppService
{
    private readonly IDataSessionFactory _dataSessionFactory = dataSessionFactory ?? throw new ArgumentNullException(nameof(dataSessionFactory));
    private readonly ILogger<RepairTaskAppService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<ServiceResult<(List<RepairTask> items, int totalCount)>> GetPageAsync(RepairTaskListQuery query)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var result = await session.RepairTasks.GetPageAsync(query).ConfigureAwait(false);
            return ServiceResult<(List<RepairTask> items, int totalCount)>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting repair task page.");
            return ServiceResult<(List<RepairTask> items, int totalCount)>.Failure("Lỗi khi lấy danh sách hạng mục sửa chữa.");
        }
    }

    public async Task<ServiceResult<RepairTask>> CreateAsync(RepairTask task)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            await session.RepairTasks.AddAsync(task).ConfigureAwait(false);
            await session.RepairTasks.SaveChangesAsync().ConfigureAwait(false);
            return ServiceResult<RepairTask>.Success(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating repair task.");
            return ServiceResult<RepairTask>.Failure("Lỗi khi tạo hạng mục mới.");
        }
    }

    public async Task<ServiceResult<RepairTask>> UpdateAsync(RepairTask task)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var existing = await session.RepairTasks.GetByIdAsync(task.Id).ConfigureAwait(false);
            if (existing == null)
            {
                return ServiceResult<RepairTask>.Failure("Không tìm thấy hạng mục.", ProtocolReason.NOT_FOUND);
            }

            existing.Name = task.Name;
            existing.BasePrice = task.BasePrice;
            existing.Description = task.Description;
            existing.IsActive = task.IsActive;

            session.RepairTasks.Update(existing);
            await session.RepairTasks.SaveChangesAsync().ConfigureAwait(false);
            return ServiceResult<RepairTask>.Success(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating repair task {Id}.", task.Id);
            return ServiceResult<RepairTask>.Failure("Lỗi khi cập nhật hạng mục.");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int taskId)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var existing = await session.RepairTasks.GetByIdAsync(taskId).ConfigureAwait(false);
            if (existing == null)
            {
                return ServiceResult<bool>.Failure("Không tìm thấy hạng mục.", ProtocolReason.NOT_FOUND);
            }

            existing.IsActive = false;
            session.RepairTasks.Delete(existing);
            await session.RepairTasks.SaveChangesAsync().ConfigureAwait(false);
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting repair task {Id}.", taskId);
            return ServiceResult<bool>.Failure("Lỗi khi xóa hạng mục.");
        }
    }
}
