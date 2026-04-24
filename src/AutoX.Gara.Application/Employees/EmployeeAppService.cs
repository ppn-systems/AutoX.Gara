// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nalix.Common.Networking.Protocols;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace AutoX.Gara.Application.Employees;
public sealed class EmployeeAppService(IDataSessionFactory dataSessionFactory, ILogger<EmployeeAppService> logger)
{
    private readonly IDataSessionFactory _dataSessionFactory = dataSessionFactory ?? throw new ArgumentNullException(nameof(dataSessionFactory));
    private readonly ILogger<EmployeeAppService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    public async Task<ServiceResult<(List<Employee> items, int totalCount)>> GetPageAsync(EmployeeListQuery query)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var result = await session.Employees.GetPageAsync(query).ConfigureAwait(false);
            return ServiceResult<(List<Employee> items, int totalCount)>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employee page.");
            return ServiceResult<(List<Employee> items, int totalCount)>.Failure("Lỗi hệ thống khi lấy danh sách nhân viên.");
        }
    }
    public async Task<ServiceResult<Employee>> CreateAsync(Employee employee)
    {
        if (!EmployeeValidation.IsValidName(employee.Name))
        {
            return ServiceResult<Employee>.Failure("Tên nhân viên không hợp lệ.", ProtocolReason.VALIDATION_FAILED);
        }
        if (!EmployeeValidation.IsValidDateOfBirth(employee.DateOfBirth))
        {
            return ServiceResult<Employee>.Failure("Ngày sinh không hợp lệ.", ProtocolReason.VALIDATION_FAILED);
        }
        if (!EmployeeValidation.IsValidDates(employee.StartDate, employee.EndDate))
        {
            return ServiceResult<Employee>.Failure("Khoảng thời gian làm việc không hợp lệ.", ProtocolReason.VALIDATION_FAILED);
        }
        try
        {
            await using var session = _dataSessionFactory.Create();
            var employees = session.Employees;
            if (await employees.ExistsByEmailAsync(employee.Email).ConfigureAwait(false))
            {
                return ServiceResult<Employee>.Failure("Email đã tồn tại.", ProtocolReason.ALREADY_EXISTS);
            }
            if (!string.IsNullOrWhiteSpace(employee.PhoneNumber) && await employees.ExistsByPhoneAsync(employee.PhoneNumber).ConfigureAwait(false))
            {
                return ServiceResult<Employee>.Failure("Số điện thoại đã tồn tại.", ProtocolReason.ALREADY_EXISTS);
            }
            employee.UpdateStatus();
            await employees.AddAsync(employee).ConfigureAwait(false);
            await employees.SaveChangesAsync().ConfigureAwait(false);
            return ServiceResult<Employee>.Success(employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee.");
            return ServiceResult<Employee>.Failure("Lỗi khi tạo nhân viên.");
        }
    }
    public async Task<ServiceResult<Employee>> UpdateAsync(Employee employee)
    {
        if (!EmployeeValidation.IsValidName(employee.Name))
        {
            return ServiceResult<Employee>.Failure("Tên nhân viên không hợp lệ.", ProtocolReason.VALIDATION_FAILED);
        }
        if (!EmployeeValidation.IsValidDateOfBirth(employee.DateOfBirth))
        {
            return ServiceResult<Employee>.Failure("Ngày sinh không hợp lệ.", ProtocolReason.VALIDATION_FAILED);
        }
        if (!EmployeeValidation.IsValidDates(employee.StartDate, employee.EndDate))
        {
            return ServiceResult<Employee>.Failure("Khoảng thời gian làm việc không hợp lệ.", ProtocolReason.VALIDATION_FAILED);
        }
        try
        {
            await using var session = _dataSessionFactory.Create();
            var employees = session.Employees;
            var existing = await employees.GetByIdAsync(employee.Id).ConfigureAwait(false);
            if (existing is null)
            {
                return ServiceResult<Employee>.Failure("Không tìm thấy nhân viên.", ProtocolReason.NOT_FOUND);
            }
            bool emailChanged = !string.Equals(existing.Email, employee.Email, StringComparison.OrdinalIgnoreCase);
            if (emailChanged)
            {
                bool duplicateEmail = await session.Context.Set<Employee>()
                    .AsNoTracking()
                    .AnyAsync(e => e.Id != employee.Id && e.Email == employee.Email)
                    .ConfigureAwait(false);
                if (duplicateEmail)
                {
                    return ServiceResult<Employee>.Failure("Email đã tồn tại.", ProtocolReason.ALREADY_EXISTS);
                }
            }
            bool phoneChanged = !string.Equals(existing.PhoneNumber, employee.PhoneNumber, StringComparison.OrdinalIgnoreCase);
            if (phoneChanged && !string.IsNullOrWhiteSpace(employee.PhoneNumber))
            {
                bool duplicatePhone = await session.Context.Set<Employee>()
                    .AsNoTracking()
                    .AnyAsync(e => e.Id != employee.Id && e.PhoneNumber == employee.PhoneNumber)
                    .ConfigureAwait(false);
                if (duplicatePhone)
                {
                    return ServiceResult<Employee>.Failure("Số điện thoại đã tồn tại.", ProtocolReason.ALREADY_EXISTS);
                }
            }
            // Update fields... (mapping logic moved here from Controller)
            existing.Name = employee.Name;
            existing.Email = employee.Email;
            existing.Address = employee.Address;
            existing.PhoneNumber = employee.PhoneNumber;
            existing.Gender = employee.Gender;
            existing.Position = employee.Position;
            existing.Status = employee.Status;
            existing.DateOfBirth = employee.DateOfBirth;
            existing.StartDate = employee.StartDate;
            existing.EndDate = employee.EndDate;
            existing.UpdateStatus();
            employees.Update(existing);
            await employees.SaveChangesAsync().ConfigureAwait(false);
            return ServiceResult<Employee>.Success(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee {Id}.", employee.Id);
            return ServiceResult<Employee>.Failure("Lỗi khi cập nhật nhân viên.");
        }
    }
    public async Task<ServiceResult<bool>> ChangeStatusAsync(int employeeId, EmploymentStatus status)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var existing = await session.Employees.GetByIdAsync(employeeId).ConfigureAwait(false);
            if (existing is null)
            {
                return ServiceResult<bool>.Failure("Không tìm thấy nhân viên.", ProtocolReason.NOT_FOUND);
            }
            existing.Status = status;
            session.Employees.Update(existing);
            await session.Employees.SaveChangesAsync().ConfigureAwait(false);
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing status for employee {Id}.", employeeId);
            return ServiceResult<bool>.Failure("Lỗi khi đổi trạng thái nhân viên.");
        }
    }
}
