// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Persistence;using AutoX.Gara.Application.Abstractions.Services;using AutoX.Gara.Domain.Entities.Suppliers;using AutoX.Gara.Shared.Models;using AutoX.Gara.Shared.Validation;using Microsoft.Extensions.Logging;using Nalix.Common.Networking.Protocols;using System;using System.Collections.Generic;using System.Threading.Tasks;

namespace AutoX.Gara.Application.Suppliers;

public sealed class SupplierAppService(IDataSessionFactory dataSessionFactory, ILogger<SupplierAppService> logger) : ISupplierAppService
{
    private readonly IDataSessionFactory _dataSessionFactory = dataSessionFactory ?? throw new ArgumentNullException(nameof(dataSessionFactory));
    private readonly ILogger<SupplierAppService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<ServiceResult<(List<Supplier> items, int totalCount)>> GetPageAsync(SupplierListQuery query)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var result = await session.Suppliers.GetPageAsync(query).ConfigureAwait(false);
            return ServiceResult<(List<Supplier> items, int totalCount)>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting supplier page.");
            return ServiceResult<(List<Supplier> items, int totalCount)>.Failure("Lỗi khi lấy danh sách nhà cung cấp.");
        }
    }

    public async Task<ServiceResult<Supplier>> CreateAsync(Supplier supplier)
    {
        if (!SupplierValidation.IsValidName(supplier.Name))
        {
            return ServiceResult<Supplier>.Failure("Tên nhà cung cấp không hợp lệ.", ProtocolReason.VALIDATION_FAILED);
        }

        try
        {
            await using var session = _dataSessionFactory.Create();
            if (await session.Suppliers.ExistsByDetailsAsync(supplier.Name, supplier.Email, supplier.PhoneNumber).ConfigureAwait(false))
            {
                return ServiceResult<Supplier>.Failure("Nhà cung cấp đã tồn tại (trùng tên, email hoặc số điện thoại).", ProtocolReason.ALREADY_EXISTS);
            }

            await session.Suppliers.AddAsync(supplier).ConfigureAwait(false);
            await session.Suppliers.SaveChangesAsync().ConfigureAwait(false);

            return ServiceResult<Supplier>.Success(supplier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating supplier.");
            return ServiceResult<Supplier>.Failure("Lỗi khi tạo nhà cung cấp mới.");
        }
    }

    public async Task<ServiceResult<Supplier>> UpdateAsync(Supplier supplier)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var repo = session.Suppliers;

            var existing = await repo.GetByIdAsync(supplier.Id).ConfigureAwait(false);
            if (existing is null)
            {
                return ServiceResult<Supplier>.Failure("Không tìm thấy nhà cung cấp.", ProtocolReason.NOT_FOUND);
            }

            existing.Name = supplier.Name;
            existing.Email = supplier.Email;
            existing.PhoneNumber = supplier.PhoneNumber;
            existing.Address = supplier.Address;
            existing.TaxCode = supplier.TaxCode;
            existing.ContactPerson = supplier.ContactPerson;
            existing.Notes = supplier.Notes;
            existing.IsActive = supplier.IsActive;

            repo.Update(existing);
            await repo.SaveChangesAsync().ConfigureAwait(false);

            return ServiceResult<Supplier>.Success(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating supplier {Id}.", supplier.Id);
            return ServiceResult<Supplier>.Failure("Lỗi khi cập nhật nhà cung cấp.");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int supplierId)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var existing = await session.Suppliers.GetByIdAsync(supplierId).ConfigureAwait(false);
            if (existing is null)
            {
                return ServiceResult<bool>.Failure("Không tìm thấy nhà cung cấp.", ProtocolReason.NOT_FOUND);
            }

            existing.IsActive = false;
            session.Suppliers.Delete(existing);
            await session.Suppliers.SaveChangesAsync().ConfigureAwait(false);

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting supplier {Id}.", supplierId);
            return ServiceResult<bool>.Failure("Lỗi khi xóa nhà cung cấp.");
        }
    }
}
