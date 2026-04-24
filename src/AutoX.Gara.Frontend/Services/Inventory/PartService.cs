// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Parts;
using AutoX.Gara.Frontend.Models.Results.Parts;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Inventory;
using Nalix.Common.Networking.Protocols;
using Nalix.Framework.DataFrames.SignalFrames;
using Nalix.Framework.Injection;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using System;

namespace AutoX.Gara.Frontend.Services.Inventory;

/// <summary>

/// Frontend service communicating with server for part operations.

/// Handles GET/POST/PUT/DELETE for unified Part entity.

/// Cache 30s for GET, invalidate cache after all write operations.

/// </summary>

public sealed class PartService : IPartService

{
    private const int RequestTimeoutMs = 10_000;

    private readonly IPartQueryCache _cache;

    public PartService(IPartQueryCache cache) => _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));

    public async System.Threading.Tasks.Task<PartListResult> GetListAsync(

        int page,

        int pageSize,

        string? searchTerm = null,

        PartSortField sortBy = PartSortField.PartName,

        bool sortDescending = false,

        int? filterSupplierId = null,

        PartCategory? filterCategory = null,

        bool? filterInStock = null,

        bool? filterDefective = null,

        bool? filterExpired = null,

        bool? filterDiscontinued = null,

        System.Threading.CancellationToken ct = default)

    {
        PartCacheKey key = new(page, pageSize, searchTerm ?? "", sortBy, sortDescending, filterSupplierId, filterCategory, filterInStock, filterDefective, filterExpired, filterDiscontinued);

        if (_cache.TryGet(key, out PartCacheEntry? cached))

        {
            return PartListResult.Success(cached!.Parts, cached!.TotalCount, page * pageSize < cached!.TotalCount);

        }

        try

        {
            TcpSession client = InstanceManager.Instance.GetExistingInstance<TcpSession>()!;

            PartQueryRequest packet = new() { Page = page, PageSize = pageSize, SearchTerm = searchTerm ?? "", SortBy = sortBy, SortDescending = sortDescending, FilterSupplierId = filterSupplierId ?? 0, FilterCategory = filterCategory, FilterInStock = filterInStock, FilterDefective = filterDefective, FilterExpired = filterExpired, FilterDiscontinued = filterDiscontinued, OpCode = (System.UInt16)OpCommand.PART_GET };

            Nalix.Common.Networking.Packets.IPacket r = await client.RequestAsync<Nalix.Common.Networking.Packets.IPacket>(packet, options: Nalix.SDK.Options.RequestOptions.Default.WithTimeout(RequestTimeoutMs).WithEncrypt(), predicate: p => p is PartQueryResponse or Directive, ct: ct).ConfigureAwait(false);

            if (r is PartQueryResponse resp)

            {
                _cache.Set(key, resp.Parts, resp.TotalCount);

                return PartListResult.Success(resp.Parts, resp.TotalCount, page * pageSize < resp.TotalCount);

            }

            return r is Directive err
                ? PartListResult.Failure(err.Reason.ToString(), err.Action)
                : PartListResult.Failure("Unknown response", ProtocolAdvice.NONE);
        }

        catch (System.TimeoutException) { return PartListResult.Timeout(); }

        catch (Exception ex) { return PartListResult.Failure(ex.Message, ProtocolAdvice.NONE); }

    }

    public async System.Threading.Tasks.Task<PartWriteResult> CreateAsync(PartDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.PART_CREATE, data, true, ct);

    public async System.Threading.Tasks.Task<PartWriteResult> UpdateAsync(PartDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.PART_UPDATE, data, true, ct);

    public async System.Threading.Tasks.Task<PartWriteResult> DeleteAsync(PartDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.PART_DELETE, data, false, ct);

    private async System.Threading.Tasks.Task<PartWriteResult> SendWriteAsync(System.UInt16 op, PartDto data, bool echo, System.Threading.CancellationToken ct)

    {
        try

        {
            data.OpCode = op;

            TcpSession client = InstanceManager.Instance.GetExistingInstance<TcpSession>()!;

            Nalix.Common.Networking.Packets.IPacket r = await client.RequestAsync<Nalix.Common.Networking.Packets.IPacket>(data, options: Nalix.SDK.Options.RequestOptions.Default.WithTimeout(RequestTimeoutMs).WithEncrypt(), predicate: p => (echo && p is PartDto) || p is Directive, ct: ct).ConfigureAwait(false);

            if (echo && r is PartDto confirmed)
            {
                return PartWriteResult.Success(confirmed);
            }

            if (r is Directive resp)

            {
                if (resp.Type == ControlType.NONE) { _cache.Invalidate(); return PartWriteResult.Success(); }

                return PartWriteResult.Failure(resp.Reason.ToString(), resp.Action);

            }

            return PartWriteResult.Failure("Unknown", ProtocolAdvice.NONE);

        }

        catch (Exception ex) { return PartWriteResult.Failure(ex.Message, ProtocolAdvice.NONE); }

    }
}

public interface IPartService

{
    /// <summary>

    /// Retrieves a paginated list of parts.

    /// </summary>

    System.Threading.Tasks.Task<PartListResult> GetListAsync(

        int page,

        int pageSize,

        string? searchTerm = null,

        PartSortField sortBy = PartSortField.PartName,

        bool sortDescending = false,

        int? filterSupplierId = null,

        PartCategory? filterCategory = null,

        bool? filterInStock = null,

        bool? filterDefective = null,

        bool? filterExpired = null,

        bool? filterDiscontinued = null,

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
