// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Frontend.ViewModels.Results;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Suppliers;
using Nalix.Common.Diagnostics.Abstractions;
using Nalix.Common.Networking.Protocols;
using Nalix.Framework.Injection;
using Nalix.Framework.Random;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using Nalix.Shared.Frames.Controls;

namespace AutoX.Gara.Frontend.Services.Suppliers;

/// <summary>
/// Real implementation của <see cref="ISupplierService"/>.
/// <para>
/// <list type="bullet">
///   <item>Inject <see cref="ISupplierQueryCache"/> → cache 30 giây tránh duplicate request.</item>
///   <item>Write operations tự động gọi <see cref="ISupplierQueryCache.Invalidate"/> sau khi thành công.</item>
///   <item>Cache hit hoàn toàn bypass network — trả về ngay từ memory.</item>
/// </list>
/// </para>
/// </summary>
public sealed class SupplierService : ISupplierService
{
    private const System.Int32 RequestTimeoutMs = 10_000;

    private readonly ISupplierQueryCache _cache;

    public SupplierService(ISupplierQueryCache cache)
        => _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));

    // ─── GetListAsync ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async System.Threading.Tasks.Task<SupplierListResult> GetListAsync(
        System.Int32 page,
        System.Int32 pageSize,
        System.String? searchTerm = null,
        SupplierSortField sortBy = SupplierSortField.Name,
        System.Boolean sortDescending = false,
        SupplierStatus filterStatus = SupplierStatus.None,
        PaymentTerms filterPaymentTerms = PaymentTerms.None,
        System.Threading.CancellationToken ct = default)
    {
        // ── Cache hit: trả về ngay, không tốn băng thông ─────────────────
        SupplierCacheKey key = new(
            page, pageSize,
            searchTerm ?? System.String.Empty,
            sortBy, sortDescending,
            filterStatus, filterPaymentTerms);

        if (_cache.TryGet(key, out SupplierCacheEntry? cached))
        {
            System.Boolean hasMore = page * pageSize < cached!.TotalCount;
            return SupplierListResult.Success(
                cached.Suppliers,
                totalCount: cached.TotalCount,
                hasMore: hasMore);
        }

        // ── Cache miss: gửi request lên server ────────────────────────────
        try
        {
            System.UInt32 sq = Csprng.NextUInt32();
            ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

            SupplierQueryRequest packet = new()
            {
                SequenceId = sq,
                Page = page,
                PageSize = pageSize,
                SearchTerm = searchTerm ?? System.String.Empty,
                SortBy = sortBy,
                SortDescending = sortDescending,
                FilterStatus = filterStatus,
                FilterPaymentTerms = filterPaymentTerms,
                OpCode = (System.UInt16)OpCommand.SUPPLIER_GET
            };

            System.Threading.Tasks.TaskCompletionSource<SupplierListResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? sub = null;
            System.IDisposable? errSub = null;

            sub = client.OnOnce<SupplierQueryResponse>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();
                    errSub?.Dispose();

                    _cache.Set(key, resp.Suppliers, resp.TotalCount);

                    System.Boolean hasMore = page * pageSize < resp.TotalCount;
                    tcs.TrySetResult(SupplierListResult.Success(
                        resp.Suppliers,
                        totalCount: resp.TotalCount,
                        hasMore: hasMore));
                });

            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();
                    errSub?.Dispose();
                    tcs.TrySetResult(
                        SupplierListResult.Failure(MapErrorReason(resp.Reason), resp.Action));
                });

            await client.SendAsync(packet, ct).ConfigureAwait(false);

            using System.Threading.CancellationTokenSource cts =
                System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct);

            System.Threading.Tasks.Task timeoutTask =
                System.Threading.Tasks.Task.Delay(RequestTimeoutMs, cts.Token);

            System.Threading.Tasks.Task winner =
                await System.Threading.Tasks.Task.WhenAny(tcs.Task, timeoutTask)
                    .ConfigureAwait(false);

            cts.Cancel();

            if (!ReferenceEquals(winner, tcs.Task))
            {
                sub?.Dispose();
                errSub?.Dispose();
                return SupplierListResult.Timeout();
            }

            return await tcs.Task.ConfigureAwait(false);
        }
        catch (System.OperationCanceledException)
        {
            return SupplierListResult.Failure("Yêu cầu bị hủy.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return SupplierListResult.Failure(
                $"Lỗi không xác định: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    // ─── CreateAsync ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async System.Threading.Tasks.Task<SupplierWriteResult> CreateAsync(
        SupplierDto data,
        System.Threading.CancellationToken ct = default)
    {
        SupplierWriteResult result = await SendWritePacketAsync(
            (System.UInt16)OpCommand.SUPPLIER_CREATE, data, expectEcho: true, ct)
            .ConfigureAwait(false);

        if (result.IsSuccess)
        {
            _cache.Invalidate();
        }

        return result;
    }

    // ─── UpdateAsync ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async System.Threading.Tasks.Task<SupplierWriteResult> UpdateAsync(
        SupplierDto data,
        System.Threading.CancellationToken ct = default)
    {
        SupplierWriteResult result = await SendWritePacketAsync(
            (System.UInt16)OpCommand.SUPPLIER_UPDATE, data, expectEcho: true, ct)
            .ConfigureAwait(false);

        if (result.IsSuccess)
        {
            _cache.Invalidate();
        }

        return result;
    }

    // ─── ChangeStatusAsync ────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async System.Threading.Tasks.Task<SupplierWriteResult> ChangeStatusAsync(
        System.Int32 supplierId,
        SupplierStatus newStatus,
        System.Threading.CancellationToken ct = default)
    {
        SupplierDto data = new()
        {
            SupplierId = supplierId,
            Status = newStatus
        };

        SupplierWriteResult result = await SendWritePacketAsync(
            (System.UInt16)OpCommand.SUPPLIER_CHANGE_STATUS, data, expectEcho: false, ct)
            .ConfigureAwait(false);

        if (result.IsSuccess)
        {
            _cache.Invalidate();
        }

        return result;
    }

    // ─── Private Helpers ─────────────────────────────────────────────────────

    private static async System.Threading.Tasks.Task<SupplierWriteResult> SendWritePacketAsync(
        System.UInt16 opcode,
        SupplierDto data,
        System.Boolean expectEcho,
        System.Threading.CancellationToken ct)
    {
        try
        {
            System.UInt32 sq = Csprng.NextUInt32();
            ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

            data.OpCode = opcode;
            data.SequenceId = sq;

            ILogger logger = InstanceManager.Instance.GetOrCreateInstance<ILogger>();
            logger.Info($"Sending packet SeqId={sq} OpCode={opcode} expectEcho={expectEcho}");

            System.Threading.Tasks.TaskCompletionSource<SupplierWriteResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? echoSub = null;
            System.IDisposable? errSub = null;

            if (expectEcho)
            {
                echoSub = client.OnOnce<SupplierDto>(
                    predicate: p => p.SequenceId == sq,
                    handler: confirmed =>
                    {
                        echoSub?.Dispose();
                        errSub?.Dispose();
                        tcs.TrySetResult(SupplierWriteResult.Success(confirmed));
                    });
            }

            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    echoSub?.Dispose();
                    errSub?.Dispose();

                    SupplierWriteResult result = resp.Type == ControlType.NONE
                        ? SupplierWriteResult.Success()
                        : SupplierWriteResult.Failure(MapErrorReason(resp.Reason), resp.Action);

                    tcs.TrySetResult(result);
                });

            await client.SendAsync(data, ct).ConfigureAwait(false);

            using System.Threading.CancellationTokenSource cts =
                System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct);

            System.Threading.Tasks.Task timeoutTask =
                System.Threading.Tasks.Task.Delay(RequestTimeoutMs, cts.Token);

            System.Threading.Tasks.Task winner =
                await System.Threading.Tasks.Task.WhenAny(tcs.Task, timeoutTask)
                    .ConfigureAwait(false);

            cts.Cancel();

            if (!ReferenceEquals(winner, tcs.Task))
            {
                echoSub?.Dispose();
                errSub?.Dispose();
                return SupplierWriteResult.Timeout();
            }

            return await tcs.Task.ConfigureAwait(false);
        }
        catch (System.OperationCanceledException)
        {
            return SupplierWriteResult.Failure("Yêu cầu bị hủy.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return SupplierWriteResult.Failure(
                $"Lỗi không xác định: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    private static System.String MapErrorReason(ProtocolReason reason)
        => reason switch
        {
            ProtocolReason.NOT_FOUND => "Không tìm thấy nhà cung cấp.",
            ProtocolReason.ALREADY_EXISTS => "Email hoặc mã số thuế đã tồn tại.",
            ProtocolReason.MALFORMED_PACKET => "Dữ liệu không hợp lệ.",
            ProtocolReason.INTERNAL_ERROR => "Lỗi hệ thống. Vui lòng thử lại sau.",
            ProtocolReason.FORBIDDEN => "Bạn không có quyền thực hiện thao tác này.",
            ProtocolReason.UNAUTHENTICATED => "Bạn không có quyền thực hiện thao tác này.",
            ProtocolReason.RATE_LIMITED => "Bạn đang thao tác quá nhanh. Vui lòng chờ một chút rồi thử lại.",
            ProtocolReason.UNSUPPORTED_PACKET => "Yêu cầu không được hỗ trợ. Vui lòng cập nhật phần mềm nếu có thể.",
            ProtocolReason.CRYPTO_UNSUPPORTED => "Lỗi mã hóa. Vui lòng cập nhật phần mềm nếu có thể.",
            ProtocolReason.COMPRESSION_FAILED => "Lỗi nén dữ liệu. Vui lòng cập nhật phần mềm nếu có thể.",
            ProtocolReason.TRANSFORM_FAILED => "Lỗi xử lý dữ liệu. Vui lòng cập nhật phần mềm nếu có thể.",
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
