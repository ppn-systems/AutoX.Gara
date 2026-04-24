// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Domain.Entities.Invoices;
using AutoX.Gara.Shared.Models;
using Microsoft.Extensions.Logging;
using Nalix.Common.Networking.Protocols;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoX.Gara.Application.Inventory;

public sealed class RepairOrderAppService(IDataSessionFactory dataSessionFactory, ILogger<RepairOrderAppService> logger)
{
    private readonly IDataSessionFactory _dataSessionFactory = dataSessionFactory ?? throw new ArgumentNullException(nameof(dataSessionFactory));
    private readonly ILogger<RepairOrderAppService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<ServiceResult<(List<RepairOrder> items, int totalCount)>> GetPageAsync(RepairOrderListQuery query)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var result = await session.RepairOrders.GetPageAsync(query).ConfigureAwait(false);
            return ServiceResult<(List<RepairOrder> items, int totalCount)>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting repair order page.");
            return ServiceResult<(List<RepairOrder> items, int totalCount)>.Failure("Lỗi khi lấy danh sách lệnh sửa chữa.");
        }
    }

    public async Task<ServiceResult<RepairOrder>> CreateAsync(RepairOrder order)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();

            order.OrderDate = DateTime.UtcNow;
            if (order.ExpectedCompletionDate.HasValue && order.ExpectedCompletionDate.Value < order.OrderDate)
            {
                return ServiceResult<RepairOrder>.Failure("Ngày hoàn thành dự kiến không thể trước ngày đặt lệnh.", ProtocolReason.MALFORMED_PACKET);
            }

            await session.RepairOrders.AddAsync(order).ConfigureAwait(false);
            await session.RepairOrders.SaveChangesAsync().ConfigureAwait(false);

            return ServiceResult<RepairOrder>.Success(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating repair order.");
            return ServiceResult<RepairOrder>.Failure("Lỗi khi tạo lệnh sửa chữa mới.");
        }
    }

    public async Task<ServiceResult<RepairOrder>> UpdateAsync(RepairOrder order)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var repo = session.RepairOrders;

            var existing = await repo.GetByIdAsync(order.Id).ConfigureAwait(false);
            if (existing is null)
            {
                return ServiceResult<RepairOrder>.Failure("Không tìm thấy lệnh sửa chữa.", ProtocolReason.NOT_FOUND);
            }

            existing.VehicleId = order.VehicleId;
            existing.EmployeeId = order.EmployeeId;
            existing.Status = order.Status;
            existing.Priority = order.Priority;
            existing.Description = order.Description;
            existing.ExpectedCompletionDate = order.ExpectedCompletionDate;
            existing.CompletionDate = order.CompletionDate;

            repo.Update(existing);
            await repo.SaveChangesAsync().ConfigureAwait(false);

            return ServiceResult<RepairOrder>.Success(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating repair order {Id}.", order.Id);
            return ServiceResult<RepairOrder>.Failure("Lỗi khi cập nhật lệnh sửa chữa.");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int orderId)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var existing = await session.RepairOrders.GetByIdAsync(orderId).ConfigureAwait(false);
            if (existing is null)
            {
                return ServiceResult<bool>.Failure("Không tìm thấy lệnh sửa chữa.", ProtocolReason.NOT_FOUND);
            }

            session.RepairOrders.Delete(existing);
            await session.RepairOrders.SaveChangesAsync().ConfigureAwait(false);

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting repair order {Id}.", orderId);
            return ServiceResult<bool>.Failure("Lỗi khi xóa lệnh sửa chữa.");
        }
    }
}

