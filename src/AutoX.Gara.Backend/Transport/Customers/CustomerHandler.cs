using AutoX.Gara.Application.Customers;
using AutoX.Gara.Backend.Transport.Common;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Protocol.Customers;
using Nalix.Common.Networking;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security;
using Nalix.Framework.DataFrames.Pooling;
using Nalix.Framework.Injection;
using Nalix.Framework.Memory.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace AutoX.Gara.Backend.Transport.Customers;

/// <summary>
/// Packet Handler for customer related operations.
/// </summary>
[PacketController]
public sealed class CustomerHandler(CustomerAppService customerService)
{
    private readonly CustomerAppService _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.CUSTOMER_GET)]
    public async ValueTask GetAsync(IPacketContext<CustomerQueryRequest> context)
    {
        CustomerQueryRequest packet = context.Packet;
        IConnection connection = context.Connection;

        var result = await _customerService.GetPageAsync(BuildListQuery(packet)).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        using var lease = PacketPool<CustomerQueryResponse>.Rent();
        var response = lease.Value;
        response.TotalCount = result.Data!.totalCount;
        response.SequenceId = packet.SequenceId;
        response.Customers = result.Data.items.ConvertAll(c => MapToPacket(c, 0));

        try
        {
            await connection.TCP.SendAsync(response).ConfigureAwait(false);
        }
        finally
        {
            ReturnDtos(response.Customers);
        }
    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.CUSTOMER_CREATE)]
    public async ValueTask CreateAsync(IPacketContext<CustomerDto> context)
    {
        CustomerDto packet = context.Packet;
        IConnection connection = context.Connection;

        if (string.IsNullOrWhiteSpace(packet.Name))
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }

        var customer = new Customer
        {
            Name = packet.Name,
            Email = packet.Email,
            PhoneNumber = packet.PhoneNumber,
            Address = packet.Address,
            DateOfBirth = packet.DateOfBirth,
            TaxCode = packet.TaxCode,
            Type = packet.Type,
            Membership = packet.Membership,
            Gender = packet.Gender ?? AutoX.Gara.Domain.Enums.Gender.None,
            Notes = packet.Notes ?? string.Empty
        };

        var result = await _customerService.CreateAsync(customer).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        var confirmed = MapToPacket(result.Data!, packet.SequenceId);
        try
        {
            await connection.TCP.SendAsync(confirmed).ConfigureAwait(false);

        }
        finally
        {
            ReturnToPool(confirmed);
        }
    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.CUSTOMER_UPDATE)]
    public async ValueTask UpdateAsync(IPacketContext<CustomerDto> context)
    {
        CustomerDto packet = context.Packet;
        IConnection connection = context.Connection;

        if (packet.CustomerId == null)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }

        var customer = new Customer
        {
            Id = packet.CustomerId.Value,
            Name = packet.Name,
            Email = packet.Email,
            PhoneNumber = packet.PhoneNumber,
            Address = packet.Address,
            DateOfBirth = packet.DateOfBirth,
            TaxCode = packet.TaxCode,
            Type = packet.Type,
            Membership = packet.Membership,
            Gender = packet.Gender ?? AutoX.Gara.Domain.Enums.Gender.None,
            Notes = packet.Notes ?? string.Empty
        };

        var result = await _customerService.UpdateAsync(customer).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        var confirmed = MapToPacket(result.Data!, packet.SequenceId);
        try
        {
            await connection.TCP.SendAsync(confirmed).ConfigureAwait(false);

        }
        finally
        {
            ReturnToPool(confirmed);
        }
    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((ushort)OpCommand.CUSTOMER_DELETE)]
    public async ValueTask DeleteAsync(IPacketContext<CustomerDto> context)
    {
        CustomerDto packet = context.Packet;
        IConnection connection = context.Connection;

        if (packet.CustomerId == null)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }

        var result = await _customerService.DeleteAsync(packet.CustomerId.Value).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        await context.OkAsync().ConfigureAwait(false);
    }

    private static CustomerListQuery BuildListQuery(CustomerQueryRequest request)
        => new(request.Page, request.PageSize, request.SearchTerm, request.SortBy, request.SortDescending, request.FilterType, request.FilterMembership);

    private static CustomerDto MapToPacket(Customer c, ushort sequenceId)
    {
        var data = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Get<CustomerDto>();
        data.SequenceId = sequenceId;
        data.CustomerId = c.Id;
        data.Type = c.Type;
        data.Name = c.Name;
        data.Email = c.Email;
        data.PhoneNumber = c.PhoneNumber;
        data.Address = c.Address;
        data.DateOfBirth = c.DateOfBirth;
        data.TaxCode = c.TaxCode;
        data.Membership = c.Membership;
        data.Gender = c.Gender;
        data.Notes = c.Notes ?? string.Empty;
        data.CreatedAt = c.CreatedAt;
        data.UpdatedAt = c.UpdatedAt;
        return data;
    }

    private static void ReturnDtos(IEnumerable<CustomerDto> dtos)
    {
        if (dtos == null)
        {
            return;
        }

        var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
        foreach (var dto in dtos)
        {
            pool.Return(dto);
        }
    }

    private static void ReturnToPool(CustomerDto dto)
    {
        if (dto != null)
        {
            InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Return(dto);
        }
    }
}


