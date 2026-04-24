// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Frontend.Models.Results.Billings;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Repairs;
using Nalix.Common.Networking.Protocols;
using Nalix.Framework.DataFrames.SignalFrames;
using Nalix.Framework.Injection;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using System;
namespace AutoX.Gara.Frontend.Services.Repairs;
public sealed class RepairOrderItemService
{
    private const int RequestTimeoutMs = 10_000;
    private readonly RepairOrderItemQueryCache _cache;
    public RepairOrderItemService(RepairOrderItemQueryCache cache)
        => _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));
    public async System.Threading.Tasks.Task<RepairOrderItemListResult> GetListAsync(
        int page,
        int pageSize,
        int filterRepairOrderId,
        string? searchTerm = null,
        RepairOrderItemSortField sortBy = RepairOrderItemSortField.Id,
        bool sortDescending = true,
        int filterPartId = 0,
        System.Threading.CancellationToken ct = default)
    {
        RepairOrderItemCacheKey key = new(page, pageSize, searchTerm ?? "", sortBy, sortDescending, filterRepairOrderId, filterPartId);
        if (_cache.TryGet(key, out RepairOrderItemCacheEntry? cached))
        {
            return RepairOrderItemListResult.Success(cached!.Items, cached!.TotalCount, page * pageSize < cached!.TotalCount);
        }
        try
        {
            TcpSession client = InstanceManager.Instance.GetExistingInstance<TcpSession>()!;
            RepairOrderItemQueryRequest packet = new() { Page = page, PageSize = pageSize, SearchTerm = searchTerm ?? "", SortBy = sortBy, SortDescending = sortDescending, FilterRepairOrderId = filterRepairOrderId, FilterPartId = filterPartId, OpCode = (System.UInt16)OpCommand.REPAIR_ORDER_ITEM_GET };
            Nalix.Common.Networking.Packets.IPacket r = await client.RequestAsync<Nalix.Common.Networking.Packets.IPacket>(packet, options: Nalix.SDK.Options.RequestOptions.Default.WithTimeout(RequestTimeoutMs).WithEncrypt(), predicate: p => p is RepairOrderItemQueryResponse or Directive, ct: ct).ConfigureAwait(false);
            if (r is RepairOrderItemQueryResponse resp)
            {
                _cache.Set(key, resp.RepairOrderItems, resp.TotalCount);
                return RepairOrderItemListResult.Success(resp.RepairOrderItems, resp.TotalCount, page * pageSize < resp.TotalCount);
            }
            return r is Directive err
                ? RepairOrderItemListResult.Failure(err.Reason.ToString(), err.Action)
                : RepairOrderItemListResult.Failure("Unknown response", ProtocolAdvice.NONE);
        }
        catch (Exception ex) { return RepairOrderItemListResult.Failure(ex.Message, ProtocolAdvice.NONE); }
    }
    public async System.Threading.Tasks.Task<RepairOrderItemWriteResult> CreateAsync(RepairOrderItemDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.REPAIR_ORDER_ITEM_CREATE, data, true, ct);
    public async System.Threading.Tasks.Task<RepairOrderItemWriteResult> UpdateAsync(RepairOrderItemDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.REPAIR_ORDER_ITEM_UPDATE, data, true, ct);
    public async System.Threading.Tasks.Task<RepairOrderItemWriteResult> DeleteAsync(RepairOrderItemDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.REPAIR_ORDER_ITEM_DELETE, data, false, ct);
    private async System.Threading.Tasks.Task<RepairOrderItemWriteResult> SendWriteAsync(System.UInt16 op, RepairOrderItemDto data, bool echo, System.Threading.CancellationToken ct)
    {
        try
        {
            data.OpCode = op;
            TcpSession client = InstanceManager.Instance.GetExistingInstance<TcpSession>()!;
            Nalix.Common.Networking.Packets.IPacket r = await client.RequestAsync<Nalix.Common.Networking.Packets.IPacket>(data, options: Nalix.SDK.Options.RequestOptions.Default.WithTimeout(RequestTimeoutMs).WithEncrypt(), predicate: p => (echo && p is RepairOrderItemDto) || p is Directive, ct: ct).ConfigureAwait(false);
            if (echo && r is RepairOrderItemDto confirmed)
            {
                return RepairOrderItemWriteResult.Success(confirmed);
            }
            if (r is Directive resp)
            {
                if (resp.Type == ControlType.NONE) { _cache.Invalidate(); return RepairOrderItemWriteResult.Success(); }
                return RepairOrderItemWriteResult.Failure(resp.Reason.ToString(), resp.Action);
            }
            return RepairOrderItemWriteResult.Failure("Unknown", ProtocolAdvice.NONE);
        }
        catch (Exception ex) { return RepairOrderItemWriteResult.Failure(ex.Message, ProtocolAdvice.NONE); }
    }
}
