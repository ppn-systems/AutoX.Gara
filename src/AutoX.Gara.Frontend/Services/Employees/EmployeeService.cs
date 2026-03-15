// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Frontend.Models.Results.Employees;
using AutoX.Gara.Frontend.Results.Employees;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Employees;
using Nalix.Common.Diagnostics.Abstractions;
using Nalix.Common.Networking.Protocols;
using Nalix.Framework.Injection;
using Nalix.Framework.Random;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using Nalix.Shared.Frames.Controls;

namespace AutoX.Gara.Frontend.Services.Employees;

/// <summary>
/// Frontend service for employee operations.
/// </summary>
public sealed class EmployeeService : IEmployeeService
{
    private const System.Int32 RequestTimeoutMs = 10_000;
    private readonly IEmployeeQueryCache _cache;

    public EmployeeService(IEmployeeQueryCache cache)
        => _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));

    public async System.Threading.Tasks.Task<EmployeeListResult> GetListAsync(
        System.Int32 page,
        System.Int32 pageSize,
        System.String? searchTerm = null,
        EmployeeSortField sortBy = EmployeeSortField.Name,
        System.Boolean sortDescending = false,
        Position filterPosition = Position.None,
        EmploymentStatus filterStatus = EmploymentStatus.None,
        Gender filterGender = Gender.None,
        System.Threading.CancellationToken ct = default)
    {
        EmployeeCacheKey key = new(
            page, pageSize,
            searchTerm ?? System.String.Empty,
            sortBy, sortDescending,
            filterPosition, filterStatus, filterGender);

        if (_cache.TryGet(key, out EmployeeCacheEntry? cached))
        {
            System.Boolean hasMore = page * pageSize < cached!.TotalCount;
            return EmployeeListResult.Success(cached.Employees, cached.TotalCount, hasMore);
        }

        try
        {
            System.UInt32 sq = Csprng.NextUInt32();
            ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

            EmployeeQueryRequest packet = new()
            {
                Page = page,
                PageSize = pageSize,
                SequenceId = sq,
                SearchTerm = searchTerm ?? System.String.Empty,
                SortBy = sortBy,
                SortDescending = sortDescending,
                FilterPosition = filterPosition,
                FilterStatus = filterStatus,
                FilterGender = filterGender,
                OpCode = (System.UInt16)OpCommand.EMPLOYEE_GET
            };

            System.Threading.Tasks.TaskCompletionSource<EmployeeListResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? sub = null;
            System.IDisposable? errSub = null;

            sub = client.OnOnce<EmployeeQueryResponse>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();
                    errSub?.Dispose();
                    _cache.Set(key, resp.Employees, resp.TotalCount);
                    System.Boolean hasMore = page * pageSize < resp.TotalCount;
                    tcs.TrySetResult(EmployeeListResult.Success(resp.Employees, resp.TotalCount, hasMore));
                });

            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();
                    errSub?.Dispose();
                    if (resp.Type == ControlType.NONE && resp.Reason == ProtocolReason.NONE)
                    {
                        return;
                    }

                    tcs.TrySetResult(EmployeeListResult.Failure(MapErrorReason(resp.Reason), resp.Action));
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
                return EmployeeListResult.Timeout();
            }

            return await tcs.Task.ConfigureAwait(false);
        }
        catch (System.OperationCanceledException)
        {
            return EmployeeListResult.Failure("Yêu cầu bị hủy.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return EmployeeListResult.Failure(
                $"Lỗi không xác định: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    public async System.Threading.Tasks.Task<EmployeeWriteResult> CreateAsync(
        EmployeeDto data,
        System.Threading.CancellationToken ct = default)
    {
        EmployeeWriteResult result = await SendWritePacketAsync(
            (System.UInt16)OpCommand.EMPLOYEE_CREATE, data, expectEcho: true, ct).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            _cache.Invalidate();
        }

        return result;
    }

    public async System.Threading.Tasks.Task<EmployeeWriteResult> UpdateAsync(
        EmployeeDto data,
        System.Threading.CancellationToken ct = default)
    {
        EmployeeWriteResult result = await SendWritePacketAsync(
            (System.UInt16)OpCommand.EMPLOYEE_UPDATE, data, expectEcho: true, ct).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            _cache.Invalidate();
        }

        return result;
    }

    public async System.Threading.Tasks.Task<EmployeeWriteResult> ChangeStatusAsync(
        EmployeeDto data,
        System.Threading.CancellationToken ct = default)
    {
        EmployeeWriteResult result = await SendWritePacketAsync(
            (System.UInt16)OpCommand.EMPLOYEE_CHANGE_STATUS, data, expectEcho: false, ct).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            _cache.Invalidate();
        }

        return result;
    }

    private static async System.Threading.Tasks.Task<EmployeeWriteResult> SendWritePacketAsync(
        System.UInt16 opcode,
        EmployeeDto data,
        System.Boolean expectEcho,
        System.Threading.CancellationToken ct)
    {
        try
        {
            System.UInt32 sq = Csprng.NextUInt32();
            ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

            data.OpCode = opcode;
            data.SequenceId = sq;

            System.Threading.Tasks.TaskCompletionSource<EmployeeWriteResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? echoSub = null;
            System.IDisposable? errSub = null;

            if (expectEcho)
            {
                echoSub = client.OnOnce<EmployeeDto>(
                    predicate: p => p.SequenceId == sq,
                    handler: confirmed =>
                    {
                        echoSub?.Dispose();
                        errSub?.Dispose();
                        tcs.TrySetResult(EmployeeWriteResult.Success(confirmed));
                    });
            }

            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    echoSub?.Dispose();
                    errSub?.Dispose();
                    EmployeeWriteResult result = resp.Type == ControlType.NONE
                        ? EmployeeWriteResult.Success()
                        : EmployeeWriteResult.Failure(MapErrorReason(resp.Reason), resp.Action);
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
                return EmployeeWriteResult.Timeout();
            }

            return await tcs.Task.ConfigureAwait(false);
        }
        catch (System.OperationCanceledException)
        {
            return EmployeeWriteResult.Failure("Yêu cầu bị hủy.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return EmployeeWriteResult.Failure(
                $"Lỗi không xác định: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    private static System.String MapErrorReason(ProtocolReason reason)
        => reason switch
        {
            ProtocolReason.NOT_FOUND => "Không tìm thấy nhân viên.",
            ProtocolReason.ALREADY_EXISTS => "Email hoặc số điện thoại đã tồn tại.",
            ProtocolReason.MALFORMED_PACKET => "Dữ liệu không hợp lệ.",
            ProtocolReason.INTERNAL_ERROR => "Lỗi hệ thống, vui lòng thử lại sau.",
            ProtocolReason.FORBIDDEN => "Bạn không có quyền thực hiện thao tác này.",
            ProtocolReason.UNAUTHENTICATED => "Bạn không có quyền thực hiện thao tác này.",
            ProtocolReason.RATE_LIMITED => "Bạn đang thao tác quá nhanh, vui lòng chờ một chút rồi thử lại.",
            ProtocolReason.TIMEOUT => "Máy chủ phản hồi hết hạn, vui lòng thử lại.",
            _ => "Thao tác thất bại, vui lòng thử lại."
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

/// <summary>
/// Abstraction for employee service.
/// </summary>
public interface IEmployeeService
{
    System.Threading.Tasks.Task<EmployeeListResult> GetListAsync(
        System.Int32 page,
        System.Int32 pageSize,
        System.String? searchTerm = null,
        EmployeeSortField sortBy = EmployeeSortField.Name,
        System.Boolean sortDescending = false,
        Position filterPosition = Position.None,
        EmploymentStatus filterStatus = EmploymentStatus.None,
        Gender filterGender = Gender.None,
        System.Threading.CancellationToken ct = default);

    System.Threading.Tasks.Task<EmployeeWriteResult> CreateAsync(EmployeeDto data, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<EmployeeWriteResult> UpdateAsync(EmployeeDto data, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<EmployeeWriteResult> ChangeStatusAsync(EmployeeDto data, System.Threading.CancellationToken ct = default);
}
