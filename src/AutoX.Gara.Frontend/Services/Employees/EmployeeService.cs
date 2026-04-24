// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Frontend.Models.Results.Employees;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Employees;
using Nalix.Common.Networking.Protocols;
using Nalix.Framework.DataFrames.SignalFrames;
using Nalix.Framework.Injection;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using System;
namespace AutoX.Gara.Frontend.Services.Employees;
/// <summary>
/// Frontend service for employee operations.
/// </summary>
public sealed class EmployeeService : IEmployeeService
{
    private const int RequestTimeoutMs = 10_000;
    private readonly IEmployeeQueryCache _cache;
    public EmployeeService(IEmployeeQueryCache cache)
        => _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));
    public async System.Threading.Tasks.Task<EmployeeListResult> GetListAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        EmployeeSortField sortBy = EmployeeSortField.Name,
        bool sortDescending = false,
        Position filterPosition = Position.None,
        EmploymentStatus filterStatus = EmploymentStatus.None,
        Gender filterGender = Gender.None,
        System.Threading.CancellationToken ct = default)
    {
        EmployeeCacheKey key = new(page, pageSize, searchTerm ?? "", sortBy, sortDescending, filterPosition, filterStatus, filterGender);
        if (_cache.TryGet(key, out EmployeeCacheEntry? cached))
        {
            return EmployeeListResult.Success(cached!.Employees, cached!.TotalCount, page * pageSize < cached!.TotalCount);
        }
        try
        {
            TcpSession client = InstanceManager.Instance.GetExistingInstance<TcpSession>()!;
            EmployeeQueryRequest packet = new() { Page = page, PageSize = pageSize, SearchTerm = searchTerm ?? "", SortBy = sortBy, SortDescending = sortDescending, FilterPosition = filterPosition, FilterStatus = filterStatus, FilterGender = filterGender, OpCode = (System.UInt16)OpCommand.EMPLOYEE_GET };
            Nalix.Common.Networking.Packets.IPacket r = await client.RequestAsync<Nalix.Common.Networking.Packets.IPacket>(packet, options: Nalix.SDK.Options.RequestOptions.Default.WithTimeout(RequestTimeoutMs).WithEncrypt(), predicate: p => p is EmployeeQueryResponse or Directive, ct: ct).ConfigureAwait(false);
            if (r is EmployeeQueryResponse resp)
            {
                _cache.Set(key, resp.Employees, resp.TotalCount);
                return EmployeeListResult.Success(resp.Employees, resp.TotalCount, page * pageSize < resp.TotalCount);
            }
            return r is Directive err
                ? EmployeeListResult.Failure(err.Reason.ToString(), err.Action)
                : EmployeeListResult.Failure("Unknown response", ProtocolAdvice.NONE);
        }
        catch (System.TimeoutException) { return EmployeeListResult.Timeout(); }
        catch (Exception ex) { return EmployeeListResult.Failure(ex.Message, ProtocolAdvice.NONE); }
    }
    public async System.Threading.Tasks.Task<EmployeeWriteResult> CreateAsync(EmployeeDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.EMPLOYEE_CREATE, data, true, ct);
    public async System.Threading.Tasks.Task<EmployeeWriteResult> UpdateAsync(EmployeeDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.EMPLOYEE_UPDATE, data, true, ct);
    public async System.Threading.Tasks.Task<EmployeeWriteResult> ChangeStatusAsync(EmployeeDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.EMPLOYEE_CHANGE_STATUS, data, false, ct);
    private async System.Threading.Tasks.Task<EmployeeWriteResult> SendWriteAsync(System.UInt16 op, EmployeeDto data, bool echo, System.Threading.CancellationToken ct)
    {
        try
        {
            data.OpCode = op;
            TcpSession client = InstanceManager.Instance.GetExistingInstance<TcpSession>()!;
            Nalix.Common.Networking.Packets.IPacket r = await client.RequestAsync<Nalix.Common.Networking.Packets.IPacket>(data, options: Nalix.SDK.Options.RequestOptions.Default.WithTimeout(RequestTimeoutMs).WithEncrypt(), predicate: p => (echo && p is EmployeeDto) || p is Directive, ct: ct).ConfigureAwait(false);
            if (echo && r is EmployeeDto confirmed)
            {
                return EmployeeWriteResult.Success(confirmed);
            }
            if (r is Directive resp)
            {
                if (resp.Type == ControlType.NONE) { _cache.Invalidate(); return EmployeeWriteResult.Success(); }
                return EmployeeWriteResult.Failure(resp.Reason.ToString(), resp.Action);
            }
            return EmployeeWriteResult.Failure("Unknown", ProtocolAdvice.NONE);
        }
        catch (Exception ex) { return EmployeeWriteResult.Failure(ex.Message, ProtocolAdvice.NONE); }
    }
}
public interface IEmployeeService
{
    System.Threading.Tasks.Task<EmployeeListResult> GetListAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        EmployeeSortField sortBy = EmployeeSortField.Name,
        bool sortDescending = false,
        Position filterPosition = Position.None,
        EmploymentStatus filterStatus = EmploymentStatus.None,
        Gender filterGender = Gender.None,
        System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<EmployeeWriteResult> CreateAsync(EmployeeDto data, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<EmployeeWriteResult> UpdateAsync(EmployeeDto data, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<EmployeeWriteResult> ChangeStatusAsync(EmployeeDto data, System.Threading.CancellationToken ct = default);
}
