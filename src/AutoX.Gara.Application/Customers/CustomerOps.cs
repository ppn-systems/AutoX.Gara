// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Infrastructure.Abstractions;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Infrastructure.Repositories;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Protocol.Customers;
using AutoX.Gara.Shared.Validation;
using Nalix.Common.Networking.Abstractions;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Attributes;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security.Enums;
using Nalix.Framework.Injection;
using Nalix.Network.Connections;
using Nalix.Shared.Memory.Pooling;
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
public sealed class CustomerOps(AutoXDbContextFactory dbContextFactory)
{
    private readonly AutoXDbContextFactory _dbContextFactory = dbContextFactory
        ?? throw new System.ArgumentNullException(nameof(dbContextFactory));

    // ─── GET LIST ─────────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.CUSTOMER_GET)]
    public async System.Threading.Tasks.Task GetAsync(
        IPacket p,
        IConnection connection)
    {
        if (p is not CustomerQueryRequest packet)
        {
            System.UInt32 fallbackSeq = p is IPacketSequenced ps0 ? ps0.SequenceId : 0;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);

            return;
        }

        CustomerQueryResponse response = null;

        try
        {
            CustomerListQuery query = new(
                Page: packet.Page,
                PageSize: packet.PageSize,
                SearchTerm: packet.SearchTerm,
                SortBy: packet.SortBy,
                SortDescending: packet.SortDescending,
                FilterType: packet.FilterType,
                FilterMembership: packet.FilterMembership);

            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var customers = new CustomerRepository(db);

            (System.Collections.Generic.List<Customer> items, System.Int32 totalCount) =
                await customers.GetPageAsync(query).ConfigureAwait(false);

            response = new()
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
        finally
        {
            if (response?.Customers != null)
            {
                foreach (CustomerDto cdp in response.Customers)
                {
                    InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Return(cdp);
                }
            }
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
        if (!TryParseCustomerPacket(p, out CustomerDto packet, out System.UInt32 fallbackSeq))
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

        CustomerDto confirmed = null;
        try
        {
            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var customers = new CustomerRepository(db);

            System.Boolean existed = await customers.ExistsByContactAsync(packet.Email, packet.PhoneNumber).ConfigureAwait(false);

            if (existed)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.ALREADY_EXISTS,
                    ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);

                return;
            }

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

            await customers.AddAsync(newCustomer).ConfigureAwait(false);
            await customers.SaveChangesAsync().ConfigureAwait(false);

            confirmed = MapToPacket(newCustomer, packet.SequenceId);
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
        finally
        {
            if (confirmed != null)
            {
                InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                    .Return(confirmed);
            }
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
        if (!TryParseCustomerPacket(p, out CustomerDto packet, out System.UInt32 fallbackSeq))
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

        CustomerDto confirmed = null;
        try
        {
            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var customers = new CustomerRepository(db);

            var existing = await customers.GetByIdAsync(packet.CustomerId!.Value).ConfigureAwait(false);

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

            customers.Update(existing);
            await customers.SaveChangesAsync().ConfigureAwait(false);

            confirmed = MapToPacket(existing, packet.SequenceId);
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
        finally
        {
            if (confirmed != null)
            {
                InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                    .Return(confirmed);
            }
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
        if (p is not CustomerDto packet || packet.CustomerId == null)
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
            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var customers = new CustomerRepository(db);

            var existing = await customers.GetByIdAsync(packet.CustomerId.Value).ConfigureAwait(false);

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

            customers.Update(existing);
            await customers.SaveChangesAsync().ConfigureAwait(false);

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
        out CustomerDto packet,
        out System.UInt32 fallbackSeqId)
    {
        fallbackSeqId = p is IPacketSequenced ps ? ps.SequenceId : 0;

        if (p is not CustomerDto cp ||
            !AccountValidation.IsValidEmail(cp.Email) ||
            !AccountValidation.IsValidVietnamPhoneNumber(cp.PhoneNumber))
        {
            packet = null;
            return false;
        }

        packet = cp;
        return true;
    }

    private static CustomerDto MapToPacket(Customer c, System.UInt32 sequenceId)
    {
        CustomerDto data = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                                          .Get<CustomerDto>();

        data.Type = c.Type;
        data.Name = c.Name;
        data.Email = c.Email;
        data.Gender = c.Gender;
        data.CustomerId = c.Id;
        data.TaxCode = c.TaxCode;
        data.Address = c.Address;
        data.CreatedAt = c.CreatedAt;
        data.UpdatedAt = c.UpdatedAt;
        data.SequenceId = sequenceId;
        data.Membership = c.Membership;
        data.PhoneNumber = c.PhoneNumber;
        data.DateOfBirth = c.DateOfBirth;
        data.Notes = c.Notes ?? System.String.Empty;

        return data;
    }
}