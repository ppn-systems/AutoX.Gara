// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Frontend.Models.Results.Employees;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Protocol.Employees;
using Nalix.Common.Networking.Protocols;
using Nalix.Framework.DataFrames.SignalFrames;
using Nalix.Framework.Injection;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using System;
namespace AutoX.Gara.Frontend.Services.Employees;
public sealed class EmployeeSalaryService
{
    private const int QueryTimeoutMs = 10_000;
    private const int WriteTimeoutMs = 20_000;
    private readonly IEmployeeSalaryQueryCache _cache;
    public EmployeeSalaryService(IEmployeeSalaryQueryCache cache) => _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));
    public async System.Threading.Tasks.Task<EmployeeSalaryListResult> GetListAsync(
        int page,
        int pageSize,
        int filterEmployeeId,
        string? searchTerm = null,
        EmployeeSalarySortField sortBy = EmployeeSalarySortField.EffectiveFrom,
        bool sortDescending = true,
        SalaryType? filterSalaryType = null,
        DateTime? filterFromDate = null,
        DateTime? filterToDate = null,
        System.Threading.CancellationToken ct = default)
    {
        EmployeeSalaryCacheKey key = new(page, pageSize, searchTerm ?? "", sortBy, sortDescending, filterEmployeeId, filterSalaryType, filterFromDate, filterToDate);
        if (_cache.TryGet(key, out EmployeeSalaryCacheEntry? cached))
        {
            return EmployeeSalaryListResult.Success(cached!.Salaries, cached!.TotalCount, page * pageSize < cached!.TotalCount);
        }
        try
        {
            TcpSession client = InstanceManager.Instance.GetExistingInstance<TcpSession>()!;
            EmployeeSalaryQueryRequest packet = new() { Page = page, PageSize = pageSize, SearchTerm = searchTerm ?? "", SortBy = sortBy, SortDescending = sortDescending, FilterEmployeeId = filterEmployeeId, FilterSalaryType = filterSalaryType, FilterFromDate = filterFromDate, FilterToDate = filterToDate, OpCode = (System.UInt16)OpCommand.EMPLOYEE_SALARY_GET };
            Nalix.Common.Networking.Packets.IPacket r = await client.RequestAsync<Nalix.Common.Networking.Packets.IPacket>(packet, options: Nalix.SDK.Options.RequestOptions.Default.WithTimeout(QueryTimeoutMs).WithEncrypt(), predicate: p => p is EmployeeSalaryQueryResponse or Directive, ct: ct).ConfigureAwait(false);
            if (r is EmployeeSalaryQueryResponse resp)
            {
                _cache.Set(key, resp.Salaries, resp.TotalCount);
                return EmployeeSalaryListResult.Success(resp.Salaries, resp.TotalCount, page * pageSize < resp.TotalCount);
            }
            return r is Directive err
                ? EmployeeSalaryListResult.Failure(err.Reason.ToString(), err.Action)
                : EmployeeSalaryListResult.Failure("Unknown response", ProtocolAdvice.NONE);
        }
        catch (Exception ex) { return EmployeeSalaryListResult.Failure(ex.Message, ProtocolAdvice.NONE); }
    }
    public async System.Threading.Tasks.Task<EmployeeSalaryWriteResult> CreateAsync(EmployeeSalaryDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.EMPLOYEE_SALARY_CREATE, data, true, ct);
    public async System.Threading.Tasks.Task<EmployeeSalaryWriteResult> UpdateAsync(EmployeeSalaryDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.EMPLOYEE_SALARY_UPDATE, data, true, ct);
    private async System.Threading.Tasks.Task<EmployeeSalaryWriteResult> SendWriteAsync(System.UInt16 op, EmployeeSalaryDto data, bool echo, System.Threading.CancellationToken ct)
    {
        try
        {
            data.OpCode = op;
            TcpSession client = InstanceManager.Instance.GetExistingInstance<TcpSession>()!;
            Nalix.Common.Networking.Packets.IPacket r = await client.RequestAsync<Nalix.Common.Networking.Packets.IPacket>(data, options: Nalix.SDK.Options.RequestOptions.Default.WithTimeout(WriteTimeoutMs).WithEncrypt(), predicate: p => (echo && p is EmployeeSalaryDto) || p is Directive, ct: ct).ConfigureAwait(false);
            if (echo && r is EmployeeSalaryDto confirmed)
            {
                return EmployeeSalaryWriteResult.Success(confirmed);
            }
            if (r is Directive resp)
            {
                if (resp.Type == ControlType.NONE) { _cache.Invalidate(); return EmployeeSalaryWriteResult.Success(); }
                return EmployeeSalaryWriteResult.Failure(resp.Reason.ToString(), resp.Action);
            }
            return EmployeeSalaryWriteResult.Failure("Unknown", ProtocolAdvice.NONE);
        }
        catch (Exception ex) { return EmployeeSalaryWriteResult.Failure(ex.Message, ProtocolAdvice.NONE); }
    }
}

