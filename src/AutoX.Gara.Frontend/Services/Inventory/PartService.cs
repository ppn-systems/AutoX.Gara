// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Parts;
using AutoX.Gara.Frontend.Results.Parts;
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
/// Frontend service communicating with server for part operations.
/// Handles GET/POST/PUT/DELETE for unified Part entity.
/// Cache 30s for GET, invalidate cache after all write operations.
/// </summary>
public sealed class PartService : IPartService
{
    private const System.Int32 RequestTimeoutMs = 10_000;
    private readonly IPartQueryCache _cache;

    /// <summary>
    /// Initializes a new instance of PartService.
    /// </summary>
    public PartService(IPartQueryCache cache)
        => _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));

    // --- GetListAsync ---------------------------------------------------------

    /// <summary>
    /// Retrieves a paginated list of parts with filtering and sorting.
    /// </summary>
    public async System.Threading.Tasks.Task<PartListResult> GetListAsync(
        System.Int32 page,
        System.Int32 pageSize,
        System.String? searchTerm = null,
        PartSortField sortBy = PartSortField.PartName,
        System.Boolean sortDescending = false,
        System.Int32? filterSupplierId = null,
        PartCategory? filterCategory = null,
        System.Boolean? filterInStock = null,
        System.Boolean? filterDefective = null,
        System.Boolean? filterExpired = null,
        System.Boolean? filterDiscontinued = null,
        System.Threading.CancellationToken ct = default)
    {
        PartCacheKey key = new(
            page, pageSize,
            searchTerm ?? System.String.Empty,
            sortBy, sortDescending,
            filterSupplierId, filterCategory,
            filterInStock, filterDefective, filterExpired, filterDiscontinued);

        if (_cache.TryGet(key, out PartCacheEntry? cached))
        {
            System.Boolean hasMore = page * pageSize < cached!.TotalCount;
            return PartListResult.Success(cached.Parts, cached.TotalCount, hasMore);
        }

        try
        {
            System.UInt32 sq = Csprng.NextUInt32();
            ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

            PartQueryRequest packet = new()
            {
                Page = page,
                PageSize = pageSize,
                SequenceId = sq,
                SearchTerm = searchTerm ?? System.String.Empty,
                SortBy = sortBy,
                SortDescending = sortDescending,
                FilterSupplierId = filterSupplierId ?? 0,
                FilterCategory = filterCategory,
                FilterInStock = filterInStock,
                FilterDefective = filterDefective,
                FilterExpired = filterExpired,
                FilterDiscontinued = filterDiscontinued,
                OpCode = (System.UInt16)OpCommand.PART_GET
            };

            System.Threading.Tasks.TaskCompletionSource<PartListResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? sub = null;
            System.IDisposable? errSub = null;

            sub = client.OnOnce<PartQueryResponse>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();
                    errSub?.Dispose();
                    _cache.Set(key, resp.Parts, resp.TotalCount);
                    System.Boolean hasMore = page * pageSize < resp.TotalCount;
                    tcs.TrySetResult(PartListResult.Success(resp.Parts, resp.TotalCount, hasMore));
                });

            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();
                    errSub?.Dispose();
                    tcs.TrySetResult(PartListResult.Failure(MapErrorReason(resp.Reason), resp.Action));
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
                return PartListResult.Timeout();
            }

            return await tcs.Task.ConfigureAwait(false);
        }
        catch (System.OperationCanceledException)
        {
            return PartListResult.Failure("Yêu c?u b? h?y.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return PartListResult.Failure($"L?i không xác d?nh: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    // --- CreateAsync ----------------------------------------------------------

    /// <summary>
    /// Creates a new part.
    /// </summary>
    public async System.Threading.Tasks.Task<PartWriteResult> CreateAsync(
        PartDto data,
        System.Threading.CancellationToken ct = default)
    {
        PartWriteResult result = await SendWritePacketAsync(
            (System.UInt16)OpCommand.PART_CREATE, data, expectEcho: true, ct).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            _cache.Invalidate();
        }

        return result;
    }

    // --- UpdateAsync ----------------------------------------------------------

    /// <summary>
    /// Updates an existing part.
    /// </summary>
    public async System.Threading.Tasks.Task<PartWriteResult> UpdateAsync(
        PartDto data,
        System.Threading.CancellationToken ct = default)
    {
        PartWriteResult result = await SendWritePacketAsync(
            (System.UInt16)OpCommand.PART_UPDATE, data, expectEcho: true, ct).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            _cache.Invalidate();
        }

        return result;
    }

    // --- DeleteAsync ----------------------------------------------------------

    /// <summary>
    /// Deletes or discontinues a part (soft delete via IsDiscontinued flag).
    /// </summary>
    public async System.Threading.Tasks.Task<PartWriteResult> DeleteAsync(
        PartDto data,
        System.Threading.CancellationToken ct = default)
    {
        PartWriteResult result = await SendWritePacketAsync(
            (System.UInt16)OpCommand.PART_DELETE, data, expectEcho: false, ct).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            _cache.Invalidate();
        }

        return result;
    }

    // --- Private Helpers -----------------------------------------------------

    private static async System.Threading.Tasks.Task<PartWriteResult> SendWritePacketAsync(
        System.UInt16 opcode,
        PartDto data,
        System.Boolean expectEcho,
        System.Threading.CancellationToken ct)
    {
        try
        {
            System.UInt32 sq = Csprng.NextUInt32();
            ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

            data.OpCode = opcode;
            data.SequenceId = sq;

            System.Threading.Tasks.TaskCompletionSource<PartWriteResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? echoSub = null;
            System.IDisposable? errSub = null;

            if (expectEcho)
            {
                echoSub = client.OnOnce<PartDto>(
                    predicate: p => p.SequenceId == sq,
                    handler: confirmed =>
                    {
                        echoSub?.Dispose();
                        errSub?.Dispose();
                        tcs.TrySetResult(PartWriteResult.Success(confirmed));
                    });
            }

            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    echoSub?.Dispose();
                    errSub?.Dispose();
                    PartWriteResult result = resp.Type == ControlType.NONE
                        ? PartWriteResult.Success()
                        : PartWriteResult.Failure(MapErrorReason(resp.Reason), resp.Action);
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
                return PartWriteResult.Timeout();
            }

            return await tcs.Task.ConfigureAwait(false);
        }
        catch (System.OperationCanceledException)
        {
            return PartWriteResult.Failure("Yêu c?u b? h?y.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return PartWriteResult.Failure($"L?i không xác d?nh: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    private static System.String MapErrorReason(ProtocolReason reason)
        => reason switch
        {
            ProtocolReason.NOT_FOUND => "Không tìm th?y phụ tùng.",
            ProtocolReason.ALREADY_EXISTS => "Mã SKU/phụ tùng dã t?n Tải.",
            ProtocolReason.MALFORMED_PACKET => "D? li?u không h?p l?.",
            ProtocolReason.INTERNAL_ERROR => "L?i h? th?ng. Vui lòng Thử lại sau.",
            ProtocolReason.FORBIDDEN => "B?n không có quy?n th?c hi?n thao tác này.",
            ProtocolReason.UNAUTHENTICATED => "B?n không có quy?n th?c hi?n thao tác này.",
            ProtocolReason.RATE_LIMITED => "B?n dang thao tác quá nhanh. Vui lòng ch? m?t chút r?i Thử lại.",
            ProtocolReason.TIMEOUT => "Máy ch? phụn h?i h?t h?n. Vui lòng Thử lại.",
            _ => "Thao tác thất bại. Vui lòng Thử lại."
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
/// Abstraction for part service.
/// </summary>
public interface IPartService
{
    /// <summary>
    /// Retrieves a paginated list of parts.
    /// </summary>
    System.Threading.Tasks.Task<PartListResult> GetListAsync(
        System.Int32 page,
        System.Int32 pageSize,
        System.String? searchTerm = null,
        PartSortField sortBy = PartSortField.PartName,
        System.Boolean sortDescending = false,
        System.Int32? filterSupplierId = null,
        PartCategory? filterCategory = null,
        System.Boolean? filterInStock = null,
        System.Boolean? filterDefective = null,
        System.Boolean? filterExpired = null,
        System.Boolean? filterDiscontinued = null,
        System.Threading.CancellationToken ct = default);

    /// <summary>
    /// Creates a new part.
    /// </summary>
    System.Threading.Tasks.Task<PartWriteResult> CreateAsync(PartDto data, System.Threading.CancellationToken ct = default);

    /// <summary>
    /// Updates an existing part.
    /// </summary>
    System.Threading.Tasks.Task<PartWriteResult> UpdateAsync(PartDto data, System.Threading.CancellationToken ct = default);

    /// <summary>
    /// Deletes or discontinues a part.
    /// </summary>
    System.Threading.Tasks.Task<PartWriteResult> DeleteAsync(PartDto data, System.Threading.CancellationToken ct = default);
}
