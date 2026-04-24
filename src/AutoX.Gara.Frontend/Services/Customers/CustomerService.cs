// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Customers;
using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Frontend.Models.Results.Customer;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Customers;
using Nalix.Common.Networking.Protocols;
using Nalix.Framework.DataFrames.SignalFrames;
using Nalix.Framework.Injection;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using System;
namespace AutoX.Gara.Frontend.Services.Customers;
/// <summary>
/// Real implementation c?a <see cref="ICustomerService"/>.
/// <para>
/// Thay d?i so v?i version cu:
/// <list type="bullet">
///   <item>Inject <see cref="ICustomerQueryCache"/> ? cache 30 gi?y tr?nh duplicate request.</item>
///   <item>Write operations t? d?ng g?i <see cref="ICustomerQueryCache.Invalidate"/> sau khi th?nh c?ng.</item>
///   <item>Cache hit ho?n to?n bypass network ? tr? v? ngay t? memory.</item>
/// </list>
/// </para>
/// </summary>
public sealed class CustomerService : ICustomerService
{
    private const int RequestTimeoutMs = 10_000;
    private readonly ICustomerQueryCache _cache;
    public CustomerService(ICustomerQueryCache cache) => _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));
    public async System.Threading.Tasks.Task<CustomerListResult> GetListAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        CustomerSortField sortBy = CustomerSortField.CreatedAt,
        bool sortDescending = true,
        CustomerType filterType = CustomerType.None,
        MembershipLevel filterMembership = MembershipLevel.None,
        System.Threading.CancellationToken ct = default)
    {
        CustomerCacheKey key = new(page, pageSize, searchTerm ?? "", sortBy, sortDescending, filterType, filterMembership);
        if (_cache.TryGet(key, out CustomerCacheEntry? cached))
        {
            return CustomerListResult.Success(cached!.Customers, cached!.TotalCount, page * pageSize < cached!.TotalCount);
        }
        try
        {
            TcpSession client = InstanceManager.Instance.GetExistingInstance<TcpSession>()!;
            CustomerQueryRequest packet = new() { Page = page, PageSize = pageSize, SearchTerm = searchTerm ?? "", SortBy = sortBy, SortDescending = sortDescending, FilterType = filterType, FilterMembership = filterMembership, OpCode = (System.UInt16)OpCommand.CUSTOMER_GET };
            Nalix.Common.Networking.Packets.IPacket r = await client.RequestAsync<Nalix.Common.Networking.Packets.IPacket>(packet, options: Nalix.SDK.Options.RequestOptions.Default.WithTimeout(RequestTimeoutMs).WithEncrypt(), predicate: p => p is CustomerQueryResponse or Directive, ct: ct).ConfigureAwait(false);
            if (r is CustomerQueryResponse resp)
            {
                _cache.Set(key, resp.Customers, resp.TotalCount);
                return CustomerListResult.Success(resp.Customers, resp.TotalCount, page * pageSize < resp.TotalCount);
            }
            return r is Directive err
                ? CustomerListResult.Failure(err.Reason.ToString(), err.Action)
                : CustomerListResult.Failure("Unknown response", ProtocolAdvice.NONE);
        }
        catch (System.TimeoutException) { return CustomerListResult.Timeout(); }
        catch (Exception ex) { return CustomerListResult.Failure(ex.Message, ProtocolAdvice.NONE); }
    }
    public async System.Threading.Tasks.Task<CustomerWriteResult> CreateAsync(CustomerDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.CUSTOMER_CREATE, data, true, ct);
    public async System.Threading.Tasks.Task<CustomerWriteResult> UpdateAsync(CustomerDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.CUSTOMER_UPDATE, data, true, ct);
    public async System.Threading.Tasks.Task<CustomerWriteResult> DeleteAsync(CustomerDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.CUSTOMER_DELETE, data, false, ct);
    private async System.Threading.Tasks.Task<CustomerWriteResult> SendWriteAsync(System.UInt16 op, CustomerDto data, bool echo, System.Threading.CancellationToken ct)
    {
        try
        {
            data.OpCode = op;
            TcpSession client = InstanceManager.Instance.GetExistingInstance<TcpSession>()!;
            Nalix.Common.Networking.Packets.IPacket r = await client.RequestAsync<Nalix.Common.Networking.Packets.IPacket>(data, options: Nalix.SDK.Options.RequestOptions.Default.WithTimeout(RequestTimeoutMs).WithEncrypt(), predicate: p => (echo && p is CustomerDto) || p is Directive, ct: ct).ConfigureAwait(false);
            if (echo && r is CustomerDto confirmed)
            {
                return CustomerWriteResult.Success(confirmed);
            }
            if (r is Directive resp)
            {
                if (resp.Type == ControlType.NONE) { _cache.Invalidate(); return CustomerWriteResult.Success(); }
                return CustomerWriteResult.Failure(resp.Reason.ToString(), resp.Action);
            }
            return CustomerWriteResult.Failure("Unknown", ProtocolAdvice.NONE);
        }
        catch (Exception ex) { return CustomerWriteResult.Failure(ex.Message, ProtocolAdvice.NONE); }
    }
}
