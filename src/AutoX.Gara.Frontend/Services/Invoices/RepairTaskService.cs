// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Repairs;
using AutoX.Gara.Frontend.Results.Billings;
using AutoX.Gara.Shared.Enums;
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
    private const System.Int32 RequestTimeoutMs = 10_000;
    private readonly RepairTaskQueryCache _cache;

    public RepairTaskService(RepairTaskQueryCache cache)
        => _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));

    public async System.Threading.Tasks.Task<RepairTaskListResult> GetListAsync(
        System.Int32 page,
        System.Int32 pageSize,
        System.Int32 filterRepairOrderId,
        System.String? searchTerm = null,
        RepairTaskSortField sortBy = RepairTaskSortField.Id,
        System.Boolean sortDescending = true,
        System.Int32 filterEmployeeId = 0,
        System.Int32 filterServiceItemId = 0,
        RepairOrderStatus? filterStatus = null,
        System.Threading.CancellationToken ct = default)
    {
        RepairTaskCacheKey key = new(
            page, pageSize,
            searchTerm ?? System.String.Empty,
            sortBy, sortDescending,
            filterRepairOrderId, filterEmployeeId, filterServiceItemId,
            filterStatus);

        if (_cache.TryGet(key, out RepairTaskCacheEntry? cached))
        {
            System.Boolean hasMore = page * pageSize < cached!.TotalCount;
            return RepairTaskListResult.Success(cached.RepairTasks, cached.TotalCount, hasMore);
        }

        try
        {
            System.UInt32 sq = Csprng.NextUInt32();
            TcpSession client = InstanceManager.Instance.GetOrCreateInstance<TcpSession>();

            RepairTaskQueryRequest packet = new()
            {
                SequenceId = sq,
                Page = page,
                PageSize = pageSize,
                SearchTerm = searchTerm ?? System.String.Empty,
                SortBy = sortBy,
                SortDescending = sortDescending,
                FilterRepairOrderId = filterRepairOrderId,
                FilterEmployeeId = filterEmployeeId,
                FilterServiceItemId = filterServiceItemId,
                FilterStatus = filterStatus,
                OpCode = (System.UInt16)OpCommand.REPAIR_TASK_GET
            };

            System.Threading.Tasks.TaskCompletionSource<RepairTaskListResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? sub = null;
            System.IDisposable? errSub = null;

            sub = client.OnOnce<RepairTaskQueryResponse>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();
                    errSub?.Dispose();
                    _cache.Set(key, resp.RepairTasks, resp.TotalCount);
                    System.Boolean hasMore = page * pageSize < resp.TotalCount;
                    tcs.TrySetResult(RepairTaskListResult.Success(resp.RepairTasks, resp.TotalCount, hasMore));
                });

            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();
                    errSub?.Dispose();
                    tcs.TrySetResult(RepairTaskListResult.Failure(MapErrorReason(resp.Reason), resp.Action));
                });

            await client.SendAsync(packet, ct).ConfigureAwait(false);

            using System.Threading.CancellationTokenSource cts =
                System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct);

            System.Threading.Tasks.Task timeoutTask =
                System.Threading.Tasks.Task.Delay(RequestTimeoutMs, cts.Token);

            System.Threading.Tasks.Task winner =
                await System.Threading.Tasks.Task.WhenAny(tcs.Task, timeoutTask).ConfigureAwait(false);

            cts.Cancel();

            if (!ReferenceEquals(winner, tcs.Task))
            {
                sub?.Dispose();
                errSub?.Dispose();
                return RepairTaskListResult.Timeout();
            }

            return await tcs.Task.ConfigureAwait(false);
        }
        catch (System.OperationCanceledException)
        {
            return RepairTaskListResult.Failure("Yêu cầu bị hủy.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return RepairTaskListResult.Failure($"Lỗi không xác định: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    public async System.Threading.Tasks.Task<RepairTaskWriteResult> CreateAsync(RepairTaskDto data, System.Threading.CancellationToken ct = default)
        => await SendWritePacketAsync((System.UInt16)OpCommand.REPAIR_TASK_CREATE, data, expectEcho: true, ct).ConfigureAwait(false);

    public async System.Threading.Tasks.Task<RepairTaskWriteResult> UpdateAsync(RepairTaskDto data, System.Threading.CancellationToken ct = default)
        => await SendWritePacketAsync((System.UInt16)OpCommand.REPAIR_TASK_UPDATE, data, expectEcho: true, ct).ConfigureAwait(false);

    public async System.Threading.Tasks.Task<RepairTaskWriteResult> DeleteAsync(RepairTaskDto data, System.Threading.CancellationToken ct = default)
        => await SendWritePacketAsync((System.UInt16)OpCommand.REPAIR_TASK_DELETE, data, expectEcho: false, ct).ConfigureAwait(false);

    private async System.Threading.Tasks.Task<RepairTaskWriteResult> SendWritePacketAsync(
        System.UInt16 opcode,
        RepairTaskDto data,
        System.Boolean expectEcho,
        System.Threading.CancellationToken ct)
    {
        try
        {
            System.UInt32 sq = Csprng.NextUInt32();
            TcpSession client = InstanceManager.Instance.GetOrCreateInstance<TcpSession>();

            data.OpCode = opcode;
            data.SequenceId = sq;

            System.Threading.Tasks.TaskCompletionSource<RepairTaskWriteResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? echoSub = null;
            System.IDisposable? errSub = null;

            if (expectEcho)
            {
                echoSub = client.OnOnce<RepairTaskDto>(
                    predicate: p => p.SequenceId == sq,
                    handler: confirmed =>
                    {
                        echoSub?.Dispose();
                        errSub?.Dispose();
                        tcs.TrySetResult(RepairTaskWriteResult.Success(confirmed));
                    });
            }

            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    echoSub?.Dispose();
                    errSub?.Dispose();
                    RepairTaskWriteResult result = resp.Type == ControlType.NONE
                        ? RepairTaskWriteResult.Success()
                        : RepairTaskWriteResult.Failure(MapErrorReason(resp.Reason), resp.Action);
                    tcs.TrySetResult(result);
                });

            await client.SendAsync(data, ct).ConfigureAwait(false);

            using System.Threading.CancellationTokenSource cts =
                System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct);

            System.Threading.Tasks.Task timeoutTask =
                System.Threading.Tasks.Task.Delay(RequestTimeoutMs, cts.Token);

            System.Threading.Tasks.Task winner =
                await System.Threading.Tasks.Task.WhenAny(tcs.Task, timeoutTask).ConfigureAwait(false);

            cts.Cancel();

            if (!ReferenceEquals(winner, tcs.Task))
            {
                echoSub?.Dispose();
                errSub?.Dispose();
                return RepairTaskWriteResult.Timeout();
            }

            RepairTaskWriteResult final = await tcs.Task.ConfigureAwait(false);
            if (final.IsSuccess)
            {
                _cache.Invalidate();
            }
            return final;
        }
        catch (System.OperationCanceledException)
        {
            return RepairTaskWriteResult.Failure("Yêu cầu bị hủy.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return RepairTaskWriteResult.Failure($"Lỗi không xác định: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    private static System.String MapErrorReason(ProtocolReason reason)
        => reason switch
        {
            ProtocolReason.NOT_FOUND => "Không tìm thấy task.",
            ProtocolReason.MALFORMED_PACKET => "Dữ liệu không hợp lệ.",
            ProtocolReason.VALIDATION_FAILED => "Dữ liệu không hợp lệ.",
            ProtocolReason.INTERNAL_ERROR => "Lỗi hệ thống. Vui lòng thử lại sau.",
            ProtocolReason.FORBIDDEN => "Bạn không có quyền thực hiện thao tác này.",
            ProtocolReason.UNAUTHENTICATED => "Bạn không có quyền thực hiện thao tác này.",
            ProtocolReason.RATE_LIMITED => "Bạn đang thao tác quá nhanh. Vui lòng chờ một chút.",
            ProtocolReason.TIMEOUT => "Máy chủ phản hồi hết hạn. Vui lòng thử lại.",
            _ => "Thao tác thất bại. Vui lòng thử lại."
        };

    private static void LogException(System.Exception ex)
    {
        ILogger logger = InstanceManager.Instance.GetOrCreateInstance<ILogger>();
        logger.Error(ex.ToString());
        if (ex.InnerException is not null)
        {
            logger.Error("Inner: " + ex.InnerException);
        }
    }
}

