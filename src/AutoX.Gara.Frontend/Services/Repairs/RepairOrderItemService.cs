// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Models.Results.Billings;
using AutoX.Gara.Frontend.Results.Billings;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Billings;
using AutoX.Gara.Shared.Protocol.Repairs;
using Nalix.Common.Diagnostics.Abstractions;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security.Enums;
using Nalix.Framework.Injection;
using Nalix.Framework.Random;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using Nalix.Shared.Frames.Controls;

namespace AutoX.Gara.Frontend.Services.Repairs;

public sealed class RepairOrderItemService
{
    private const System.Int32 RequestTimeoutMs = 10_000;
    private readonly RepairOrderItemQueryCache _cache;

    public RepairOrderItemService(RepairOrderItemQueryCache cache)
        => _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));

    public async System.Threading.Tasks.Task<RepairOrderItemListResult> GetListAsync(
        System.Int32 page,
        System.Int32 pageSize,
        System.Int32 filterRepairOrderId,
        System.String? searchTerm = null,
        RepairOrderItemSortField sortBy = RepairOrderItemSortField.Id,
        System.Boolean sortDescending = true,
        System.Int32 filterPartId = 0,
        System.Threading.CancellationToken ct = default)
    {
        RepairOrderItemCacheKey key = new(
            page, pageSize,
            searchTerm ?? System.String.Empty,
            sortBy, sortDescending,
            filterRepairOrderId, filterPartId);

        if (_cache.TryGet(key, out RepairOrderItemCacheEntry? cached))
        {
            System.Boolean hasMore = page * pageSize < cached!.TotalCount;
            return RepairOrderItemListResult.Success(cached.Items, cached.TotalCount, hasMore);
        }

        try
        {
            System.UInt32 sq = Csprng.NextUInt32();
            ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

            RepairOrderItemQueryRequest packet = new()
            {
                SequenceId = sq,
                Page = page,
                PageSize = pageSize,
                SearchTerm = searchTerm ?? System.String.Empty,
                SortBy = sortBy,
                SortDescending = sortDescending,
                FilterRepairOrderId = filterRepairOrderId,
                FilterPartId = filterPartId,
                OpCode = (System.UInt16)OpCommand.REPAIR_ORDER_ITEM_GET
            };

            System.Threading.Tasks.TaskCompletionSource<RepairOrderItemListResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? sub = null;
            System.IDisposable? errSub = null;

            sub = client.OnOnce<RepairOrderItemQueryResponse>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();
                    errSub?.Dispose();
                    _cache.Set(key, resp.RepairOrderItems, resp.TotalCount);
                    System.Boolean hasMore = page * pageSize < resp.TotalCount;
                    tcs.TrySetResult(RepairOrderItemListResult.Success(resp.RepairOrderItems, resp.TotalCount, hasMore));
                });

            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();
                    errSub?.Dispose();
                    tcs.TrySetResult(RepairOrderItemListResult.Failure(MapErrorReason(resp.Reason), resp.Action));
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
                return RepairOrderItemListResult.Timeout();
            }

            return await tcs.Task.ConfigureAwait(false);
        }
        catch (System.OperationCanceledException)
        {
            return RepairOrderItemListResult.Failure("Yêu cầu bị hủy.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return RepairOrderItemListResult.Failure($"Lỗi không xác định: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    public async System.Threading.Tasks.Task<RepairOrderItemWriteResult> CreateAsync(RepairOrderItemDto data, System.Threading.CancellationToken ct = default)
        => await SendWritePacketAsync((System.UInt16)OpCommand.REPAIR_ORDER_ITEM_CREATE, data, expectEcho: true, ct).ConfigureAwait(false);

    public async System.Threading.Tasks.Task<RepairOrderItemWriteResult> UpdateAsync(RepairOrderItemDto data, System.Threading.CancellationToken ct = default)
        => await SendWritePacketAsync((System.UInt16)OpCommand.REPAIR_ORDER_ITEM_UPDATE, data, expectEcho: true, ct).ConfigureAwait(false);

    public async System.Threading.Tasks.Task<RepairOrderItemWriteResult> DeleteAsync(RepairOrderItemDto data, System.Threading.CancellationToken ct = default)
        => await SendWritePacketAsync((System.UInt16)OpCommand.REPAIR_ORDER_ITEM_DELETE, data, expectEcho: false, ct).ConfigureAwait(false);

    private async System.Threading.Tasks.Task<RepairOrderItemWriteResult> SendWritePacketAsync(
        System.UInt16 opcode,
        RepairOrderItemDto data,
        System.Boolean expectEcho,
        System.Threading.CancellationToken ct)
    {
        try
        {
            System.UInt32 sq = Csprng.NextUInt32();
            ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

            data.OpCode = opcode;
            data.SequenceId = sq;

            RepairOrderItemDto.Encrypt(data, client.Options.EncryptionKey, CipherSuiteType.SALSA20);

            System.Threading.Tasks.TaskCompletionSource<RepairOrderItemWriteResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? echoSub = null;
            System.IDisposable? errSub = null;

            if (expectEcho)
            {
                echoSub = client.OnOnce<RepairOrderItemDto>(
                    predicate: p => p.SequenceId == sq,
                    handler: confirmed =>
                    {
                        echoSub?.Dispose();
                        errSub?.Dispose();
                        tcs.TrySetResult(RepairOrderItemWriteResult.Success(confirmed));
                    });
            }

            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    echoSub?.Dispose();
                    errSub?.Dispose();
                    RepairOrderItemWriteResult result = resp.Type == ControlType.NONE
                        ? RepairOrderItemWriteResult.Success()
                        : RepairOrderItemWriteResult.Failure(MapErrorReason(resp.Reason), resp.Action);
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
                return RepairOrderItemWriteResult.Timeout();
            }

            RepairOrderItemWriteResult final = await tcs.Task.ConfigureAwait(false);
            if (final.IsSuccess)
            {
                _cache.Invalidate();
            }
            return final;
        }
        catch (System.OperationCanceledException)
        {
            return RepairOrderItemWriteResult.Failure("Yêu cầu bị hủy.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return RepairOrderItemWriteResult.Failure($"Lỗi không xác định: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    private static System.String MapErrorReason(ProtocolReason reason)
        => reason switch
        {
            ProtocolReason.NOT_FOUND => "Không tìm thấy phụ tùng trong lệnh.",
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

