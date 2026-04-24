// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Persistence;using AutoX.Gara.Application.Abstractions.Services;using AutoX.Gara.Domain.Entities.Inventory;using AutoX.Gara.Shared.Models;using AutoX.Gara.Shared.Validation;using Microsoft.Extensions.Logging;using Nalix.Common.Networking.Protocols;using System;using System.Collections.Generic;using System.Threading.Tasks;

namespace AutoX.Gara.Application.Inventory;

public sealed class PartAppService(IDataSessionFactory dataSessionFactory, ILogger<PartAppService> logger) : IPartAppService
{
    private readonly IDataSessionFactory _dataSessionFactory = dataSessionFactory ?? throw new ArgumentNullException(nameof(dataSessionFactory));
    private readonly ILogger<PartAppService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<ServiceResult<(List<Part> items, int totalCount)>> GetPageAsync(PartListQuery query)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var result = await session.Parts.GetPageAsync(query).ConfigureAwait(false);
            return ServiceResult<(List<Part> items, int totalCount)>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting part page.");
            return ServiceResult<(List<Part> items, int totalCount)>.Failure("Lỗi khi lấy danh sách phụ tùng.");
        }
    }

    public async Task<ServiceResult<Part>> CreateAsync(Part part)
    {
        if (!PartValidation.IsValidName(part.PartName))
        {
            return ServiceResult<Part>.Failure("Tên phụ tùng không hợp lệ.", ProtocolReason.VALIDATION_FAILED);
        }

        if (!PartValidation.IsValidPrice(part.PurchasePrice, part.SellingPrice))
        {
            return ServiceResult<Part>.Failure("Giá bán không được nhỏ hơn giá nhập.", ProtocolReason.VALIDATION_FAILED);
        }

        try
        {
            await using var session = _dataSessionFactory.Create();
            if (await session.Parts.ExistsByPartCodeAsync(part.PartCode).ConfigureAwait(false))
            {
                return ServiceResult<Part>.Failure("Mã phụ tùng đã tồn tại.", ProtocolReason.ALREADY_EXISTS);
            }

            await session.Parts.AddAsync(part).ConfigureAwait(false);
            await session.Parts.SaveChangesAsync().ConfigureAwait(false);

            return ServiceResult<Part>.Success(part);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating part.");
            return ServiceResult<Part>.Failure("Lỗi khi tạo phụ tùng mới.");
        }
    }

    public async Task<ServiceResult<Part>> UpdateAsync(Part part)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var existing = await session.Parts.GetByIdAsync(part.Id).ConfigureAwait(false);
            if (existing is null)
            {
                return ServiceResult<Part>.Failure("Không tìm thấy phụ tùng.", ProtocolReason.NOT_FOUND);
            }

            existing.PartName = part.PartName;
            existing.Manufacturer = part.Manufacturer;
            existing.PartCategory = part.PartCategory;
            existing.PurchasePrice = part.PurchasePrice;
            existing.SellingPrice = part.SellingPrice;
            existing.InventoryQuantity = part.InventoryQuantity;
            existing.DateAdded = part.DateAdded;
            existing.ExpiryDate = part.ExpiryDate;
            existing.IsDiscontinued = part.IsDiscontinued;

            if (part.IsDefective && !existing.IsDefective)
            {
                existing.MarkAsDefective();
            }
            else if (!part.IsDefective && existing.IsDefective)
            {
                existing.UnmarkAsDefective();
            }

            session.Parts.Update(existing);
            await session.Parts.SaveChangesAsync().ConfigureAwait(false);

            return ServiceResult<Part>.Success(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating part {Id}.", part.Id);
            return ServiceResult<Part>.Failure("Lỗi khi cập nhật phụ tùng.");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int partId)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var existing = await session.Parts.GetByIdAsync(partId).ConfigureAwait(false);
            if (existing is null)
            {
                return ServiceResult<bool>.Failure("Không tìm thấy phụ tùng.", ProtocolReason.NOT_FOUND);
            }

            existing.IsDiscontinued = true;
            session.Parts.Delete(existing);
            await session.Parts.SaveChangesAsync().ConfigureAwait(false);

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting part {Id}.", partId);
            return ServiceResult<bool>.Failure("Lỗi khi xóa phụ tùng.");
        }
    }
}
