// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Repairs;
using AutoX.Gara.Frontend.Results.Billings;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Billings;
using AutoX.Gara.Shared.Protocol.Invoices;
using Nalix.Common.Diagnostics.Abstractions;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security.Enums;
using Nalix.Framework.Injection;
using Nalix.Framework.Random;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using Nalix.Shared.Frames.Controls;
using System.Diagnostics;

namespace AutoX.Gara.Frontend.Services.Repairs;

public sealed class RepairOrderService
{
    private const System.Int32 QueryTimeoutMs = 10_000;
    private const System.Int32 WriteTimeoutMs = 30_000;
    private readonly RepairOrderQueryCache _cache;

    public RepairOrderService(RepairOrderQueryCache cache)
        => _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));

    public async System.Threading.Tasks.Task<RepairOrderListResult> GetListAsync(
        System.Int32 page,
        System.Int32 pageSize,
        System.Int32 filterCustomerId,
        System.Int32 filterVehicleId,
        System.String? searchTerm = null,
        RepairOrderSortField sortBy = RepairOrderSortField.OrderDate,
        System.Boolean sortDescending = true,
        System.Int32 filterInvoiceId = 0,
        RepairOrderStatus? filterStatus = null,
        System.Threading.CancellationToken ct = default)
    {
        RepairOrderCacheKey key = new(
            page, pageSize,
            searchTerm ?? System.String.Empty,
            sortBy, sortDescending,
            filterCustomerId, filterVehicleId, filterInvoiceId,
            filterStatus);

        if (_cache.TryGet(key, out RepairOrderCacheEntry? cached))
        {
            System.Boolean hasMore = page * pageSize < cached!.TotalCount;
            return RepairOrderListResult.Success(cached.RepairOrders, cached.TotalCount, hasMore);
        }

        try
        {
            ILogger? logger = InstanceManager.Instance.GetExistingInstance<ILogger>();
            Stopwatch sw = Stopwatch.StartNew();

            System.UInt32 sq = Csprng.NextUInt32();
            TcpSession client = InstanceManager.Instance.GetOrCreateInstance<TcpSession>();

            RepairOrderQueryRequest packet = new()
            {
                SequenceId = sq,
                Page = page,
                PageSize = pageSize,
                SearchTerm = searchTerm ?? System.String.Empty,
                SortBy = sortBy,
                SortDescending = sortDescending,
                FilterCustomerId = filterCustomerId,
                FilterVehicleId = filterVehicleId,
                FilterInvoiceId = filterInvoiceId,
                FilterStatus = filterStatus,
                OpCode = (System.UInt16)OpCommand.REPAIR_ORDER_GET
            };

            logger?.Info(
                $"[FE.{nameof(RepairOrderService)}:{nameof(GetListAsync)}] send seq={sq} op={(System.UInt16)OpCommand.REPAIR_ORDER_GET} page={page} size={pageSize} cust={filterCustomerId} veh={filterVehicleId} inv={filterInvoiceId} status={filterStatus} sort={sortBy} desc={sortDescending} term='{packet.SearchTerm}'");

            System.Threading.Tasks.TaskCompletionSource<RepairOrderListResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? sub = null;
            System.IDisposable? errSub = null;

            sub = client.OnOnce<RepairOrderQueryResponse>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();
                    errSub?.Dispose();

                    _cache.Set(key, resp.RepairOrders, resp.TotalCount);
                    System.Boolean hasMore = page * pageSize < resp.TotalCount;
                    logger?.Info(
                        $"[FE.{nameof(RepairOrderService)}:{nameof(GetListAsync)}] ok seq={sq} ms={sw.ElapsedMilliseconds} items={resp.RepairOrders?.Count ?? 0} total={resp.TotalCount}");
                    tcs.TrySetResult(RepairOrderListResult.Success(resp.RepairOrders!, resp.TotalCount, hasMore));
                });

            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();
                    errSub?.Dispose();
                    logger?.Warn(
                        $"[FE.{nameof(RepairOrderService)}:{nameof(GetListAsync)}] directive seq={sq} ms={sw.ElapsedMilliseconds} reason={resp.Reason} advice={resp.Action} type={resp.Type}");
                    tcs.TrySetResult(RepairOrderListResult.Failure(MapErrorReason(resp.Reason), resp.Action));
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
                logger?.Warn(
                    $"[FE.{nameof(RepairOrderService)}:{nameof(GetListAsync)}] timeout seq={sq} ms={sw.ElapsedMilliseconds} timeoutMs={QueryTimeoutMs}");
                return RepairOrderListResult.Timeout();
            }

            return await tcs.Task.ConfigureAwait(false);
        }
        catch (System.OperationCanceledException)
        {
            return RepairOrderListResult.Failure("Yêu cầu bị hủy.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return RepairOrderListResult.Failure($"Lỗi không xác định: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    public async System.Threading.Tasks.Task<RepairOrderWriteResult> CreateAsync(RepairOrderDto data, System.Threading.CancellationToken ct = default)
        => await SendWritePacketAsync((System.UInt16)OpCommand.REPAIR_ORDER_CREATE, data, expectEcho: true, ct).ConfigureAwait(false);

    public async System.Threading.Tasks.Task<RepairOrderWriteResult> UpdateAsync(RepairOrderDto data, System.Threading.CancellationToken ct = default)
        => await SendWritePacketAsync((System.UInt16)OpCommand.REPAIR_ORDER_UPDATE, data, expectEcho: true, ct).ConfigureAwait(false);

    public async System.Threading.Tasks.Task<RepairOrderWriteResult> DeleteAsync(RepairOrderDto data, System.Threading.CancellationToken ct = default)
        => await SendWritePacketAsync((System.UInt16)OpCommand.REPAIR_ORDER_DELETE, data, expectEcho: false, ct).ConfigureAwait(false);

    private async System.Threading.Tasks.Task<RepairOrderWriteResult> SendWritePacketAsync(
        System.UInt16 opcode,
        RepairOrderDto data,
        System.Boolean expectEcho,
        System.Threading.CancellationToken ct)
    {
        try
        {
            ILogger? logger = InstanceManager.Instance.GetExistingInstance<ILogger>();
            Stopwatch sw = Stopwatch.StartNew();

            System.UInt32 sq = Csprng.NextUInt32();
            TcpSession client = InstanceManager.Instance.GetOrCreateInstance<TcpSession>();

            data.OpCode = opcode;
            data.SequenceId = sq;

            logger?.Info(
                $"[FE.{nameof(RepairOrderService)}:{nameof(SendWritePacketAsync)}] send seq={sq} op={opcode} expectEcho={expectEcho} roId={data.RepairOrderId} cust={data.CustomerId} veh={data.VehicleId} inv={data.InvoiceId} status={data.Status}");

            RepairOrderDto.Encrypt(data, client.Options.EncryptionKey, CipherSuiteType.SALSA20);

            System.Threading.Tasks.TaskCompletionSource<RepairOrderWriteResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? echoSub = null;
            System.IDisposable? errSub = null;

            if (expectEcho)
            {
                echoSub = client.OnOnce<RepairOrderDto>(
                    predicate: p => p.SequenceId == sq,
                    handler: confirmed =>
                    {
                        echoSub?.Dispose();
                        errSub?.Dispose();
                        logger?.Info(
                            $"[FE.{nameof(RepairOrderService)}:{nameof(SendWritePacketAsync)}] ok seq={sq} ms={sw.ElapsedMilliseconds} roId={confirmed.RepairOrderId} inv={confirmed.InvoiceId} totalCost={confirmed.TotalRepairCost}");
                        tcs.TrySetResult(RepairOrderWriteResult.Success(confirmed));
                    });
            }

            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    echoSub?.Dispose();
                    errSub?.Dispose();

                    logger?.Warn(
                        $"[FE.{nameof(RepairOrderService)}:{nameof(SendWritePacketAsync)}] directive seq={sq} ms={sw.ElapsedMilliseconds} op={opcode} reason={resp.Reason} advice={resp.Action} type={resp.Type}");

                    RepairOrderWriteResult result = resp.Type == ControlType.NONE
                        ? RepairOrderWriteResult.Success()
                        : RepairOrderWriteResult.Failure(MapErrorReason(resp.Reason), resp.Action);

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
                logger?.Warn(
                    $"[FE.{nameof(RepairOrderService)}:{nameof(SendWritePacketAsync)}] timeout seq={sq} ms={sw.ElapsedMilliseconds} op={opcode} timeoutMs={WriteTimeoutMs}");
                return RepairOrderWriteResult.Timeout();
            }

            RepairOrderWriteResult final = await tcs.Task.ConfigureAwait(false);
            if (final.IsSuccess)
            {
                _cache.Invalidate();
            }

            return final;
        }
        catch (System.OperationCanceledException)
        {
            return RepairOrderWriteResult.Failure("Yêu cầu bị hủy.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return RepairOrderWriteResult.Failure($"Lỗi không xác định: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    private static System.String MapErrorReason(ProtocolReason reason)
        => reason switch
        {
            ProtocolReason.NOT_FOUND => "Không tìm thấy lệnh sửa chữa.",
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
