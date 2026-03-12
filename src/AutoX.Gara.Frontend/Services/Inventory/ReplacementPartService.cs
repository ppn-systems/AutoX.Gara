// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Parts;
using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Frontend.ViewModels.Results;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Inventory;
using Nalix.Common.Diagnostics.Abstractions;
using Nalix.Common.Networking.Protocols;
using Nalix.Framework.Injection;
using Nalix.Framework.Random;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using Nalix.Shared.Frames.Controls;

namespace AutoX.Gara.Frontend.Services.Inventory;

/// <summary>
/// Frontend service giao tiếp với server cho nghiệp vụ <c>ReplacementPart</c>.
/// <para>
/// Cache 30s cho GET, invalidate cache sau mọi write.
/// Delete là hard delete — server xóa vĩnh viễn.
/// </para>
/// </summary>
public sealed class ReplacementPartService : IReplacementPartService
{
    private const System.Int32 RequestTimeoutMs = 10_000;
    private readonly IReplacementPartQueryCache _cache;

    public ReplacementPartService(IReplacementPartQueryCache cache)
        => _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));

    // ─── GetListAsync ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async System.Threading.Tasks.Task<ReplacementPartListResult> GetListAsync(
        System.Int32 page,
        System.Int32 pageSize,
        System.String? searchTerm = null,
        ReplacementPartSortField sortBy = ReplacementPartSortField.DateAdded,
        System.Boolean sortDescending = true,
        System.Boolean? filterInStock = null,
        System.Boolean? filterDefective = null,
        System.Boolean? filterExpired = null,
        System.Threading.CancellationToken ct = default)
    {
        ReplacementPartCacheKey key = new(
            page, pageSize,
            searchTerm ?? System.String.Empty,
            sortBy, sortDescending,
            filterInStock, filterDefective, filterExpired);

        if (_cache.TryGet(key, out ReplacementPartCacheEntry? cached))
        {
            System.Boolean hasMore = page * pageSize < cached!.TotalCount;
            return ReplacementPartListResult.Success(cached.Parts, cached.TotalCount, hasMore);
        }

        try
        {
            System.UInt32 sq = Csprng.NextUInt32();
            ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

            ReplacementPartQueryRequest packet = new()
            {
                Page = page,
                PageSize = pageSize,
                SequenceId = sq,
                SearchTerm = searchTerm ?? System.String.Empty,
                SortBy = sortBy,
                SortDescending = sortDescending,
                FilterInStock = filterInStock,
                FilterDefective = filterDefective,
                FilterExpired = filterExpired,
                OpCode = (System.UInt16)OpCommand.REPLACEMENT_PART_GET
            };

            System.Threading.Tasks.TaskCompletionSource<ReplacementPartListResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? sub = null;
            System.IDisposable? errSub = null;

            sub = client.OnOnce<ReplacementPartQueryResponse>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();
                    errSub?.Dispose();
                    _cache.Set(key, resp.Parts, resp.TotalCount);
                    System.Boolean hasMore = page * pageSize < resp.TotalCount;
                    tcs.TrySetResult(ReplacementPartListResult.Success(resp.Parts, resp.TotalCount, hasMore));
                });

            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();
                    errSub?.Dispose();
                    tcs.TrySetResult(ReplacementPartListResult.Failure(MapErrorReason(resp.Reason), resp.Action));
                });

            await client.SendAsync(packet, ct).ConfigureAwait(false);

            using System.Threading.CancellationTokenSource cts = System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct);
            System.Threading.Tasks.Task timeoutTask = System.Threading.Tasks.Task.Delay(RequestTimeoutMs, cts.Token);
            System.Threading.Tasks.Task winner = await System.Threading.Tasks.Task.WhenAny(tcs.Task, timeoutTask).ConfigureAwait(false);

            cts.Cancel();

            if (!ReferenceEquals(winner, tcs.Task))
            {
                sub?.Dispose();
                errSub?.Dispose();
                return ReplacementPartListResult.Timeout();
            }

            return await tcs.Task.ConfigureAwait(false);
        }
        catch (System.OperationCanceledException)
        {
            return ReplacementPartListResult.Failure("Yêu cầu bị hủy.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return ReplacementPartListResult.Failure($"Lỗi không xác định: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    // ─── CreateAsync ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async System.Threading.Tasks.Task<ReplacementPartWriteResult> CreateAsync(
        ReplacementPartDto data,
        System.Threading.CancellationToken ct = default)
    {
        ReplacementPartWriteResult result = await SendWritePacketAsync(
            (System.UInt16)OpCommand.REPLACEMENT_PART_CREATE, data, expectEcho: true, ct).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            _cache.Invalidate();
        }

        return result;
    }

    // ─── UpdateAsync ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async System.Threading.Tasks.Task<ReplacementPartWriteResult> UpdateAsync(
        ReplacementPartDto data,
        System.Threading.CancellationToken ct = default)
    {
        ReplacementPartWriteResult result = await SendWritePacketAsync(
            (System.UInt16)OpCommand.REPLACEMENT_PART_UPDATE, data, expectEcho: true, ct).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            _cache.Invalidate();
        }

        return result;
    }

    // ─── DeleteAsync ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async System.Threading.Tasks.Task<ReplacementPartWriteResult> DeleteAsync(
        ReplacementPartDto data,
        System.Threading.CancellationToken ct = default)
    {
        ReplacementPartWriteResult result = await SendWritePacketAsync(
            (System.UInt16)OpCommand.REPLACEMENT_PART_DELETE, data, expectEcho: false, ct).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            _cache.Invalidate();
        }

        return result;
    }

    // ─── Private Helpers ─────────────────────────────────────────────────────

    private static async System.Threading.Tasks.Task<ReplacementPartWriteResult> SendWritePacketAsync(
        System.UInt16 opcode,
        ReplacementPartDto data,
        System.Boolean expectEcho,
        System.Threading.CancellationToken ct)
    {
        try
        {
            System.UInt32 sq = Csprng.NextUInt32();
            ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

            data.OpCode = opcode;
            data.SequenceId = sq;

            System.Threading.Tasks.TaskCompletionSource<ReplacementPartWriteResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? echoSub = null;
            System.IDisposable? errSub = null;

            if (expectEcho)
            {
                echoSub = client.OnOnce<ReplacementPartDto>(
                    predicate: p => p.SequenceId == sq,
                    handler: confirmed =>
                    {
                        echoSub?.Dispose();
                        errSub?.Dispose();
                        tcs.TrySetResult(ReplacementPartWriteResult.Success(confirmed));
                    });
            }

            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    echoSub?.Dispose();
                    errSub?.Dispose();
                    ReplacementPartWriteResult result = resp.Type == ControlType.NONE
                        ? ReplacementPartWriteResult.Success()
                        : ReplacementPartWriteResult.Failure(MapErrorReason(resp.Reason), resp.Action);
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
                return ReplacementPartWriteResult.Timeout();
            }

            return await tcs.Task.ConfigureAwait(false);
        }
        catch (System.OperationCanceledException)
        {
            return ReplacementPartWriteResult.Failure("Yêu cầu bị hủy.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return ReplacementPartWriteResult.Failure($"Lỗi không xác định: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    private static System.String MapErrorReason(ProtocolReason reason)
        => reason switch
        {
            ProtocolReason.NOT_FOUND => "Không tìm thấy phụ tùng kho.",
            ProtocolReason.ALREADY_EXISTS => "Mã SKU đã tồn tại trong kho.",
            ProtocolReason.MALFORMED_PACKET => "Dữ liệu không hợp lệ.",
            ProtocolReason.INTERNAL_ERROR => "Lỗi hệ thống. Vui lòng thử lại sau.",
            ProtocolReason.FORBIDDEN => "Bạn không có quyền thực hiện thao tác này.",
            ProtocolReason.UNAUTHENTICATED => "Bạn không có quyền thực hiện thao tác này.",
            ProtocolReason.RATE_LIMITED => "Bạn đang thao tác quá nhanh. Vui lòng chờ một chút rồi thử lại.",
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
