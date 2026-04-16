using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Repairs;

using AutoX.Gara.Frontend.Results.Billings;

using Nalix.Common.Networking.Protocols;

using AutoX.Gara.Shared.Protocol.Repairs;

using Microsoft.Extensions.Logging;

using Nalix.Common.Networking.Protocols;

using Nalix.Framework.Injection;

using Nalix.Framework.Random;

using Nalix.SDK.Transport;

using Nalix.SDK.Transport.Extensions;

using Nalix.Framework.DataFrames.SignalFrames;

namespace AutoX.Gara.Frontend.Services.Invoices;

public sealed class RepairTaskService

{
    private const int RequestTimeoutMs = 10_000;

    private readonly RepairTaskQueryCache _cache;

    public RepairTaskService(RepairTaskQueryCache cache)

        => _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));

    public async System.Threading.Tasks.Task<RepairTaskListResult> GetListAsync(

        int page,

        int pageSize,

        int filterRepairOrderId,

        string? searchTerm = null,

        RepairTaskSortField sortBy = RepairTaskSortField.Id,

        bool sortDescending = true,

        int filterEmployeeId = 0,

        int filterServiceItemId = 0,

        RepairOrderStatus? filterStatus = null,

        System.Threading.CancellationToken ct = default)

    {
        RepairTaskCacheKey key = new(page, pageSize, searchTerm ?? "", sortBy, sortDescending, filterRepairOrderId, filterEmployeeId, filterServiceItemId, filterStatus);

        if (_cache.TryGet(key, out RepairTaskCacheEntry? cached))

        {
            return RepairTaskListResult.Success(cached.RepairTasks, cached.TotalCount, page * pageSize < cached.TotalCount);

        }

        try

        {
            TcpSession client = InstanceManager.Instance.GetExistingInstance<TcpSession>()!;

            RepairTaskQueryRequest packet = new() { Page = page, PageSize = pageSize, SearchTerm = searchTerm ?? "", SortBy = sortBy, SortDescending = sortDescending, FilterRepairOrderId = filterRepairOrderId, FilterEmployeeId = filterEmployeeId, FilterServiceItemId = filterServiceItemId, FilterStatus = filterStatus, OpCode = (System.UInt16)OpCommand.REPAIR_TASK_GET };

            Nalix.Common.Networking.Packets.IPacket r = await client.RequestAsync<Nalix.Common.Networking.Packets.IPacket>(packet, options: Nalix.SDK.Options.RequestOptions.Default.WithTimeout(RequestTimeoutMs).WithEncrypt(), predicate: p => p is RepairTaskQueryResponse or Directive, ct: ct).ConfigureAwait(false);

            if (r is RepairTaskQueryResponse resp)

            {
                _cache.Set(key, resp.RepairTasks, resp.TotalCount);

                return RepairTaskListResult.Success(resp.RepairTasks, resp.TotalCount, page * pageSize < resp.TotalCount);

            }

            if (r is Directive err) return RepairTaskListResult.Failure(err.Reason.ToString(), err.Action);

            return RepairTaskListResult.Failure("Unknown response", ProtocolAdvice.NONE);

        }

        catch (Exception ex) { return RepairTaskListResult.Failure(ex.Message, ProtocolAdvice.NONE); }

    }

    public async System.Threading.Tasks.Task<RepairTaskWriteResult> CreateAsync(RepairTaskDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.REPAIR_TASK_CREATE, data, true, ct);

    public async System.Threading.Tasks.Task<RepairTaskWriteResult> UpdateAsync(RepairTaskDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.REPAIR_TASK_UPDATE, data, true, ct);

    public async System.Threading.Tasks.Task<RepairTaskWriteResult> DeleteAsync(RepairTaskDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.REPAIR_TASK_DELETE, data, false, ct);

    private async System.Threading.Tasks.Task<RepairTaskWriteResult> SendWriteAsync(System.UInt16 op, RepairTaskDto data, bool echo, System.Threading.CancellationToken ct)

    {
        try

        {
            data.OpCode = op;

            TcpSession client = InstanceManager.Instance.GetExistingInstance<TcpSession>()!;

            Nalix.Common.Networking.Packets.IPacket r = await client.RequestAsync<Nalix.Common.Networking.Packets.IPacket>(data, options: Nalix.SDK.Options.RequestOptions.Default.WithTimeout(RequestTimeoutMs).WithEncrypt(), predicate: p => (echo && p is RepairTaskDto) || p is Directive, ct: ct).ConfigureAwait(false);

            if (echo && r is RepairTaskDto confirmed) return RepairTaskWriteResult.Success(confirmed);

            if (r is Directive resp)

            {
                if (resp.Type == ControlType.NONE) { _cache.Invalidate(); return RepairTaskWriteResult.Success(); }

                return RepairTaskWriteResult.Failure(resp.Reason.ToString(), resp.Action);

            }

            return RepairTaskWriteResult.Failure("Unknown", ProtocolAdvice.NONE);

        }

        catch (Exception ex) { return RepairTaskWriteResult.Failure(ex.Message, ProtocolAdvice.NONE); }

    }
}