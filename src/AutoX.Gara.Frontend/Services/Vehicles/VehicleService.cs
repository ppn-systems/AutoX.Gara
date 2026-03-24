// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Results.Vehicles;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Vehicles;
using Nalix.Common.Diagnostics;
using Nalix.Common.Networking.Protocols;
using Nalix.Framework.Injection;
using Nalix.Framework.Random;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using Nalix.Shared.Frames.Controls;

namespace AutoX.Gara.Frontend.Services.Vehicles;

/// <summary>
/// Service giao ti?p server cho Vehicle.
/// Pattern gi?ng h?t <c>CustomerService</c>:
/// cache ? network ? result.
/// </summary>
public sealed class VehicleService
{
    private const System.Int32 RequestTimeoutMs = 10_000;

    private readonly VehicleQueryCache _cache;

    public VehicleService(VehicleQueryCache cache)
        => _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));

    // --- GetListAsync ---------------------------------------------------------

    /// <summary>L?y danh sách xe c?a m?t customer (có cache 30s).</summary>
    public async System.Threading.Tasks.Task<VehicleListResult> GetListAsync(
        System.Int32 customerId,
        System.Int32 page,
        System.Int32 pageSize,
        System.Threading.CancellationToken ct = default)
    {
        ILogger logger = InstanceManager.Instance.GetOrCreateInstance<ILogger>();
        VehicleCacheKey key = new(customerId, page, pageSize);

        logger.Info($"[VehicleService.GetListAsync] customerId={customerId} page={page} pageSize={pageSize}");

        if (_cache.TryGet(key, out VehicleCacheEntry? cached))
        {
            logger.Info($"[VehicleService.GetListAsync] CACHE HIT — returning {cached!.Vehicles.Count} vehicles, total={cached.TotalCount}");
            System.Boolean hasMore = page * pageSize < cached!.TotalCount;
            return VehicleListResult.Success(cached.Vehicles, cached.TotalCount, hasMore);
        }

        logger.Info("[VehicleService.GetListAsync] CACHE MISS — sending request to server");

        try
        {
            System.UInt32 sq = Csprng.NextUInt32();
            TcpSession client = InstanceManager.Instance.GetOrCreateInstance<TcpSession>();

            // VehicleId == null ? server x? lý nhu list request theo CustomerId
            // Page du?c encode vào Year field (xem VehicleOps.GetListByCustomerAsync)
            VehicleDto packet = new()
            {
                SequenceId = sq,
                CustomerId = customerId,
                VehicleId = null,   // null = list mode
                Year = page,   // encode page number vào Year
                OpCode = (System.UInt16)OpCommand.VEHICLE_GET
            };

            logger.Info($"[VehicleService.GetListAsync] Sending VehicleDto SeqId={sq} OpCode=0x{packet.OpCode:X3} CustomerId={customerId} Year(page)={page}");

            System.Threading.Tasks.TaskCompletionSource<VehicleListResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? sub = null;
            System.IDisposable? errSub = null;

            sub = client.OnOnce<VehiclesQueryResponse>(
                predicate: p =>
                {
                    System.Boolean match = p.SequenceId == sq;
                    logger.Info($"[VehicleService.GetListAsync] OnOnce<VehiclesQueryResponse> received SeqId={p.SequenceId} expected={sq} match={match} count={p.Vehicles?.Count} total={p.TotalCount}");
                    return match;
                },
                handler: resp =>
                {
                    logger.Info($"[VehicleService.GetListAsync] VehiclesQueryResponse MATCHED — vehicles={resp.Vehicles.Count} total={resp.TotalCount}");
                    sub?.Dispose();
                    errSub?.Dispose();
                    _cache.Set(key, resp.Vehicles, resp.TotalCount);
                    System.Boolean hasMore = page * pageSize < resp.TotalCount;
                    tcs.TrySetResult(VehicleListResult.Success(resp.Vehicles, resp.TotalCount, hasMore));
                });

            errSub = client.OnOnce<Directive>(
                predicate: p =>
                {
                    System.Boolean match = p.SequenceId == sq;
                    logger.Info($"[VehicleService.GetListAsync] OnOnce<Directive> received SeqId={p.SequenceId} expected={sq} match={match} Type={p.Type} Reason={p.Reason}");
                    return match;
                },
                handler: resp =>
                {
                    logger.Warn($"[VehicleService.GetListAsync] Directive ERROR — Type={resp.Type} Reason={resp.Reason} Action={resp.Action}");
                    sub?.Dispose();
                    errSub?.Dispose();
                    tcs.TrySetResult(VehicleListResult.Failure(MapErrorReason(resp.Reason), resp.Action));
                });

            await client.SendAsync(packet, ct).ConfigureAwait(false);
            logger.Info($"[VehicleService.GetListAsync] Packet sent, waiting for response (timeout={RequestTimeoutMs}ms)...");

            using System.Threading.CancellationTokenSource cts =
                System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct);

            System.Threading.Tasks.Task timeoutTask =
                System.Threading.Tasks.Task.Delay(RequestTimeoutMs, cts.Token);

            System.Threading.Tasks.Task winner =
                await System.Threading.Tasks.Task.WhenAny(tcs.Task, timeoutTask).ConfigureAwait(false);

            cts.Cancel();

            if (!ReferenceEquals(winner, tcs.Task))
            {
                logger.Warn($"[VehicleService.GetListAsync] TIMEOUT after {RequestTimeoutMs}ms — SeqId={sq} customerId={customerId} page={page}");
                sub?.Dispose();
                errSub?.Dispose();
                return VehicleListResult.Timeout();
            }

            VehicleListResult finalResult = await tcs.Task.ConfigureAwait(false);
            logger.Info($"[VehicleService.GetListAsync] Done — IsSuccess={finalResult.IsSuccess} count={finalResult.Vehicles.Count} total={finalResult.TotalCount}");
            return finalResult;
        }
        catch (System.OperationCanceledException)
        {
            logger.Warn("[VehicleService.GetListAsync] Request cancelled by caller.");
            return VehicleListResult.Failure("Yêu c?u b? Hủy.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return VehicleListResult.Failure($"L?i không xác d?nh: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    // --- CreateAsync ----------------------------------------------------------

    public async System.Threading.Tasks.Task<VehicleWriteResult> CreateAsync(
        VehicleDto data,
        System.Threading.CancellationToken ct = default)
    {
        VehicleWriteResult result = await SendWritePacketAsync(
            (System.UInt16)OpCommand.VEHICLE_CREATE, data, expectEcho: true, ct).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            _cache.Invalidate(data.CustomerId);
        }

        return result;
    }

    // --- UpdateAsync ----------------------------------------------------------

    public async System.Threading.Tasks.Task<VehicleWriteResult> UpdateAsync(
        VehicleDto data,
        System.Threading.CancellationToken ct = default)
    {
        VehicleWriteResult result = await SendWritePacketAsync(
            (System.UInt16)OpCommand.VEHICLE_UPDATE, data, expectEcho: true, ct).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            _cache.Invalidate(data.CustomerId);
        }

        return result;
    }

    // --- DeleteAsync ----------------------------------------------------------

    public async System.Threading.Tasks.Task<VehicleWriteResult> DeleteAsync(
        VehicleDto data,
        System.Threading.CancellationToken ct = default)
    {
        VehicleWriteResult result = await SendWritePacketAsync(
            (System.UInt16)OpCommand.VEHICLE_DELETE, data, expectEcho: false, ct).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            _cache.Invalidate(data.CustomerId);
        }

        return result;
    }

    // --- Private Helpers -----------------------------------------------------

    private static async System.Threading.Tasks.Task<VehicleWriteResult> SendWritePacketAsync(
        System.UInt16 opcode,
        VehicleDto data,
        System.Boolean expectEcho,
        System.Threading.CancellationToken ct)
    {
        try
        {
            System.UInt32 sq = Csprng.NextUInt32();
            TcpSession client = InstanceManager.Instance.GetOrCreateInstance<TcpSession>();

            data.OpCode = opcode;
            data.SequenceId = sq;

            ILogger logger = InstanceManager.Instance.GetOrCreateInstance<ILogger>();
            logger.Info($"[VehicleService] SeqId={sq} OpCode={opcode} expectEcho={expectEcho}");

            System.Threading.Tasks.TaskCompletionSource<VehicleWriteResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? echoSub = null;
            System.IDisposable? errSub = null;

            if (expectEcho)
            {
                echoSub = client.OnOnce<VehicleDto>(
                    predicate: p => p.SequenceId == sq,
                    handler: confirmed =>
                    {
                        echoSub?.Dispose();
                        errSub?.Dispose();
                        tcs.TrySetResult(VehicleWriteResult.Success(confirmed));
                    });
            }

            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    echoSub?.Dispose();
                    errSub?.Dispose();
                    VehicleWriteResult r = resp.Type == ControlType.NONE
                        ? VehicleWriteResult.Success()
                        : VehicleWriteResult.Failure(MapErrorReason(resp.Reason), resp.Action);
                    tcs.TrySetResult(r);
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
                return VehicleWriteResult.Timeout();
            }

            return await tcs.Task.ConfigureAwait(false);
        }
        catch (System.OperationCanceledException)
        {
            return VehicleWriteResult.Failure("Yêu c?u b? Hủy.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return VehicleWriteResult.Failure($"L?i không xác d?nh: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    private static System.String MapErrorReason(ProtocolReason reason)
        => reason switch
        {
            ProtocolReason.NOT_FOUND => "Không tìm tHủy xe.",
            ProtocolReason.ALREADY_EXISTS => "Bi?n s? ho?c s? khung/máy dã t?n Tải.",
            ProtocolReason.MALFORMED_PACKET => "D? li?u không h?p l?.",
            ProtocolReason.INTERNAL_ERROR => "L?i h? th?ng. Vui lòng Thử lại sau.",
            ProtocolReason.FORBIDDEN => "B?n không có quy?n th?c hi?n thao tác này.",
            ProtocolReason.UNAUTHENTICATED => "B?n không có quy?n th?c hi?n thao tác này.",
            ProtocolReason.RATE_LIMITED => "B?n dang thao tác quá nhanh. Vui lòng ch? m?t chút.",
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
