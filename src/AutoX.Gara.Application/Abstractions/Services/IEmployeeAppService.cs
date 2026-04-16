// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Protocol.Employees;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoX.Gara.Application.Abstractions.Services;

public interface IEmployeeAppService
{
    Task<ServiceResult<(List<Employee> items, int totalCount)>> GetPageAsync(EmployeeListQuery query);
    Task<ServiceResult<Employee>> CreateAsync(Employee employee);
    Task<ServiceResult<Employee>> UpdateAsync(Employee employee);
    Task<ServiceResult<bool>> ChangeStatusAsync(int employeeId, AutoX.Gara.Domain.Enums.Employees.EmploymentStatus status);
}

public interface IEmployeeSalaryAppService
{
    Task<ServiceResult<(List<EmployeeSalary> items, int totalCount)>> GetPageAsync(EmployeeSalaryListQuery query);
    Task<ServiceResult<EmployeeSalary>> CreateAsync(EmployeeSalary salary);
    Task<ServiceResult<EmployeeSalary>> UpdateAsync(EmployeeSalary salary);
    Task<ServiceResult<bool>> DeleteAsync(int salaryId);
}