using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;

using AutoX.Gara.Frontend.Results.ServiceItems;

using Nalix.Common.Networking.Protocols;

using AutoX.Gara.Shared.Protocol.Billings;

using Microsoft.Extensions.Logging;

using Nalix.Common.Networking.Protocols;

using Nalix.Framework.Injection;

using Nalix.Framework.Random;

using Nalix.SDK.Transport;

using Nalix.SDK.Transport.Extensions;

using Nalix.Framework.DataFrames.SignalFrames;

namespace AutoX.Gara.Frontend.Services.Billings;

public sealed class ServiceItemService

{
    private const int RequestTimeoutMs = 10_000;

    private readonly ServiceItemQueryCache _cache;

    public ServiceItemService(ServiceItemQueryCache cache)

        => _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));

    public async System.Threading.Tasks.Task<ServiceItemListResult> GetListAsync(

        int page,

        int pageSize,

        string? searchTerm = null,

        ServiceItemSortField sortBy = ServiceItemSortField.Description,

        bool sortDescending = false,

        ServiceType? filterType = null,

        decimal? filterMinUnitPrice = null,

        decimal? filterMaxUnitPrice = null,

        System.Threading.CancellationToken ct = default)

    {
        ServiceItemCacheKey key = new(page, pageSize, searchTerm ?? "", sortBy, sortDescending, filterType, filterMinUnitPrice, filterMaxUnitPrice);

        if (_cache.TryGet(key, out ServiceItemCacheEntry? cached))

        {
            return ServiceItemListResult.Success(cached.ServiceItems, cached.TotalCount, page * pageSize < cached.TotalCount);

        }

        try

        {
            TcpSession client = InstanceManager.Instance.GetExistingInstance<TcpSession>()!;

            ServiceItemQueryRequest packet = new() { Page = page, PageSize = pageSize, SearchTerm = searchTerm ?? "", SortBy = sortBy, SortDescending = sortDescending, FilterType = filterType, FilterMinUnitPrice = filterMinUnitPrice, FilterMaxUnitPrice = filterMaxUnitPrice, OpCode = (System.UInt16)OpCommand.SERVICE_ITEM_GET };

            Nalix.Common.Networking.Packets.IPacket r = await client.RequestAsync<Nalix.Common.Networking.Packets.IPacket>(packet, options: Nalix.SDK.Options.RequestOptions.Default.WithTimeout(RequestTimeoutMs).WithEncrypt(), predicate: p => p is ServiceItemQueryResponse or Directive, ct: ct).ConfigureAwait(false);

            if (r is ServiceItemQueryResponse resp)

            {
                _cache.Set(key, resp.ServiceItems, resp.TotalCount);

                return ServiceItemListResult.Success(resp.ServiceItems, resp.TotalCount, page * pageSize < resp.TotalCount);

            }

            if (r is Directive err) return ServiceItemListResult.Failure(err.Reason.ToString(), err.Action);

            return ServiceItemListResult.Failure("Unknown response", ProtocolAdvice.NONE);

        }

        catch (Exception ex) { return ServiceItemListResult.Failure(ex.Message, ProtocolAdvice.NONE); }

    }

    public async System.Threading.Tasks.Task<ServiceItemWriteResult> CreateAsync(ServiceItemDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.SERVICE_ITEM_CREATE, data, true, ct);

    public async System.Threading.Tasks.Task<ServiceItemWriteResult> UpdateAsync(ServiceItemDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.SERVICE_ITEM_UPDATE, data, true, ct);

    public async System.Threading.Tasks.Task<ServiceItemWriteResult> DeleteAsync(ServiceItemDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.SERVICE_ITEM_DELETE, data, false, ct);

    private async System.Threading.Tasks.Task<ServiceItemWriteResult> SendWriteAsync(System.UInt16 op, ServiceItemDto data, bool echo, System.Threading.CancellationToken ct)

    {
        try

        {
            data.OpCode = op;

            TcpSession client = InstanceManager.Instance.GetExistingInstance<TcpSession>()!;

            Nalix.Common.Networking.Packets.IPacket r = await client.RequestAsync<Nalix.Common.Networking.Packets.IPacket>(data, options: Nalix.SDK.Options.RequestOptions.Default.WithTimeout(RequestTimeoutMs).WithEncrypt(), predicate: p => (echo && p is ServiceItemDto) || p is Directive, ct: ct).ConfigureAwait(false);

            if (echo && r is ServiceItemDto confirmed) return ServiceItemWriteResult.Success(confirmed);

            if (r is Directive resp)

            {
                if (resp.Type == ControlType.NONE) { _cache.Invalidate(); return ServiceItemWriteResult.Success(); }

                return ServiceItemWriteResult.Failure(resp.Reason.ToString(), resp.Action);

            }

            return ServiceItemWriteResult.Failure("Unknown", ProtocolAdvice.NONE);

        }

        catch (Exception ex) { return ServiceItemWriteResult.Failure(ex.Message, ProtocolAdvice.NONE); }

    }
}