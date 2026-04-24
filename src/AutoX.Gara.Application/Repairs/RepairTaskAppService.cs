// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Domain.Entities.Invoices;
using AutoX.Gara.Domain.Entities.Repairs;
using AutoX.Gara.Contracts.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nalix.Common.Networking.Protocols;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace AutoX.Gara.Application.Repairs;
public sealed class RepairTaskAppService(IDataSessionFactory dataSessionFactory, ILogger<RepairTaskAppService> logger)
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
        var validation = ValidateTaskInput(task);
        if (!validation.IsSuccess)
        {
            return ServiceResult<RepairTask>.Failure(validation.ErrorMessage!, validation.Reason);
        }
        try
        {
            await using var session = _dataSessionFactory.Create();
            var relationValidation = await ValidateRelationsAsync(session, task).ConfigureAwait(false);
            if (!relationValidation.IsSuccess)
            {
                return ServiceResult<RepairTask>.Failure(relationValidation.ErrorMessage!, relationValidation.Reason);
            }
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
        var validation = ValidateTaskInput(task);
        if (!validation.IsSuccess)
        {
            return ServiceResult<RepairTask>.Failure(validation.ErrorMessage!, validation.Reason);
        }
        try
        {
            await using var session = _dataSessionFactory.Create();
            var existing = await session.RepairTasks.GetByIdAsync(task.Id).ConfigureAwait(false);
            if (existing == null)
            {
                return ServiceResult<RepairTask>.Failure("Không tìm thấy hạng mục.", ProtocolReason.NOT_FOUND);
            }
            var relationValidation = await ValidateRelationsAsync(session, task).ConfigureAwait(false);
            if (!relationValidation.IsSuccess)
            {
                return ServiceResult<RepairTask>.Failure(relationValidation.ErrorMessage!, relationValidation.Reason);
            }
            existing.RepairOrderId = task.RepairOrderId;
            existing.EmployeeId = task.EmployeeId;
            existing.ServiceItemId = task.ServiceItemId;
            existing.Status = task.Status;
            existing.StartDate = task.StartDate;
            existing.EstimatedDuration = task.EstimatedDuration;
            existing.CompletionDate = task.CompletionDate;
            existing.IsActive = !task.CompletionDate.HasValue;
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
    private static ServiceResult<bool> ValidateTaskInput(RepairTask task)
    {
        if (task is null)
        {
            return ServiceResult<bool>.Failure("Dữ liệu hạng mục không hợp lệ.", ProtocolReason.MALFORMED_PACKET);
        }
        if (task.RepairOrderId <= 0 || task.EmployeeId <= 0 || task.ServiceItemId <= 0)
        {
            return ServiceResult<bool>.Failure("Thiếu thông tin liên kết hạng mục.", ProtocolReason.MALFORMED_PACKET);
        }
        if (task.EstimatedDuration <= 0 || task.EstimatedDuration > 1000)
        {
            return ServiceResult<bool>.Failure("Thời lượng dự kiến phải trong khoảng 0-1000 giờ.", ProtocolReason.VALIDATION_FAILED);
        }
        if (task.StartDate.HasValue && task.StartDate.Value > DateTime.UtcNow)
        {
            return ServiceResult<bool>.Failure("Ngày bắt đầu không thể ở tương lai.", ProtocolReason.VALIDATION_FAILED);
        }
        if (task.StartDate.HasValue && task.CompletionDate.HasValue && task.CompletionDate.Value < task.StartDate.Value)
        {
            return ServiceResult<bool>.Failure("Ngày hoàn thành không thể trước ngày bắt đầu.", ProtocolReason.VALIDATION_FAILED);
        }
        return ServiceResult<bool>.Success(true);
    }
    private static async Task<ServiceResult<bool>> ValidateRelationsAsync(IDataSession session, RepairTask task)
    {
        bool repairOrderExists = await session.Context.Set<RepairOrder>()
            .AsNoTracking()
            .AnyAsync(ro => ro.Id == task.RepairOrderId)
            .ConfigureAwait(false);
        if (!repairOrderExists)
        {
            return ServiceResult<bool>.Failure("Không tìm thấy lệnh sửa chữa tương ứng.", ProtocolReason.NOT_FOUND);
        }
        bool employeeExists = await session.Context.Set<Employee>()
            .AsNoTracking()
            .AnyAsync(e => e.Id == task.EmployeeId)
            .ConfigureAwait(false);
        if (!employeeExists)
        {
            return ServiceResult<bool>.Failure("Không tìm thấy nhân viên thực hiện.", ProtocolReason.NOT_FOUND);
        }
        bool serviceItemExists = await session.Context.Set<ServiceItem>()
            .AsNoTracking()
            .AnyAsync(si => si.Id == task.ServiceItemId)
            .ConfigureAwait(false);
        if (!serviceItemExists)
        {
            return ServiceResult<bool>.Failure("Không tìm thấy dịch vụ sửa chữa.", ProtocolReason.NOT_FOUND);
        }
        return ServiceResult<bool>.Success(true);
    }
}

