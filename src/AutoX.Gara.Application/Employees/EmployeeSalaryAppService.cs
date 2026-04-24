// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nalix.Common.Networking.Protocols;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoX.Gara.Application.Employees;

public sealed class EmployeeSalaryAppService(IDataSessionFactory dataSessionFactory, ILogger<EmployeeSalaryAppService> logger)
{
    private readonly IDataSessionFactory _dataSessionFactory = dataSessionFactory ?? throw new ArgumentNullException(nameof(dataSessionFactory));
    private readonly ILogger<EmployeeSalaryAppService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<ServiceResult<(List<EmployeeSalary> items, int totalCount)>> GetPageAsync(EmployeeSalaryListQuery query)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var result = await session.EmployeeSalaries.GetPageAsync(query).ConfigureAwait(false);
            return ServiceResult<(List<EmployeeSalary> items, int totalCount)>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employee salary page.");
            return ServiceResult<(List<EmployeeSalary> items, int totalCount)>.Failure("Lỗi hệ thống khi lấy bảng lương.");
        }
    }

    public async Task<ServiceResult<EmployeeSalary>> CreateAsync(EmployeeSalary salary)
    {
        var validation = ValidateSalaryPayload(salary);
        if (!validation.IsSuccess)
        {
            return ServiceResult<EmployeeSalary>.Failure(validation.ErrorMessage!, validation.Reason);
        }

        try
        {
            await using var session = _dataSessionFactory.Create();

            // Validate Employee existence
            if (!await session.Context.Set<Employee>().AsNoTracking().AnyAsync(e => e.Id == salary.EmployeeId).ConfigureAwait(false))
            {
                return ServiceResult<EmployeeSalary>.Failure("Không tìm thấy nhân viên tương ứng.", ProtocolReason.NOT_FOUND);
            }

            await session.EmployeeSalaries.AddAsync(salary).ConfigureAwait(false);
            await session.EmployeeSalaries.SaveChangesAsync().ConfigureAwait(false);

            return ServiceResult<EmployeeSalary>.Success(salary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating salary record.");
            return ServiceResult<EmployeeSalary>.Failure("Lỗi khi tạo bản ghi lương.");
        }
    }

    public async Task<ServiceResult<EmployeeSalary>> UpdateAsync(EmployeeSalary salary)
    {
        var validation = ValidateSalaryPayload(salary);
        if (!validation.IsSuccess)
        {
            return ServiceResult<EmployeeSalary>.Failure(validation.ErrorMessage!, validation.Reason);
        }

        try
        {
            await using var session = _dataSessionFactory.Create();
            var repo = session.EmployeeSalaries;

            var existing = await repo.GetByIdAsync(salary.Id).ConfigureAwait(false);
            if (existing is null)
            {
                return ServiceResult<EmployeeSalary>.Failure("Không tìm thấy bản ghi lương.", ProtocolReason.NOT_FOUND);
            }

            // Validate Employee if changed
            if (existing.EmployeeId != salary.EmployeeId)
            {
                if (!await session.Context.Set<Employee>().AsNoTracking().AnyAsync(e => e.Id == salary.EmployeeId).ConfigureAwait(false))
                {
                    return ServiceResult<EmployeeSalary>.Failure("Không tìm thấy nhân viên mới.", ProtocolReason.NOT_FOUND);
                }
            }

            existing.EmployeeId = salary.EmployeeId;
            existing.Salary = salary.Salary;
            existing.SalaryType = salary.SalaryType;
            existing.SalaryUnit = salary.SalaryUnit;
            existing.EffectiveFrom = salary.EffectiveFrom;
            existing.EffectiveTo = salary.EffectiveTo;
            existing.Note = salary.Note;

            repo.Update(existing);
            await repo.SaveChangesAsync().ConfigureAwait(false);

            return ServiceResult<EmployeeSalary>.Success(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating salary record {Id}.", salary.Id);
            return ServiceResult<EmployeeSalary>.Failure("Lỗi khi cập nhật bản ghi lương.");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int salaryId)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var existing = await session.EmployeeSalaries.GetByIdAsync(salaryId).ConfigureAwait(false);
            if (existing is null)
            {
                return ServiceResult<bool>.Failure("Không tìm thấy bản ghi lương.", ProtocolReason.NOT_FOUND);
            }

            session.EmployeeSalaries.Delete(existing);
            await session.EmployeeSalaries.SaveChangesAsync().ConfigureAwait(false);

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting salary record {Id}.", salaryId);
            return ServiceResult<bool>.Failure("Lỗi khi xóa bản ghi lương.");
        }
    }

    private static ServiceResult<bool> ValidateSalaryPayload(EmployeeSalary salary)
    {
        if (salary is null || salary.EmployeeId <= 0)
        {
            return ServiceResult<bool>.Failure("Dữ liệu lương không hợp lệ.", ProtocolReason.MALFORMED_PACKET);
        }

        if (salary.Salary <= 0)
        {
            return ServiceResult<bool>.Failure("Mức lương phải lớn hơn 0.", ProtocolReason.VALIDATION_FAILED);
        }

        if (salary.EffectiveTo.HasValue && salary.EffectiveTo.Value < salary.EffectiveFrom)
        {
            return ServiceResult<bool>.Failure("Ngày kết thúc hiệu lực không thể trước ngày bắt đầu.", ProtocolReason.VALIDATION_FAILED);
        }

        return ServiceResult<bool>.Success(true);
    }
}

