// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Frontend.Models.Results.Vehicles;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Protocol.Vehicles;
using Nalix.Common.Networking.Protocols;
using Nalix.Framework.DataFrames.SignalFrames;
using Nalix.Framework.Injection;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using System;
namespace AutoX.Gara.Frontend.Services.Vehicles;
/// <summary>
/// Service giao ti?p server cho Vehicle.
/// Pattern gi?ng h?t <c>CustomerService</c>:
/// cache ? network ? result.
/// </summary>
public sealed class VehicleService
{
    private const int RequestTimeoutMs = 10_000;
    private readonly VehicleQueryCache _cache;
    public VehicleService(VehicleQueryCache cache) => _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));
    public async System.Threading.Tasks.Task<VehicleListResult> GetListAsync(
        int customerId,
        int page,
        int pageSize,
        System.Threading.CancellationToken ct = default)
    {
        VehicleCacheKey key = new(customerId, page, pageSize);
        if (_cache.TryGet(key, out VehicleCacheEntry? cached))
        {
            return VehicleListResult.Success(cached!.Vehicles, cached!.TotalCount, page * pageSize < cached!.TotalCount);
        }
        try
        {
            TcpSession client = InstanceManager.Instance.GetExistingInstance<TcpSession>()!;
            VehicleDto packet = new() { CustomerId = customerId, VehicleId = null, Year = page, OpCode = (System.UInt16)OpCommand.VEHICLE_GET };
            Nalix.Common.Networking.Packets.IPacket r = await client.RequestAsync<Nalix.Common.Networking.Packets.IPacket>(packet, options: Nalix.SDK.Options.RequestOptions.Default.WithTimeout(RequestTimeoutMs).WithEncrypt(), predicate: p => p is VehiclesQueryResponse or Directive, ct: ct).ConfigureAwait(false);
            if (r is VehiclesQueryResponse resp)
            {
                _cache.Set(key, resp.Vehicles, resp.TotalCount);
                return VehicleListResult.Success(resp.Vehicles, resp.TotalCount, page * pageSize < resp.TotalCount);
            }
            return r is Directive err
                ? VehicleListResult.Failure(err.Reason.ToString(), err.Action)
                : VehicleListResult.Failure("Unknown response", ProtocolAdvice.NONE);
        }
        catch (System.TimeoutException) { return VehicleListResult.Timeout(); }
        catch (Exception ex) { return VehicleListResult.Failure(ex.Message, ProtocolAdvice.NONE); }
    }
    public async System.Threading.Tasks.Task<VehicleWriteResult> CreateAsync(VehicleDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.VEHICLE_CREATE, data, true, ct);
    public async System.Threading.Tasks.Task<VehicleWriteResult> UpdateAsync(VehicleDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.VEHICLE_UPDATE, data, true, ct);
    public async System.Threading.Tasks.Task<VehicleWriteResult> DeleteAsync(VehicleDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.VEHICLE_DELETE, data, false, ct);
    private async System.Threading.Tasks.Task<VehicleWriteResult> SendWriteAsync(System.UInt16 op, VehicleDto data, bool echo, System.Threading.CancellationToken ct)
    {
        try
        {
            data.OpCode = op;
            TcpSession client = InstanceManager.Instance.GetExistingInstance<TcpSession>()!;
            Nalix.Common.Networking.Packets.IPacket r = await client.RequestAsync<Nalix.Common.Networking.Packets.IPacket>(data, options: Nalix.SDK.Options.RequestOptions.Default.WithTimeout(RequestTimeoutMs).WithEncrypt(), predicate: p => (echo && p is VehicleDto) || p is Directive, ct: ct).ConfigureAwait(false);
            if (echo && r is VehicleDto confirmed)
            {
                return VehicleWriteResult.Success(confirmed);
            }
            if (r is Directive resp)
            {
                if (resp.Type == ControlType.NONE) { _cache.Invalidate(data.CustomerId); return VehicleWriteResult.Success(); }
                return VehicleWriteResult.Failure(resp.Reason.ToString(), resp.Action);
            }
            return VehicleWriteResult.Failure("Unknown", ProtocolAdvice.NONE);
        }
        catch (Exception ex) { return VehicleWriteResult.Failure(ex.Message, ProtocolAdvice.NONE); }
    }
}

