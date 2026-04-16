using Nalix.Common.Networking.Protocols;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Application.Abstractions.Services;
using AutoX.Gara.Domain.Entities.Repairs;
using AutoX.Gara.Shared.Models;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoX.Gara.Application.Repairs;

public sealed class RepairOrderItemAppService(IDataSessionFactory dataSessionFactory, ILogger<RepairOrderItemAppService> logger) : IRepairOrderItemAppService
{
    private readonly IDataSessionFactory _dataSessionFactory = dataSessionFactory ?? throw new ArgumentNullException(nameof(dataSessionFactory));
    private readonly ILogger<RepairOrderItemAppService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<ServiceResult<(List<RepairOrderItem> items, int totalCount)>> GetByOrderIdAsync(int orderId)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var result = await session.RepairOrderItems.GetByOrderIdAsync(orderId).ConfigureAwait(false);
            return ServiceResult<(List<RepairOrderItem> items, int totalCount)>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting repair order items for order {Id}.", orderId);
            return ServiceResult<(List<RepairOrderItem> items, int totalCount)>.Failure("Lỗi khi lấy danh sách hạng mục của lệnh sửa chữa.");
        }
    }

    public async Task<ServiceResult<RepairOrderItem>> CreateAsync(RepairOrderItem item)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            await session.RepairOrderItems.AddAsync(item).ConfigureAwait(false);
            await session.RepairOrderItems.SaveChangesAsync().ConfigureAwait(false);
            return ServiceResult<RepairOrderItem>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating repair order item.");
            return ServiceResult<RepairOrderItem>.Failure("Lỗi khi thêm hạng mục vào lệnh sửa chữa.");
        }
    }

    public async Task<ServiceResult<RepairOrderItem>> UpdateAsync(RepairOrderItem item)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var existing = await session.RepairOrderItems.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (existing == null) return ServiceResult<RepairOrderItem>.Failure("Không tìm thấy hạng mục.", ProtocolReason.NOT_FOUND);

            existing.Quantity = item.Quantity;

            session.RepairOrderItems.Update(existing);
            await session.RepairOrderItems.SaveChangesAsync().ConfigureAwait(false);
            return ServiceResult<RepairOrderItem>.Success(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating repair order item {Id}.", item.Id);
            return ServiceResult<RepairOrderItem>.Failure("Lỗi khi cập nhật hạng mục.");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int itemId)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var existing = await session.RepairOrderItems.GetByIdAsync(itemId).ConfigureAwait(false);
            if (existing == null) return ServiceResult<bool>.Failure("Không tìm thấy hạng mục.", ProtocolReason.NOT_FOUND);

            session.RepairOrderItems.Delete(existing);
            await session.RepairOrderItems.SaveChangesAsync().ConfigureAwait(false);
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting repair order item {Id}.", itemId);
            return ServiceResult<bool>.Failure("Lỗi khi xóa hạng mục.");
        }
    }
}