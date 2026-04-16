// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Frontend.Results.ServiceItems;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Billings;
using Microsoft.Extensions.Logging;
using Nalix.Common.Networking.Protocols;
using Nalix.Framework.Injection;
using Nalix.Framework.Random;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using Nalix.Framework.DataFrames.SignalFrames;

namespace AutoX.Gara.Frontend.Services.Billings;

public sealed class ServiceItemService
{
    private const System.Int32 RequestTimeoutMs = 10_000;
    private readonly ServiceItemQueryCache _cache;

    public ServiceItemService(ServiceItemQueryCache cache)
        => _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));

    public async System.Threading.Tasks.Task<ServiceItemListResult> GetListAsync(
        System.Int32 page,
        System.Int32 pageSize,
        System.String? searchTerm = null,
        ServiceItemSortField sortBy = ServiceItemSortField.Description,
        System.Boolean sortDescending = false,
        ServiceType? filterType = null,
        System.Decimal? filterMinUnitPrice = null,
        System.Decimal? filterMaxUnitPrice = null,
        System.Threading.CancellationToken ct = default)
    {
        ServiceItemCacheKey key = new(
            page, pageSize,
            searchTerm ?? System.String.Empty,
            sortBy, sortDescending,
            filterType, filterMinUnitPrice, filterMaxUnitPrice);

        if (_cache.TryGet(key, out ServiceItemCacheEntry? cached))
        {
            System.Boolean hasMore = page * pageSize < cached!.TotalCount;
            return ServiceItemListResult.Success(cached.ServiceItems, cached.TotalCount, hasMore);
        }

        try
        {
            System.UInt32 sq = Csprng.NextUInt32();
            TcpSession client = InstanceManager.Instance.GetOrCreateInstance<TcpSession>();

            ServiceItemQueryRequest packet = new()
            {
                SequenceId = sq,
                Page = page,
                PageSize = pageSize,
                SearchTerm = searchTerm ?? System.String.Empty,
                SortBy = sortBy,
                SortDescending = sortDescending,
                FilterType = filterType,
                FilterMinUnitPrice = filterMinUnitPrice,
                FilterMaxUnitPrice = filterMaxUnitPrice,
                OpCode = (System.UInt16)OpCommand.SERVICE_ITEM_GET
            };

            System.Threading.Tasks.TaskCompletionSource<ServiceItemListResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? sub = null;
            System.IDisposable? errSub = null;

            sub = client.OnOnce<ServiceItemQueryResponse>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();
                    errSub?.Dispose();
                    _cache.Set(key, resp.ServiceItems, resp.TotalCount);
                    System.Boolean hasMore = page * pageSize < resp.TotalCount;
                    tcs.TrySetResult(ServiceItemListResult.Success(resp.ServiceItems, resp.TotalCount, hasMore));
                });

            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();
                    errSub?.Dispose();
                    tcs.TrySetResult(ServiceItemListResult.Failure(MapErrorReason(resp.Reason), resp.Action));
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
                return ServiceItemListResult.Timeout();
            }

            return await tcs.Task.ConfigureAwait(false);
        }
        catch (System.OperationCanceledException)
        {
            return ServiceItemListResult.Failure("Yêu cầu bị hủy.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return ServiceItemListResult.Failure($"Lỗi không xác định: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    private static System.String MapErrorReason(ProtocolReason reason)
        => reason switch
        {
            ProtocolReason.NOT_FOUND => "Không tìm thấy dịch vụ.",
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

    public async System.Threading.Tasks.Task<ServiceItemWriteResult> CreateAsync(
        ServiceItemDto data,
        System.Threading.CancellationToken ct = default)
    {
        ServiceItemWriteResult result = await SendWritePacketAsync(
            (System.UInt16)OpCommand.SERVICE_ITEM_CREATE, data, expectEcho: true, ct).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            _cache.Invalidate();
        }

        return result;
    }

    public async System.Threading.Tasks.Task<ServiceItemWriteResult> UpdateAsync(
        ServiceItemDto data,
        System.Threading.CancellationToken ct = default)
    {
        ServiceItemWriteResult result = await SendWritePacketAsync(
            (System.UInt16)OpCommand.SERVICE_ITEM_UPDATE, data, expectEcho: true, ct).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            _cache.Invalidate();
        }

        return result;
    }

    public async System.Threading.Tasks.Task<ServiceItemWriteResult> DeleteAsync(
        ServiceItemDto data,
        System.Threading.CancellationToken ct = default)
    {
        ServiceItemWriteResult result = await SendWritePacketAsync(
            (System.UInt16)OpCommand.SERVICE_ITEM_DELETE, data, expectEcho: false, ct).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            _cache.Invalidate();
        }

        return result;
    }

    private static async System.Threading.Tasks.Task<ServiceItemWriteResult> SendWritePacketAsync(
        System.UInt16 opcode,
        ServiceItemDto data,
        System.Boolean expectEcho,
        System.Threading.CancellationToken ct)
    {
        try
        {
            System.UInt32 sq = Csprng.NextUInt32();
            TcpSession client = InstanceManager.Instance.GetOrCreateInstance<TcpSession>();

            data.OpCode = opcode;
            data.SequenceId = sq;

            System.Threading.Tasks.TaskCompletionSource<ServiceItemWriteResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? echoSub = null;
            System.IDisposable? errSub = null;

            if (expectEcho)
            {
                echoSub = client.OnOnce<ServiceItemDto>(
                    predicate: p => p.SequenceId == sq,
                    handler: confirmed =>
                    {
                        echoSub?.Dispose();
                        errSub?.Dispose();
                        tcs.TrySetResult(ServiceItemWriteResult.Success(confirmed));
                    });
            }

            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    echoSub?.Dispose();
                    errSub?.Dispose();
                    ServiceItemWriteResult result = resp.Type == ControlType.NONE
                        ? ServiceItemWriteResult.Success()
                        : ServiceItemWriteResult.Failure(MapErrorReason(resp.Reason), resp.Action);
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
                return ServiceItemWriteResult.Timeout();
            }

            return await tcs.Task.ConfigureAwait(false);
        }
        catch (System.OperationCanceledException)
        {
            return ServiceItemWriteResult.Failure("Yêu cầu bị hủy.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return ServiceItemWriteResult.Failure($"Lỗi không xác định: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }
}

