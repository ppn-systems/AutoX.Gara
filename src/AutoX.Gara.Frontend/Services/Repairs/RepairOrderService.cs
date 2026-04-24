// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Repairs;
using AutoX.Gara.Frontend.Models.Results.Billings;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Invoices;
using Nalix.Common.Networking.Protocols;
using Nalix.Framework.DataFrames.SignalFrames;
using Nalix.Framework.Injection;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using System;
namespace AutoX.Gara.Frontend.Services.Repairs;
public sealed class RepairOrderService
{
    private const int QueryTimeoutMs = 10_000;
    private const int WriteTimeoutMs = 20_000;
    private readonly RepairOrderQueryCache _cache;
    public RepairOrderService(RepairOrderQueryCache cache)
        => _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));
    public async System.Threading.Tasks.Task<RepairOrderListResult> GetListAsync(
        int page,
        int pageSize,
        int filterCustomerId,
        int filterVehicleId,
        string? searchTerm = null,
        RepairOrderSortField sortBy = RepairOrderSortField.OrderDate,
        bool sortDescending = true,
        int filterInvoiceId = 0,
        RepairOrderStatus? filterStatus = null,
        System.Threading.CancellationToken ct = default)
    {
        RepairOrderCacheKey key = new(page, pageSize, searchTerm ?? "", sortBy, sortDescending, filterCustomerId, filterVehicleId, filterInvoiceId, filterStatus);
        if (_cache.TryGet(key, out RepairOrderCacheEntry? cached))
        {
            return RepairOrderListResult.Success(cached!.RepairOrders, cached!.TotalCount, page * pageSize < cached!.TotalCount);
        }
        try
        {
            TcpSession client = InstanceManager.Instance.GetExistingInstance<TcpSession>()!;
            RepairOrderQueryRequest packet = new() { Page = page, PageSize = pageSize, SearchTerm = searchTerm ?? "", SortBy = sortBy, SortDescending = sortDescending, FilterCustomerId = filterCustomerId, FilterVehicleId = filterVehicleId, FilterInvoiceId = filterInvoiceId, FilterStatus = filterStatus, OpCode = (System.UInt16)OpCommand.REPAIR_ORDER_GET };
            Nalix.Common.Networking.Packets.IPacket r = await client.RequestAsync<Nalix.Common.Networking.Packets.IPacket>(packet, options: Nalix.SDK.Options.RequestOptions.Default.WithTimeout(QueryTimeoutMs).WithEncrypt(), predicate: p => p is RepairOrderQueryResponse or Directive, ct: ct).ConfigureAwait(false);
            if (r is RepairOrderQueryResponse resp)
            {
                _cache.Set(key, resp.RepairOrders, resp.TotalCount);
                return RepairOrderListResult.Success(resp.RepairOrders, resp.TotalCount, page * pageSize < resp.TotalCount);
            }
            return r is Directive err
                ? RepairOrderListResult.Failure(err.Reason.ToString(), err.Action)
                : RepairOrderListResult.Failure("Unknown response", ProtocolAdvice.NONE);
        }
        catch (Exception ex) { return RepairOrderListResult.Failure(ex.Message, ProtocolAdvice.NONE); }
    }
    public async System.Threading.Tasks.Task<RepairOrderWriteResult> CreateAsync(RepairOrderDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.REPAIR_ORDER_CREATE, data, true, ct);
    public async System.Threading.Tasks.Task<RepairOrderWriteResult> UpdateAsync(RepairOrderDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.REPAIR_ORDER_UPDATE, data, true, ct);
    public async System.Threading.Tasks.Task<RepairOrderWriteResult> DeleteAsync(RepairOrderDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.REPAIR_ORDER_DELETE, data, false, ct);
    private async System.Threading.Tasks.Task<RepairOrderWriteResult> SendWriteAsync(System.UInt16 op, RepairOrderDto data, bool echo, System.Threading.CancellationToken ct)
    {
        try
        {
            data.OpCode = op;
            TcpSession client = InstanceManager.Instance.GetExistingInstance<TcpSession>()!;
            Nalix.Common.Networking.Packets.IPacket r = await client.RequestAsync<Nalix.Common.Networking.Packets.IPacket>(data, options: Nalix.SDK.Options.RequestOptions.Default.WithTimeout(WriteTimeoutMs).WithEncrypt(), predicate: p => (echo && p is RepairOrderDto) || p is Directive, ct: ct).ConfigureAwait(false);
            if (echo && r is RepairOrderDto confirmed)
            {
                return RepairOrderWriteResult.Success(confirmed);
            }
            if (r is Directive resp)
            {
                if (resp.Type == ControlType.NONE) { _cache.Invalidate(); return RepairOrderWriteResult.Success(); }
                return RepairOrderWriteResult.Failure(resp.Reason.ToString(), resp.Action);
            }
            return RepairOrderWriteResult.Failure("Unknown", ProtocolAdvice.NONE);
        }
        catch (Exception ex) { return RepairOrderWriteResult.Failure(ex.Message, ProtocolAdvice.NONE); }
    }
}
