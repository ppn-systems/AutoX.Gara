// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Employees;
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

public sealed class EmployeeSalaryService
{
    private const System.Int32 QueryTimeoutMs = 10_000;
    private const System.Int32 WriteTimeoutMs = 20_000;

    private readonly IEmployeeSalaryQueryCache _cache;

    public EmployeeSalaryService(IEmployeeSalaryQueryCache cache)
        => _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));

    public async System.Threading.Tasks.Task<EmployeeSalaryListResult> GetListAsync(
        System.Int32 page,
        System.Int32 pageSize,
        System.Int32 filterEmployeeId,
        System.String? searchTerm = null,
        EmployeeSalarySortField sortBy = EmployeeSalarySortField.EffectiveFrom,
        System.Boolean sortDescending = true,
        SalaryType? filterSalaryType = null,
        System.DateTime? filterFromDate = null,
        System.DateTime? filterToDate = null,
        System.Threading.CancellationToken ct = default)
    {
        EmployeeSalaryCacheKey key = new(
            page, pageSize,
            searchTerm ?? System.String.Empty,
            sortBy, sortDescending,
            filterEmployeeId,
            filterSalaryType,
            filterFromDate, filterToDate);

        if (_cache.TryGet(key, out EmployeeSalaryCacheEntry? cached))
        {
            System.Boolean hasMore = page * pageSize < cached!.TotalCount;
            return EmployeeSalaryListResult.Success(cached.Salaries, cached.TotalCount, hasMore);
        }

        try
        {
            System.UInt32 sq = Csprng.NextUInt32();
            ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

            EmployeeSalaryQueryRequest packet = new()
            {
                SequenceId = sq,
                Page = page,
                PageSize = pageSize,
                SearchTerm = searchTerm ?? System.String.Empty,
                SortBy = sortBy,
                SortDescending = sortDescending,
                FilterEmployeeId = filterEmployeeId,
                FilterSalaryType = filterSalaryType,
                FilterFromDate = filterFromDate,
                FilterToDate = filterToDate,
                OpCode = (System.UInt16)OpCommand.EMPLOYEE_SALARY_GET
            };

            System.Threading.Tasks.TaskCompletionSource<EmployeeSalaryListResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? sub = null;
            System.IDisposable? errSub = null;

            sub = client.OnOnce<EmployeeSalaryQueryResponse>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();
                    errSub?.Dispose();
                    _cache.Set(key, resp.Salaries, resp.TotalCount);
                    System.Boolean hasMore = page * pageSize < resp.TotalCount;
                    tcs.TrySetResult(EmployeeSalaryListResult.Success(resp.Salaries, resp.TotalCount, hasMore));
                });

            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();
                    errSub?.Dispose();
                    tcs.TrySetResult(EmployeeSalaryListResult.Failure(MapErrorReason(resp.Reason), resp.Action));
                });

            await client.SendAsync(packet, ct).ConfigureAwait(false);

            using System.Threading.CancellationTokenSource cts =
                System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct);
            System.Threading.Tasks.Task timeoutTask =
                System.Threading.Tasks.Task.Delay(QueryTimeoutMs, cts.Token);
            System.Threading.Tasks.Task winner =
                await System.Threading.Tasks.Task.WhenAny(tcs.Task, timeoutTask).ConfigureAwait(false);
            cts.Cancel();

            if (!ReferenceEquals(winner, tcs.Task))
            {
                sub?.Dispose();
                errSub?.Dispose();
                return EmployeeSalaryListResult.Timeout();
            }

            return await tcs.Task.ConfigureAwait(false);
        }
        catch (System.OperationCanceledException)
        {
            return EmployeeSalaryListResult.Failure("Yêu cầu bị hủy.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return EmployeeSalaryListResult.Failure($"Lỗi không xác định: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    public System.Threading.Tasks.Task<EmployeeSalaryWriteResult> CreateAsync(EmployeeSalaryDto data, System.Threading.CancellationToken ct = default)
        => SendWritePacketAsync(data, (System.UInt16)OpCommand.EMPLOYEE_SALARY_CREATE, expectEcho: true, ct);

    public System.Threading.Tasks.Task<EmployeeSalaryWriteResult> UpdateAsync(EmployeeSalaryDto data, System.Threading.CancellationToken ct = default)
        => SendWritePacketAsync(data, (System.UInt16)OpCommand.EMPLOYEE_SALARY_UPDATE, expectEcho: true, ct);

    public System.Threading.Tasks.Task<EmployeeSalaryWriteResult> DeleteAsync(EmployeeSalaryDto data, System.Threading.CancellationToken ct = default)
        => SendWritePacketAsync(data, (System.UInt16)OpCommand.EMPLOYEE_SALARY_DELETE, expectEcho: false, ct);

    private async System.Threading.Tasks.Task<EmployeeSalaryWriteResult> SendWritePacketAsync(
        EmployeeSalaryDto data,
        System.UInt16 opcode,
        System.Boolean expectEcho,
        System.Threading.CancellationToken ct)
    {
        try
        {
            data.OpCode = opcode;
            data.SequenceId = data.SequenceId == 0 ? Csprng.NextUInt32() : data.SequenceId;
            System.UInt32 sq = data.SequenceId;

            ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

            System.Threading.Tasks.TaskCompletionSource<EmployeeSalaryWriteResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? echoSub = null;
            System.IDisposable? errSub = null;

            if (expectEcho)
            {
                echoSub = client.OnOnce<EmployeeSalaryDto>(
                    predicate: p => p.SequenceId == sq,
                    handler: confirmed =>
                    {
                        echoSub?.Dispose();
                        errSub?.Dispose();
                        tcs.TrySetResult(EmployeeSalaryWriteResult.Success(confirmed));
                    });
            }

            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    echoSub?.Dispose();
                    errSub?.Dispose();
                    EmployeeSalaryWriteResult result = resp.Type == ControlType.NONE
                        ? EmployeeSalaryWriteResult.Success()
                        : EmployeeSalaryWriteResult.Failure(MapErrorReason(resp.Reason), resp.Action);
                    tcs.TrySetResult(result);
                });

            await client.SendAsync(data, ct).ConfigureAwait(false);

            using System.Threading.CancellationTokenSource cts =
                System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct);
            System.Threading.Tasks.Task timeoutTask =
                System.Threading.Tasks.Task.Delay(WriteTimeoutMs, cts.Token);
            System.Threading.Tasks.Task winner =
                await System.Threading.Tasks.Task.WhenAny(tcs.Task, timeoutTask).ConfigureAwait(false);
            cts.Cancel();

            if (!ReferenceEquals(winner, tcs.Task))
            {
                echoSub?.Dispose();
                errSub?.Dispose();
                return EmployeeSalaryWriteResult.Timeout();
            }

            EmployeeSalaryWriteResult final = await tcs.Task.ConfigureAwait(false);
            if (final.IsSuccess)
            {
                _cache.Invalidate();
            }

            return final;
        }
        catch (System.OperationCanceledException)
        {
            return EmployeeSalaryWriteResult.Failure("Yêu cầu bị hủy.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return EmployeeSalaryWriteResult.Failure($"Lỗi không xác định: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    private static System.String MapErrorReason(ProtocolReason reason)
        => reason switch
        {
            ProtocolReason.NOT_FOUND => "Không tìm thấy dữ liệu lương.",
            ProtocolReason.ALREADY_EXISTS => "Dữ liệu đã tồn tại.",
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
        ILogger? logger = InstanceManager.Instance.GetExistingInstance<ILogger>();
        logger?.Error(ex.ToString());
        if (ex.InnerException is not null)
        {
            logger?.Error("Inner: " + ex.InnerException);
        }
    }
}

