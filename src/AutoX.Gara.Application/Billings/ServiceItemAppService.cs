// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Contracts.Models;
using Microsoft.Extensions.Logging;
using Nalix.Common.Networking.Protocols;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace AutoX.Gara.Application.Billings;
public sealed class ServiceItemAppService(IDataSessionFactory dataSessionFactory, ILogger<ServiceItemAppService> logger)
{
    private readonly IDataSessionFactory _dataSessionFactory = dataSessionFactory ?? throw new ArgumentNullException(nameof(dataSessionFactory));
    private readonly ILogger<ServiceItemAppService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    public async Task<ServiceResult<(List<ServiceItem> items, int totalCount)>> GetPageAsync(ServiceItemListQuery query)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var result = await session.ServiceItems.GetPageAsync(query).ConfigureAwait(false);
            return ServiceResult<(List<ServiceItem> items, int totalCount)>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service item page.");
            return ServiceResult<(List<ServiceItem> items, int totalCount)>.Failure("Lỗi khi lấy danh sách dịch vụ.");
        }
    }
    public async Task<ServiceResult<ServiceItem>> CreateAsync(ServiceItem item)
    {
        if (item is null || string.IsNullOrWhiteSpace(item.Description))
        {
            return ServiceResult<ServiceItem>.Failure("Dữ liệu dịch vụ không hợp lệ.", ProtocolReason.MALFORMED_PACKET);
        }
        if (item.UnitPrice < 0)
        {
            return ServiceResult<ServiceItem>.Failure("Đơn giá dịch vụ không hợp lệ.", ProtocolReason.VALIDATION_FAILED);
        }
        try
        {
            await using var session = _dataSessionFactory.Create();
            await session.ServiceItems.AddAsync(item).ConfigureAwait(false);
            await session.ServiceItems.SaveChangesAsync().ConfigureAwait(false);
            return ServiceResult<ServiceItem>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service item.");
            return ServiceResult<ServiceItem>.Failure("Lỗi khi tạo dịch vụ.");
        }
    }
    public async Task<ServiceResult<ServiceItem>> UpdateAsync(ServiceItem item)
    {
        if (item is null || item.Id <= 0 || string.IsNullOrWhiteSpace(item.Description))
        {
            return ServiceResult<ServiceItem>.Failure("Dữ liệu dịch vụ không hợp lệ.", ProtocolReason.MALFORMED_PACKET);
        }
        if (item.UnitPrice < 0)
        {
            return ServiceResult<ServiceItem>.Failure("Đơn giá dịch vụ không hợp lệ.", ProtocolReason.VALIDATION_FAILED);
        }
        try
        {
            await using var session = _dataSessionFactory.Create();
            var existing = await session.ServiceItems.GetByIdAsync(item.Id).ConfigureAwait(false);
            if (existing == null)
            {
                return ServiceResult<ServiceItem>.Failure("Không tìm thấy dịch vụ.", ProtocolReason.NOT_FOUND);
            }
            existing.Description = item.Description;
            existing.Type = item.Type;
            existing.UnitPrice = item.UnitPrice;
            session.ServiceItems.Update(existing);
            await session.ServiceItems.SaveChangesAsync().ConfigureAwait(false);
            return ServiceResult<ServiceItem>.Success(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service item {Id}.", item.Id);
            return ServiceResult<ServiceItem>.Failure("Lỗi khi cập nhật dịch vụ.");
        }
    }
    public async Task<ServiceResult<bool>> DeleteAsync(int itemId)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var existing = await session.ServiceItems.GetByIdAsync(itemId).ConfigureAwait(false);
            if (existing == null)
            {
                return ServiceResult<bool>.Failure("Không tìm thấy dịch vụ.", ProtocolReason.NOT_FOUND);
            }
            session.ServiceItems.Delete(existing);
            await session.ServiceItems.SaveChangesAsync().ConfigureAwait(false);
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting service item {Id}.", itemId);
            return ServiceResult<bool>.Failure("Lỗi khi xóa dịch vụ.");
        }
    }
}

