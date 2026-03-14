// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Customers;
using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Frontend.Models.Results.Customer;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Customers;
using Nalix.Common.Diagnostics.Abstractions;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security.Enums;
using Nalix.Framework.Injection;
using Nalix.Framework.Random;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using Nalix.Shared.Frames.Controls;

namespace AutoX.Gara.Frontend.Services.Customers;

/// <summary>
/// Real implementation c?a <see cref="ICustomerService"/>.
/// <para>
/// Thay đổi so vụi version cu:
/// <list type="bullet">
///   <item>Inject <see cref="ICustomerQueryCache"/> ? cache 30 giây tránh duplicate request.</item>
///   <item>Write operations t? d?ng g?i <see cref="ICustomerQueryCache.Invalidate"/> sau khi thành công.</item>
///   <item>Cache hit hoàn toàn bypass network — tr? vụ ngay t? memory.</item>
/// </list>
/// </para>
/// </summary>
public sealed class CustomerService : ICustomerService
{
    private const System.Int32 RequestTimeoutMs = 10_000;

    private readonly ICustomerQueryCache _cache;

    public CustomerService(ICustomerQueryCache cache) => _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));

    // --- GetListAsync ---------------------------------------------------------

    /// <inheritdoc/>
    public async System.Threading.Tasks.Task<CustomerListResult> GetListAsync(
        System.Int32 page,
        System.Int32 pageSize,
        System.String? searchTerm = null,
        CustomerSortField sortBy = CustomerSortField.CreatedAt,
        System.Boolean sortDescending = true,
        CustomerType filterType = CustomerType.None,
        MembershipLevel filterMembership = MembershipLevel.None,
        System.Threading.CancellationToken ct = default)
    {
        // -- Cache hit: tr? vụ ngay, không t?n bang thông -----------------
        CustomerCacheKey key = new(
            page, pageSize,
            searchTerm ?? System.String.Empty,
            sortBy, sortDescending,
            filterType, filterMembership);

        if (_cache.TryGet(key, out CustomerCacheEntry? cached))
        {
            System.Boolean hasMore = page * pageSize < cached!.TotalCount;
            return CustomerListResult.Success(
                cached.Customers,
                totalCount: cached.TotalCount,
                hasMore: hasMore);
        }

        // -- Cache miss: g?i request lên server ----------------------------
        try
        {
            System.UInt32 sq = Csprng.NextUInt32();
            ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

            CustomerQueryRequest packet = new()
            {
                Page = page,
                SequenceId = sq,
                PageSize = pageSize,
                SearchTerm = searchTerm ?? System.String.Empty,
                SortBy = sortBy,
                SortDescending = sortDescending,
                FilterType = filterType,
                FilterMembership = filterMembership,
                OpCode = (System.UInt16)OpCommand.CUSTOMER_GET
            };

            System.Threading.Tasks.TaskCompletionSource<CustomerListResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? sub = null;
            System.IDisposable? errSub = null;

            sub = client.OnOnce<CustomerQueryResponse>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();
                    errSub?.Dispose();

                    // Luu k?t qu? vào cache 30s
                    _cache.Set(key, resp.Customers, resp.TotalCount);

                    System.Boolean hasMore = page * pageSize < resp.TotalCount;
                    tcs.TrySetResult(CustomerListResult.Success(
                        resp.Customers,
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
                        CustomerListResult.Failure(MapErrorReason(resp.Reason), resp.Action));
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
                return CustomerListResult.Timeout();
            }

            return await tcs.Task.ConfigureAwait(false);
        }
        catch (System.OperationCanceledException)
        {
            return CustomerListResult.Failure("Yêu c?u b? h?y.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return CustomerListResult.Failure(
                $"L?i không xác d?nh: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    // --- CreateAsync ----------------------------------------------------------

    /// <inheritdoc/>
    public async System.Threading.Tasks.Task<CustomerWriteResult> CreateAsync(
        CustomerDto data,
        System.Threading.CancellationToken ct = default)
    {
        CustomerWriteResult result = await SendWritePacketAsync(
            (System.UInt16)OpCommand.CUSTOMER_CREATE, data, expectEcho: true, ct)
            .ConfigureAwait(false);

        // D? li?u mới ? cache cu không còn chính xác
        if (result.IsSuccess)
        {
            _cache.Invalidate();
        }

        return result;
    }

    // --- UpdateAsync ----------------------------------------------------------

    /// <inheritdoc/>
    public async System.Threading.Tasks.Task<CustomerWriteResult> UpdateAsync(
        CustomerDto data,
        System.Threading.CancellationToken ct = default)
    {
        CustomerWriteResult result = await SendWritePacketAsync(
            (System.UInt16)OpCommand.CUSTOMER_UPDATE, data, expectEcho: true, ct)
            .ConfigureAwait(false);

        if (result.IsSuccess)
        {
            _cache.Invalidate();
        }

        return result;
    }

    // --- DeleteAsync ----------------------------------------------------------

    /// <inheritdoc/>
    public async System.Threading.Tasks.Task<CustomerWriteResult> DeleteAsync(
        CustomerDto data,
        System.Threading.CancellationToken ct = default)
    {
        CustomerWriteResult result = await SendWritePacketAsync(
            (System.UInt16)OpCommand.CUSTOMER_DELETE, data, expectEcho: false, ct)
            .ConfigureAwait(false);

        if (result.IsSuccess)
        {
            _cache.Invalidate();
        }

        return result;
    }

    // --- Private Helpers -----------------------------------------------------

    private static async System.Threading.Tasks.Task<CustomerWriteResult> SendWritePacketAsync(
        System.UInt16 opcode,
        CustomerDto data,
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

            CustomerDto.Encrypt(data, client.Options.EncryptionKey, CipherSuiteType.SALSA20);

            System.Threading.Tasks.TaskCompletionSource<CustomerWriteResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? echoSub = null;
            System.IDisposable? errSub = null;

            if (expectEcho)
            {
                echoSub = client.OnOnce<CustomerDto>(
                    predicate: p => p.SequenceId == sq,
                    handler: confirmed =>
                    {
                        echoSub?.Dispose();
                        errSub?.Dispose();
                        tcs.TrySetResult(CustomerWriteResult.Success(confirmed));
                    });
            }

            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    echoSub?.Dispose();
                    errSub?.Dispose();

                    CustomerWriteResult result = resp.Type == ControlType.NONE
                        ? CustomerWriteResult.Success()
                        : CustomerWriteResult.Failure(MapErrorReason(resp.Reason), resp.Action);

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
                return CustomerWriteResult.Timeout();
            }

            return await tcs.Task.ConfigureAwait(false);
        }
        catch (System.OperationCanceledException)
        {
            return CustomerWriteResult.Failure("Yêu c?u b? h?y.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return CustomerWriteResult.Failure(
                $"L?i không xác d?nh: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    private static System.String MapErrorReason(ProtocolReason reason)
        => reason switch
        {
            ProtocolReason.NOT_FOUND => "Không tìm th?y khách hàng.",
            ProtocolReason.ALREADY_EXISTS => "Email ho?c s? di?n tho?i dã t?n Tải.",
            ProtocolReason.MALFORMED_PACKET => "D? li?u không h?p l?.",
            ProtocolReason.INTERNAL_ERROR => "L?i h? th?ng. Vui lòng Thử lại sau.",
            ProtocolReason.FORBIDDEN => "B?n không có quy?n th?c hi?n thao tác này.",
            ProtocolReason.UNAUTHENTICATED => "B?n không có quy?n th?c hi?n thao tác này.",
            ProtocolReason.RATE_LIMITED => "B?n dang thao tác quá nhanh. Vui lòng ch? m?t chút r?i Thử lại.",
            ProtocolReason.UNSUPPORTED_PACKET => "Yêu c?u không du?c h? tr?. Vui lòng c?p nh?t phụn m?m n?u có th?.",
            ProtocolReason.CRYPTO_UNSUPPORTED => "L?i mã hóa. Vui lòng c?p nh?t phụn m?m n?u có th?.",
            ProtocolReason.COMPRESSION_FAILED => "L?i nén d? li?u. Vui lòng c?p nh?t phụn m?m n?u có th?.",
            ProtocolReason.TRANSFORM_FAILED => "L?i x? lý d? li?u. Vui lòng c?p nh?t phụn m?m n?u có th?.",
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
