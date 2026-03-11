// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Models;
using AutoX.Gara.Infrastructure.Abstractions;
using AutoX.Gara.Shared.Packets.Customers;
using AutoX.Gara.Shared.Validation;
using Nalix.Common.Networking.Abstractions;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Attributes;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security.Enums;
using Nalix.Network.Connections;
using Nalix.Shared.Serialization;

namespace AutoX.Gara.Application.Customers;

/// <summary>
/// Packet controller xử lý tất cả nghiệp vụ CRUD cho Customer.
/// <para>
/// Thay đổi so với version cũ:
/// <list type="bullet">
///   <item>Inject <see cref="ICustomerRepository"/> thay vì <c>DataRepository&lt;Customer&gt;</c> trực tiếp
///         → tách Infrastructure khỏi Application layer (DDD).</item>
///   <item>Dùng <see cref="CustomerListQuery"/> value object thay vì truyền packet thẳng vào query.</item>
///   <item>Mapping tách thành private static helpers, không lặp code.</item>
/// </list>
/// </para>
/// </summary>
[PacketController]
public sealed class CustomerOps(ICustomerRepository customers)
{
    private readonly ICustomerRepository _customers = customers ?? throw new System.ArgumentNullException(nameof(customers));

    // ─── GET LIST ─────────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.CUSTOMER_GET)]
    public async System.Threading.Tasks.Task GetAsync(
        IPacket p,
        IConnection connection)
    {
        if (p is not CustomersQueryPacket packet)
        {
            System.UInt32 fallbackSeq = p is IPacketSequenced ps0 ? ps0.SequenceId : 0;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);

            return;
        }

        try
        {
            // Translate packet → domain value object
            // CustomerOps không còn biết về IQueryable hay EF Core
            CustomerListQuery query = new(
                Page: packet.Page,
                PageSize: packet.PageSize,
                SearchTerm: packet.SearchTerm,
                SortBy: packet.SortBy,
                SortDescending: packet.SortDescending,
                FilterType: packet.FilterType,
                FilterMembership: packet.FilterMembership);

            (System.Collections.Generic.List<Customer> items, System.Int32 totalCount) = await _customers.GetPageAsync(query).ConfigureAwait(false);

            CustomersPacket response = new()
            {
                TotalCount = totalCount,
                SequenceId = packet.SequenceId,
                Customers = items.ConvertAll(c => MapToPacket(c, sequenceId: 0))
            };

            System.Boolean sent = await connection.TCP.SendAsync(LiteSerializer.Serialize(response)).ConfigureAwait(false);

            if (!sent)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);

                return;
            }
        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.RETRY, packet.SequenceId).ConfigureAwait(false);
        }
    }

    // ─── CREATE ───────────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.CUSTOMER_CREATE)]
    public async System.Threading.Tasks.Task CreateAsync(
        IPacket p,
        IConnection connection)
    {
        if (!TryParseCustomerPacket(p, out CustomerDataPacket packet, out System.UInt32 fallbackSeq))
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);

            return;
        }

        if (packet!.DateOfBirth != default && packet.DateOfBirth > System.DateTime.UtcNow)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);

            return;
        }

        System.Boolean existed = await _customers.ExistsByContactAsync(packet.Email, packet.PhoneNumber).ConfigureAwait(false);

        if (existed)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.ALREADY_EXISTS,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);

            return;
        }

        try
        {
            System.DateTime now = System.DateTime.UtcNow;
            Customer newCustomer = new()
            {
                Name = packet.Name,
                Email = packet.Email,
                PhoneNumber = packet.PhoneNumber,
                Address = packet.Address,
                DateOfBirth = packet.DateOfBirth,
                TaxCode = packet.TaxCode,
                Type = packet.Type,
                Membership = packet.Membership,
                Gender = packet.Gender ?? Gender.None,
                Notes = packet.Notes ?? System.String.Empty,
                DeletedAt = null,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _customers.AddAsync(newCustomer).ConfigureAwait(false);
            await _customers.SaveChangesAsync().ConfigureAwait(false);

            CustomerDataPacket confirmed = MapToPacket(newCustomer, packet.SequenceId);
            System.Boolean sent = await connection.TCP.SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);

            if (!sent)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);

                return;
            }
        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
        }
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.CUSTOMER_UPDATE)]
    public async System.Threading.Tasks.Task UpdateAsync(
        IPacket p,
        IConnection connection)
    {
        if (!TryParseCustomerPacket(p, out CustomerDataPacket packet, out System.UInt32 fallbackSeq))
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);

            return;
        }

        if (packet!.DateOfBirth != default && packet.DateOfBirth > System.DateTime.UtcNow)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);

            return;
        }

        Customer existing = await _customers.GetByIdAsync(packet.CustomerId!.Value).ConfigureAwait(false);

        if (existing is null)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.NOT_FOUND,
                ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);

            return;
        }

        existing.Name = packet.Name;
        existing.Type = packet.Type;
        existing.Email = packet.Email;
        existing.TaxCode = packet.TaxCode;
        existing.Address = packet.Address;
        existing.Membership = packet.Membership;
        existing.PhoneNumber = packet.PhoneNumber;
        existing.DateOfBirth = packet.DateOfBirth;
        existing.Gender = packet.Gender ?? Gender.None;
        existing.Notes = packet.Notes ?? System.String.Empty;
        existing.UpdatedAt = System.DateTime.UtcNow;

        try
        {
            _customers.Update(existing);
            await _customers.SaveChangesAsync().ConfigureAwait(false);

            CustomerDataPacket confirmed = MapToPacket(existing, packet.SequenceId);
            System.Boolean sent = await connection.TCP.SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);

            if (!sent)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);

                return;
            }
        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
        }
    }

    // ─── DELETE (Soft) ────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((System.UInt16)OpCommand.CUSTOMER_DELETE)]
    public async System.Threading.Tasks.Task DeleteAsync(
        IPacket p,
        IConnection connection)
    {
        if (p is not CustomerDataPacket packet || packet.CustomerId == null)
        {
            System.UInt32 fallbackSeq = p is IPacketSequenced ps0 ? ps0.SequenceId : 0;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);

            return;
        }

        try
        {
            Customer existing = await _customers.GetByIdAsync(packet.CustomerId.Value).ConfigureAwait(false);

            if (existing is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);

                return;
            }

            System.DateTime now = System.DateTime.UtcNow;
            existing.DeletedAt = now;
            existing.UpdatedAt = now;

            _customers.Update(existing);
            await _customers.SaveChangesAsync().ConfigureAwait(false);

            await connection.SendAsync(
                ControlType.NONE,
                ProtocolReason.NONE,
                ProtocolAdvice.NONE, packet.SequenceId).ConfigureAwait(false);
        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
        }
    }

    // ─── Private Helpers ─────────────────────────────────────────────────────

    private static System.Boolean TryParseCustomerPacket(
        IPacket p,
        out CustomerDataPacket packet,
        out System.UInt32 fallbackSeqId)
    {
        fallbackSeqId = p is IPacketSequenced ps ? ps.SequenceId : 0;

        if (p is not CustomerDataPacket cp ||
            !AccountValidation.IsValidEmail(cp.Email) ||
            !AccountValidation.IsValidVietnamPhoneNumber(cp.PhoneNumber))
        {
            packet = null;
            return false;
        }

        packet = cp;
        return true;
    }

    private static CustomerDataPacket MapToPacket(Customer c, System.UInt32 sequenceId) => new()
    {
        SequenceId = sequenceId,
        CustomerId = c.Id,
        Type = c.Type,
        Name = c.Name,
        Email = c.Email,
        Gender = c.Gender,
        TaxCode = c.TaxCode,
        Address = c.Address,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
        Membership = c.Membership,
        PhoneNumber = c.PhoneNumber,
        DateOfBirth = c.DateOfBirth,
        Notes = c.Notes ?? System.String.Empty
    };
}