// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nalix.Common.Networking.Protocols;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoX.Gara.Application.Customers;

public sealed class CustomerAppService(IDataSessionFactory dataSessionFactory, ILogger<CustomerAppService> logger)
{
    private readonly IDataSessionFactory _dataSessionFactory = dataSessionFactory ?? throw new ArgumentNullException(nameof(dataSessionFactory));
    private readonly ILogger<CustomerAppService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<ServiceResult<(List<Customer> items, int totalCount)>> GetPageAsync(CustomerListQuery query)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var result = await session.Customers.GetPageAsync(query).ConfigureAwait(false);
            return ServiceResult<(List<Customer> items, int totalCount)>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer page.");
            return ServiceResult<(List<Customer> items, int totalCount)>.Failure("Lỗi khi lấy danh sách khách hàng.");
        }
    }

    public async Task<ServiceResult<Customer>> CreateAsync(Customer customer)
    {
        if (!CustomerValidation.IsValidName(customer.Name))
        {
            return ServiceResult<Customer>.Failure("Tên khách hàng không hợp lệ.", ProtocolReason.VALIDATION_FAILED);
        }

        if (!CustomerValidation.IsValidTaxCode(customer.TaxCode, customer.Type))
        {
            return ServiceResult<Customer>.Failure("Mã số thuế không hợp lệ.", ProtocolReason.VALIDATION_FAILED);
        }

        if (!CustomerValidation.IsValidDateOfBirth(customer.DateOfBirth))
        {
            return ServiceResult<Customer>.Failure("Ngày sinh khách hàng không hợp lệ.", ProtocolReason.VALIDATION_FAILED);
        }

        try
        {
            await using var session = _dataSessionFactory.Create();
            if (await session.Customers.ExistsByContactAsync(customer.Email, customer.PhoneNumber).ConfigureAwait(false))
            {
                return ServiceResult<Customer>.Failure("Email hoặc số điện thoại đã tồn tại.", ProtocolReason.ALREADY_EXISTS);
            }

            await session.Customers.AddAsync(customer).ConfigureAwait(false);
            await session.Customers.SaveChangesAsync().ConfigureAwait(false);

            return ServiceResult<Customer>.Success(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer.");
            return ServiceResult<Customer>.Failure("Lỗi khi tạo khách hàng.");
        }
    }

    public async Task<ServiceResult<Customer>> UpdateAsync(Customer customer)
    {
        if (!CustomerValidation.IsValidName(customer.Name))
        {
            return ServiceResult<Customer>.Failure("Tên khách hàng không hợp lệ.", ProtocolReason.VALIDATION_FAILED);
        }

        if (!CustomerValidation.IsValidTaxCode(customer.TaxCode, customer.Type))
        {
            return ServiceResult<Customer>.Failure("Mã số thuế không hợp lệ.", ProtocolReason.VALIDATION_FAILED);
        }

        if (!CustomerValidation.IsValidDateOfBirth(customer.DateOfBirth))
        {
            return ServiceResult<Customer>.Failure("Ngày sinh khách hàng không hợp lệ.", ProtocolReason.VALIDATION_FAILED);
        }

        try
        {
            await using var session = _dataSessionFactory.Create();
            var existing = await session.Customers.GetByIdAsync(customer.Id).ConfigureAwait(false);
            if (existing is null)
            {
                return ServiceResult<Customer>.Failure("Không tìm thấy khách hàng.", ProtocolReason.NOT_FOUND);
            }

            bool duplicateContact = await session.Context.Set<Customer>()
                .AsNoTracking()
                .AnyAsync(c => c.Id != customer.Id
                    && ((c.Email != null && c.Email == customer.Email)
                        || (c.PhoneNumber != null && c.PhoneNumber == customer.PhoneNumber)))
                .ConfigureAwait(false);
            if (duplicateContact)
            {
                return ServiceResult<Customer>.Failure("Email hoặc số điện thoại đã được dùng cho khách hàng khác.", ProtocolReason.ALREADY_EXISTS);
            }

            existing.Name = customer.Name;
            existing.Email = customer.Email;
            existing.PhoneNumber = customer.PhoneNumber;
            existing.Address = customer.Address;
            existing.DateOfBirth = customer.DateOfBirth;
            existing.TaxCode = customer.TaxCode;
            existing.Type = customer.Type;
            existing.Membership = customer.Membership;
            existing.Gender = customer.Gender;
            existing.Notes = customer.Notes;



            session.Customers.Update(existing);
            await session.Customers.SaveChangesAsync().ConfigureAwait(false);

            return ServiceResult<Customer>.Success(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer {Id}.", customer.Id);
            return ServiceResult<Customer>.Failure("Lỗi khi cập nhật khách hàng.");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int customerId)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var existing = await session.Customers.GetByIdAsync(customerId).ConfigureAwait(false);
            if (existing is null)
            {
                return ServiceResult<bool>.Failure("Không tìm thấy khách hàng.", ProtocolReason.NOT_FOUND);
            }

            session.Customers.Delete(existing);
            await session.Customers.SaveChangesAsync().ConfigureAwait(false);

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer {Id}.", customerId);
            return ServiceResult<bool>.Failure("Lỗi khi xóa khách hàng.");
        }
    }
}


