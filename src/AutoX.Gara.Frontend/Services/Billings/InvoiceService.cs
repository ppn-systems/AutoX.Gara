// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Frontend.Results.Billings;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Billings;
using Nalix.Common.Diagnostics.Abstractions;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security.Enums;
using Nalix.Framework.Injection;
using Nalix.Framework.Random;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using Nalix.Shared.Frames.Controls;

namespace AutoX.Gara.Frontend.Services.Billings;

public sealed class InvoiceService
{
    private const int RequestTimeoutMs = 10_000;
    private readonly InvoiceQueryCache _cache;

    public InvoiceService(InvoiceQueryCache cache)
        => _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));

    public async System.Threading.Tasks.Task<InvoiceListResult> GetListAsync(
        int page,
        int pageSize,
        int filterCustomerId,
        string? searchTerm = null,
        InvoiceSortField sortBy = InvoiceSortField.InvoiceDate,
        bool sortDescending = true,
        PaymentStatus? filterPaymentStatus = null,
        System.Threading.CancellationToken ct = default)
    {
        InvoiceCacheKey key = new(
            page, pageSize,
            searchTerm ?? string.Empty,
            sortBy, sortDescending,
            filterCustomerId, filterPaymentStatus);

        if (_cache.TryGet(key, out InvoiceCacheEntry? cached))
        {
            bool hasMore = page * pageSize < cached!.TotalCount;
            return InvoiceListResult.Success(cached.Invoices, cached.TotalCount, hasMore);
        }

        try
        {
            uint sq = Csprng.NextUInt32();
            ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

            InvoiceQueryRequest packet = new()
            {
                SequenceId = sq,
                Page = page,
                PageSize = pageSize,
                SearchTerm = searchTerm ?? string.Empty,
                SortBy = sortBy,
                SortDescending = sortDescending,
                FilterCustomerId = filterCustomerId,
                FilterPaymentStatus = filterPaymentStatus,
                OpCode = (ushort)OpCommand.INVOICE_GET
            };

            System.Threading.Tasks.TaskCompletionSource<InvoiceListResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? sub = null;
            System.IDisposable? errSub = null;

            sub = client.OnOnce<InvoiceQueryResponse>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();
                    errSub?.Dispose();
                    _cache.Set(key, resp.Invoices, resp.TotalCount);
                    bool hasMore = page * pageSize < resp.TotalCount;
                    tcs.TrySetResult(InvoiceListResult.Success(resp.Invoices, resp.TotalCount, hasMore));
                });

            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();
                    errSub?.Dispose();
                    tcs.TrySetResult(InvoiceListResult.Failure(MapErrorReason(resp.Reason), resp.Action));
                });

            await client.SendAsync(packet, ct).ConfigureAwait(false);

            using System.Threading.CancellationTokenSource cts =
                System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct);

            System.Threading.Tasks.Task timeoutTask = System.Threading.Tasks.Task.Delay(RequestTimeoutMs, cts.Token);
            System.Threading.Tasks.Task winner = await System.Threading.Tasks.Task.WhenAny(tcs.Task, timeoutTask).ConfigureAwait(false);
            cts.Cancel();

            if (!ReferenceEquals(winner, tcs.Task))
            {
                sub?.Dispose();
                errSub?.Dispose();
                return InvoiceListResult.Timeout();
            }

            return await tcs.Task.ConfigureAwait(false);
        }
        catch (System.OperationCanceledException)
        {
            return InvoiceListResult.Failure("Yêu cầu bị hủy.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return InvoiceListResult.Failure($"Lỗi không xác định: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    public async System.Threading.Tasks.Task<InvoiceWriteResult> CreateAsync(InvoiceDto data, System.Threading.CancellationToken ct = default)
        => await SendWritePacketAsync((ushort)OpCommand.INVOICE_CREATE, data, expectEcho: true, ct).ConfigureAwait(false);

    public async System.Threading.Tasks.Task<InvoiceWriteResult> UpdateAsync(InvoiceDto data, System.Threading.CancellationToken ct = default)
        => await SendWritePacketAsync((ushort)OpCommand.INVOICE_UPDATE, data, expectEcho: true, ct).ConfigureAwait(false);

    public async System.Threading.Tasks.Task<InvoiceWriteResult> DeleteAsync(InvoiceDto data, System.Threading.CancellationToken ct = default)
        => await SendWritePacketAsync((ushort)OpCommand.INVOICE_DELETE, data, expectEcho: false, ct).ConfigureAwait(false);

    private async System.Threading.Tasks.Task<InvoiceWriteResult> SendWritePacketAsync(
        ushort opcode,
        InvoiceDto data,
        bool expectEcho,
        System.Threading.CancellationToken ct)
    {
        try
        {
            uint sq = Csprng.NextUInt32();
            ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

            data.OpCode = opcode;
            data.SequenceId = sq;

            InvoiceDto.Encrypt(data, client.Options.EncryptionKey, CipherSuiteType.SALSA20);

            System.Threading.Tasks.TaskCompletionSource<InvoiceWriteResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? echoSub = null;
            System.IDisposable? errSub = null;

            if (expectEcho)
            {
                echoSub = client.OnOnce<InvoiceDto>(
                    predicate: p => p.SequenceId == sq,
                    handler: confirmed =>
                    {
                        echoSub?.Dispose();
                        errSub?.Dispose();
                        tcs.TrySetResult(InvoiceWriteResult.Success(confirmed));
                    });
            }

            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    echoSub?.Dispose();
                    errSub?.Dispose();
                    InvoiceWriteResult result = resp.Type == ControlType.NONE
                        ? InvoiceWriteResult.Success()
                        : InvoiceWriteResult.Failure(MapErrorReason(resp.Reason), resp.Action);
                    tcs.TrySetResult(result);
                });

            await client.SendAsync(data, ct).ConfigureAwait(false);

            using System.Threading.CancellationTokenSource cts =
                System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct);
            System.Threading.Tasks.Task timeoutTask = System.Threading.Tasks.Task.Delay(RequestTimeoutMs, cts.Token);
            System.Threading.Tasks.Task winner = await System.Threading.Tasks.Task.WhenAny(tcs.Task, timeoutTask).ConfigureAwait(false);
            cts.Cancel();

            if (!ReferenceEquals(winner, tcs.Task))
            {
                echoSub?.Dispose();
                errSub?.Dispose();
                return InvoiceWriteResult.Timeout();
            }

            InvoiceWriteResult final = await tcs.Task.ConfigureAwait(false);
            if (final.IsSuccess)
            {
                _cache.Invalidate();
            }
            return final;
        }
        catch (System.OperationCanceledException)
        {
            return InvoiceWriteResult.Failure("Yêu cầu bị hủy.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return InvoiceWriteResult.Failure($"Lỗi không xác định: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    private static string MapErrorReason(ProtocolReason reason)
        => reason switch
        {
            ProtocolReason.NOT_FOUND => "Không tìm thấy hóa đơn.",
            ProtocolReason.ALREADY_EXISTS => "Số hóa đơn đã tồn tại.",
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

