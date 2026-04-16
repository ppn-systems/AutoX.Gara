// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Infrastructure.Abstractions.Repositories;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Infrastructure.Repositories;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Protocol.Customers;
using AutoX.Gara.Shared.Validation;
using Microsoft.Extensions.Logging;

using Nalix.Common.Networking;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security;
using Nalix.Framework.Injection;
using Nalix.Network.Connections;
using Nalix.Framework.Memory.Objects;
using Nalix.Framework.Serialization;
using System.Diagnostics;

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

        ILogger logger = InstanceManager.Instance.GetOrCreateInstance<ILogger>();
        Stopwatch sw = Stopwatch.StartNew();
        CustomerQueryResponse response = null;

        try
        {
            CustomerListQuery query = BuildCustomerListQuery(packet);

            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var customers = new CustomerRepository(db);
            (System.Collections.Generic.List<Customer> items, System.Int32 totalCount) =
                await customers.GetPageAsync(query).ConfigureAwait(false);

            System.Collections.Generic.List<CustomerDto> payload = items.ConvertAll(c => MapToPacket(c, sequenceId: 0));
            response = new()
            {
                TotalCount = totalCount,
                SequenceId = packet.SequenceId,
                Customers = payload
            };

            System.Byte[] bytes;
            try
            {
                bytes = LiteSerializer.Serialize(response);
            }
            catch (System.Exception ex)
            {
                logger?.Error(
                    $"[APP.{nameof(CustomerOps)}:{nameof(GetAsync)}] serialization-failed seq={packet.SequenceId} page={packet.Page} size={packet.PageSize} ms={sw.ElapsedMilliseconds}\n{ex}");
                await SendErrorAsync(connection, ProtocolReason.INTERNAL_ERROR, ProtocolAdvice.DO_NOT_RETRY, logger, nameof(GetAsync), packet.SequenceId).ConfigureAwait(false);
                return;
            }
            System.Boolean sent = await connection.TCP.SendAsync(bytes).ConfigureAwait(false);

            if (!sent)
            {
                logger?.Warn($"[APP.{nameof(CustomerOps)}:{nameof(GetAsync)}] send-failed seq={packet.SequenceId}");

                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
            }

            logger?.Info(
                $"[APP.{nameof(CustomerOps)}:{nameof(GetAsync)}] ok seq={packet.SequenceId} len={bytes.Length} page={packet.Page} size={packet.PageSize} total={totalCount} returned={payload.Count} ms={sw.ElapsedMilliseconds}");
        }
        catch (System.Exception ex)
        {
            logger?.Error(
                $"[APP.{nameof(CustomerOps)}:{nameof(GetAsync)}] failed seq={packet.SequenceId} page={packet.Page} size={packet.PageSize} ms={sw.ElapsedMilliseconds}\n{ex}");
            await SendErrorAsync(connection, ProtocolReason.INTERNAL_ERROR, ProtocolAdvice.RETRY, logger, nameof(GetAsync), packet.SequenceId).ConfigureAwait(false);
        }
        finally
        {
            ReturnDtos(response?.Customers);
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

        ILogger logger = InstanceManager.Instance.GetOrCreateInstance<ILogger>();
        Stopwatch sw = Stopwatch.StartNew();

        if (!ValidateDateOfBirth(packet))
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);

            logger?.Warn($"[APP.{nameof(CustomerOps)}:{nameof(CreateAsync)}] invalid-dob seq={packet.SequenceId}");
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
                logger?.Warn($"[APP.{nameof(CustomerOps)}:{nameof(CreateAsync)}] duplicate-contact seq={packet.SequenceId} email={packet.Email} phone={packet.PhoneNumber}");
                await SendErrorAsync(connection, ProtocolReason.ALREADY_EXISTS, ProtocolAdvice.FIX_AND_RETRY, logger, nameof(CreateAsync), packet.SequenceId).ConfigureAwait(false);
                return;
            }

            Customer newCustomer = BuildCustomer(packet, System.DateTime.UtcNow);
            await customers.AddAsync(newCustomer).ConfigureAwait(false);
            await customers.SaveChangesAsync().ConfigureAwait(false);

            confirmed = MapToPacket(newCustomer, packet.SequenceId);

            System.Byte[] bytes = LiteSerializer.Serialize(confirmed);
            System.Boolean sent = await connection.TCP.SendAsync(bytes).ConfigureAwait(false);

            if (!sent)
            {
                logger?.Warn($"[APP.{nameof(CustomerOps)}:{nameof(GetAsync)}] send-failed seq={packet.SequenceId}");

                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
            }

            logger?.Info(
                $"[APP.{nameof(CustomerOps)}:{nameof(CreateAsync)}] ok seq={packet.SequenceId} id={newCustomer.Id} len={bytes.Length} ms={sw.ElapsedMilliseconds}");
        }
        catch (System.Exception ex)
        {
            logger?.Error(
                $"[APP.{nameof(CustomerOps)}:{nameof(CreateAsync)}] failed seq={packet.SequenceId} ms={sw.ElapsedMilliseconds}\n{ex}");
            await SendErrorAsync(connection, ProtocolReason.INTERNAL_ERROR, ProtocolAdvice.DO_NOT_RETRY, logger, nameof(CreateAsync), packet.SequenceId).ConfigureAwait(false);
        }
        finally
        {
            ReturnToPool(confirmed);
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

        ILogger logger = InstanceManager.Instance.GetOrCreateInstance<ILogger>();
        Stopwatch sw = Stopwatch.StartNew();

        if (!ValidateDateOfBirth(packet))
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);

            logger?.Warn($"[APP.{nameof(CustomerOps)}:{nameof(UpdateAsync)}] invalid-dob seq={packet.SequenceId}");
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
                await SendErrorAsync(connection, ProtocolReason.NOT_FOUND, ProtocolAdvice.DO_NOT_RETRY, logger, nameof(UpdateAsync), packet.SequenceId).ConfigureAwait(false);
                return;
            }

            ApplyPacket(existing, packet);
            customers.Update(existing);
            await customers.SaveChangesAsync().ConfigureAwait(false);

            confirmed = MapToPacket(existing, packet.SequenceId);

            System.Byte[] bytes = LiteSerializer.Serialize(confirmed);
            System.Boolean sent = await connection.TCP.SendAsync(bytes).ConfigureAwait(false);

            if (!sent)
            {
                logger?.Warn($"[APP.{nameof(CustomerOps)}:{nameof(GetAsync)}] send-failed seq={packet.SequenceId}");

                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
            }

            logger?.Info(
                $"[APP.{nameof(CustomerOps)}:{nameof(UpdateAsync)}] ok seq={packet.SequenceId} id={existing.Id} len={bytes.Length} ms={sw.ElapsedMilliseconds}");
        }
        catch (System.Exception ex)
        {
            logger?.Error(
                $"[APP.{nameof(CustomerOps)}:{nameof(UpdateAsync)}] failed seq={packet.SequenceId} ms={sw.ElapsedMilliseconds}\n{ex}");
            await SendErrorAsync(connection, ProtocolReason.INTERNAL_ERROR, ProtocolAdvice.DO_NOT_RETRY, logger, nameof(UpdateAsync), packet.SequenceId).ConfigureAwait(false);
        }
        finally
        {
            ReturnToPool(confirmed);
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

        ILogger logger = InstanceManager.Instance.GetOrCreateInstance<ILogger>();
        Stopwatch sw = Stopwatch.StartNew();

        try
        {
            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var customers = new CustomerRepository(db);

            var existing = await customers.GetByIdAsync(packet.CustomerId.Value).ConfigureAwait(false);

            if (existing is null)
            {
                await SendErrorAsync(connection, ProtocolReason.NOT_FOUND, ProtocolAdvice.DO_NOT_RETRY, logger, nameof(DeleteAsync), packet.SequenceId).ConfigureAwait(false);
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

            logger?.Info(
                $"[APP.{nameof(CustomerOps)}:{nameof(DeleteAsync)}] ok seq={packet.SequenceId} id={existing.Id} ms={sw.ElapsedMilliseconds}");
        }
        catch (System.Exception ex)
        {
            logger?.Error(
                $"[APP.{nameof(CustomerOps)}:{nameof(DeleteAsync)}] failed seq={packet.SequenceId} ms={sw.ElapsedMilliseconds}\n{ex}");
            await SendErrorAsync(connection, ProtocolReason.INTERNAL_ERROR, ProtocolAdvice.DO_NOT_RETRY, logger, nameof(DeleteAsync), packet.SequenceId).ConfigureAwait(false);
        }
    }

    // ─── Private Helpers ─────────────────────────────────────────────────────

    private static CustomerListQuery BuildCustomerListQuery(CustomerQueryRequest request)
        => new(
            Page: request.Page,
            PageSize: request.PageSize,
            SearchTerm: request.SearchTerm,
            SortBy: request.SortBy,
            SortDescending: request.SortDescending,
            FilterType: request.FilterType,
            FilterMembership: request.FilterMembership);

    private static System.Boolean ValidateDateOfBirth(CustomerDto packet)
        => packet.DateOfBirth == default || packet.DateOfBirth <= System.DateTime.UtcNow;

    private static Customer BuildCustomer(CustomerDto packet, System.DateTime now)
        => new()
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

    private static void ApplyPacket(Customer existing, CustomerDto packet)
    {
        existing.Name = packet.Name;
        existing.Email = packet.Email;
        existing.PhoneNumber = packet.PhoneNumber;
        existing.Address = packet.Address;
        existing.DateOfBirth = packet.DateOfBirth;
        existing.TaxCode = packet.TaxCode;
        existing.Type = packet.Type;
        existing.Membership = packet.Membership;
        existing.Gender = packet.Gender ?? Gender.None;
        existing.Notes = packet.Notes ?? System.String.Empty;
        existing.UpdatedAt = System.DateTime.UtcNow;
    }

    private static void ReturnDtos(System.Collections.Generic.IEnumerable<CustomerDto> dtos)
    {
        if (dtos is null)
        {
            return;
        }

        var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
        foreach (CustomerDto dto in dtos)
        {
            pool.Return(dto);
        }
    }

    private static void ReturnToPool(CustomerDto dto)
    {
        if (dto is null)
        {
            return;
        }

        InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Return(dto);
    }

    private static async System.Threading.Tasks.Task SendErrorAsync(
        IConnection connection,
        ProtocolReason reason,
        ProtocolAdvice advice,
        ILogger logger,
        System.String operation,
        System.UInt32 sequenceId)
    {
        logger?.Warn(
            $"[APP.{nameof(CustomerOps)}:{operation}] reason={reason} seq={sequenceId}");
        await connection.SendAsync(
            ControlType.ERROR,
            reason,
            advice, sequenceId).ConfigureAwait(false);
    }

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


