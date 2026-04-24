using AutoX.Gara.Application.Suppliers;
using AutoX.Gara.Backend.Transport.Common;
// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Entities.Suppliers;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Protocol.Suppliers;
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
namespace AutoX.Gara.Backend.Transport.Suppliers;
/// <summary>
/// Packet Handler for supplier related operations.
/// </summary>
[PacketController]
public sealed class SupplierHandler(SupplierAppService supplierService)
{
    private readonly SupplierAppService _supplierService = supplierService ?? throw new ArgumentNullException(nameof(supplierService));
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.SUPPLIER_GET)]
    public async ValueTask GetAsync(IPacketContext<SupplierQueryRequest> context)
    {
        SupplierQueryRequest packet = context.Packet;
        IConnection connection = context.Connection;
        var query = new SupplierListQuery(
            packet.Page,
            packet.PageSize,
            packet.SearchTerm ?? string.Empty,
            packet.SortBy,
            packet.SortDescending,
            packet.FilterStatus,
            packet.FilterPaymentTerms
        );
        var result = await _supplierService.GetPageAsync(query).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }
        using var lease = PacketPool<SupplierQueryResponse>.Rent();
        var response = lease.Value;
        response.TotalCount = result.Data!.totalCount;
        response.SequenceId = packet.SequenceId;
        response.Suppliers = result.Data.items.ConvertAll(s => MapToPacket(s, 0));
        try
        {
            await connection.TCP.SendAsync(response).ConfigureAwait(false);
        }
        finally
        {
            ReturnDtos(response.Suppliers);
        }
    }
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.SUPPLIER_CREATE)]
    public async ValueTask CreateAsync(IPacketContext<SupplierDto> context)
    {
        SupplierDto packet = context.Packet;
        IConnection connection = context.Connection;
        if (string.IsNullOrWhiteSpace(packet.Name))
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }
        var supplier = new Supplier
        {
            Name = packet.Name,
            Email = packet.Email,
            PhoneNumber = packet.PhoneNumber,
            Address = packet.Address,
            TaxCode = packet.TaxCode,
            ContactPerson = packet.ContactPerson,
            Notes = packet.Notes,
            IsActive = true
        };
        var result = await _supplierService.CreateAsync(supplier).ConfigureAwait(false);
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
    [PacketOpcode((ushort)OpCommand.SUPPLIER_UPDATE)]
    public async ValueTask UpdateAsync(IPacketContext<SupplierDto> context)
    {
        SupplierDto packet = context.Packet;
        IConnection connection = context.Connection;
        if (packet.SupplierId == null)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }
        var supplier = new Supplier
        {
            Id = packet.SupplierId.Value,
            Name = packet.Name,
            Email = packet.Email,
            PhoneNumber = packet.PhoneNumber,
            Address = packet.Address,
            TaxCode = packet.TaxCode,
            ContactPerson = packet.ContactPerson,
            Notes = packet.Notes,
            IsActive = packet.IsActive
        };
        var result = await _supplierService.UpdateAsync(supplier).ConfigureAwait(false);
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
    [PacketOpcode((ushort)OpCommand.SUPPLIER_DELETE)]
    public async ValueTask DeleteAsync(IPacketContext<SupplierDto> context)
    {
        SupplierDto packet = context.Packet;
        IConnection connection = context.Connection;
        if (packet.SupplierId == null)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }
        var result = await _supplierService.DeleteAsync(packet.SupplierId.Value).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }
        await context.OkAsync().ConfigureAwait(false);
    }
    private static SupplierDto MapToPacket(Supplier s, ushort sequenceId)
    {
        var dto = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Get<SupplierDto>();
        dto.SequenceId = sequenceId;
        dto.SupplierId = s.Id;
        dto.Name = s.Name;
        dto.Email = s.Email;
        dto.PhoneNumber = s.PhoneNumber;
        dto.Address = s.Address;
        dto.TaxCode = s.TaxCode;
        dto.ContactPerson = s.ContactPerson;
        dto.Notes = s.Notes;
        dto.IsActive = s.IsActive;
        return dto;
    }
    private static void ReturnDtos(IEnumerable<SupplierDto> dtos)
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
    private static void ReturnToPool(SupplierDto dto)
    {
        if (dto != null)
        {
            InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Return(dto);
        }
    }
}
