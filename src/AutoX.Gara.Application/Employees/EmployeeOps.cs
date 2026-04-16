// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Protocol.Employees;
using Microsoft.Extensions.Logging;
using Nalix.Common.Networking;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security;
using Nalix.Framework.Injection;
using Nalix.Framework.Memory.Objects;
using Nalix.Framework.Serialization;
using Nalix.Runtime.Extensions;
using System.Diagnostics;

namespace AutoX.Gara.Application.Employees;

/// <summary>
/// Packet controller xử lý tất cả nghiệp vụ CRUD cho Employee.
/// </summary>
[PacketController]
public sealed class EmployeeOps(IDataSessionFactory dataSessionFactory)
{
    private readonly IDataSessionFactory _dataSessionFactory = dataSessionFactory
        ?? throw new System.ArgumentNullException(nameof(dataSessionFactory));

    // ─── GET LIST ─────────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.EMPLOYEE_GET)]
    public async System.Threading.Tasks.Task GetAsync(IPacket p, IConnection connection)
    {
        if (p is not EmployeeQueryRequest packet)
        {
            System.UInt32 fallbackSeq = p.SequenceId;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, fallbackSeq, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        ILogger logger = InstanceManager.Instance.GetOrCreateInstance<ILogger>();
        Stopwatch sw = Stopwatch.StartNew();
        EmployeeQueryResponse response = null;

        try
        {
            EmployeeListQuery query = BuildListQuery(packet);
            await using var session = _dataSessionFactory.Create();
            var employees = session.Employees;

            (System.Collections.Generic.List<Employee> items, System.Int32 totalCount)
                = await employees.GetPageAsync(query).ConfigureAwait(false);

            var payload = items.ConvertAll(e => MapToPacket(e, sequenceId: packet.SequenceId));
            response = new()
            {
                TotalCount = totalCount,
                SequenceId = packet.SequenceId,
                Employees = payload
            };

            System.Byte[] bytes;
            try
            {
                bytes = LiteSerializer.Serialize(response);
            }
            catch (System.Exception ex)
            {
                logger?.Error(
                    $"[APP.{nameof(EmployeeOps)}:{nameof(GetAsync)}] serialization-failed seq={packet.SequenceId} ms={sw.ElapsedMilliseconds}\n{ex}");
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);

                return;
            }
            await connection.TCP.SendAsync(bytes).ConfigureAwait(false);

            logger?.Info(
                $"[APP.{nameof(EmployeeOps)}:{nameof(GetAsync)}] ok seq={packet.SequenceId} count={payload.Count} total={totalCount} ms={sw.ElapsedMilliseconds}");
        }
        catch (System.Exception ex)
        {
            logger?.Error(
                $"[APP.{nameof(EmployeeOps)}:{nameof(GetAsync)}] failed seq={packet.SequenceId} ms={sw.ElapsedMilliseconds}\n{ex}");
            await SendErrorAsync(connection, ProtocolReason.INTERNAL_ERROR, ProtocolAdvice.RETRY, logger, nameof(GetAsync), packet.SequenceId).ConfigureAwait(false);
        }
        finally
        {
            ReturnDtos(response?.Employees);
        }
    }

    // ─── CREATE ───────────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((System.UInt16)OpCommand.EMPLOYEE_CREATE)]
    public async System.Threading.Tasks.Task CreateAsync(IPacket p, IConnection connection)
    {
        if (!TryParseEmployeePacket(p, out EmployeeDto packet, out System.UInt32 fallbackSeq))
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, fallbackSeq, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        // Validate dates
        if (packet!.DateOfBirth.HasValue && packet.DateOfBirth > System.DateTime.UtcNow)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        if (packet.StartDate.HasValue && packet.EndDate.HasValue && packet.EndDate < packet.StartDate)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        EmployeeDto confirmed = null;

        try
        {
            await using var session = _dataSessionFactory.Create();
            var employees = session.Employees;

            System.Boolean emailExists = await employees
                .ExistsByEmailAsync(packet.Email).ConfigureAwait(false);

            if (emailExists)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.ALREADY_EXISTS,
                    ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
                return;
            }

            if (!System.String.IsNullOrWhiteSpace(packet.PhoneNumber))
            {
                System.Boolean phoneExists = await employees
                    .ExistsByPhoneAsync(packet.PhoneNumber).ConfigureAwait(false);

                if (phoneExists)
                {
                    await connection.SendAsync(
                        ControlType.ERROR,
                        ProtocolReason.ALREADY_EXISTS,
                        ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
                    return;
                }
            }

            Employee newEmployee = new()
            {
                Name = packet.Name,
                Email = packet.Email,
                Address = packet.Address ?? System.String.Empty,
                PhoneNumber = packet.PhoneNumber ?? System.String.Empty,
                Gender = packet.Gender ?? Gender.None,
                Position = packet.Position ?? Position.None,
                Status = packet.Status ?? EmploymentStatus.None,
                DateOfBirth = packet.DateOfBirth,
                StartDate = packet.StartDate ?? System.DateTime.UtcNow,
                EndDate = packet.EndDate
            };

            newEmployee.UpdateStatus();

            await employees.AddAsync(newEmployee).ConfigureAwait(false);
            await employees.SaveChangesAsync().ConfigureAwait(false);

            confirmed = MapToPacket(newEmployee, packet.SequenceId);
            await connection.TCP
                .SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);

        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
        finally
        {
            if (confirmed is not null)
            {
                InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Return(confirmed);
            }
        }
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((System.UInt16)OpCommand.EMPLOYEE_UPDATE)]
    public async System.Threading.Tasks.Task UpdateAsync(IPacket p, IConnection connection)
    {
        if (!TryParseEmployeePacket(p, out EmployeeDto packet, out System.UInt32 fallbackSeq))
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, fallbackSeq, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        if (packet!.EmployeeId is null)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        if (packet.StartDate.HasValue && packet.EndDate.HasValue && packet.EndDate < packet.StartDate)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        EmployeeDto confirmed = null;

        try
        {
            await using var session = _dataSessionFactory.Create();
            var employees = session.Employees;

            Employee existing = await employees
                .GetByIdAsync(packet.EmployeeId.Value).ConfigureAwait(false);

            if (existing is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
                return;
            }

            existing.Name = packet.Name;
            existing.Email = packet.Email;
            existing.Address = packet.Address ?? System.String.Empty;
            existing.PhoneNumber = packet.PhoneNumber ?? System.String.Empty;

            if (packet.Gender.HasValue)
            {
                existing.Gender = packet.Gender.Value;
            }

            if (packet.Position.HasValue)
            {
                existing.Position = packet.Position.Value;
            }

            if (packet.Status.HasValue)
            {
                existing.Status = packet.Status.Value;
            }

            if (packet.DateOfBirth.HasValue)
            {
                existing.DateOfBirth = packet.DateOfBirth;
            }

            if (packet.StartDate.HasValue)
            {
                existing.StartDate = packet.StartDate.Value;
            }

            existing.EndDate = packet.EndDate;
            existing.UpdateStatus();

            employees.Update(existing);
            await employees.SaveChangesAsync().ConfigureAwait(false);

            confirmed = MapToPacket(existing, packet.SequenceId);
            await connection.TCP
                .SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);

        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
        finally
        {
            if (confirmed is not null)
            {
                InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Return(confirmed);
            }
        }
    }

    // ─── CHANGE STATUS ────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((System.UInt16)OpCommand.EMPLOYEE_CHANGE_STATUS)]
    public async System.Threading.Tasks.Task ChangeStatusAsync(IPacket p, IConnection connection)
    {
        if (p is not EmployeeDto packet
            || packet.EmployeeId is null
            || packet.Status is null)
        {
            System.UInt32 fallbackSeq = p.SequenceId;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, fallbackSeq, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        try
        {
            await using var session = _dataSessionFactory.Create();
            var employees = session.Employees;

            Employee existing = await employees
                .GetByIdAsync(packet.EmployeeId.Value).ConfigureAwait(false);

            if (existing is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
                return;
            }

            existing.Status = packet.Status.Value;

            employees.Update(existing);
            await employees.SaveChangesAsync().ConfigureAwait(false);

            await connection.SendAsync(
                ControlType.NONE,
                ProtocolReason.NONE,
                ProtocolAdvice.NONE, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
    }

    // ─── Private Helpers ──────────────────────────────────────────────────────

    private static System.Boolean TryParseEmployeePacket(
        IPacket p,
        out EmployeeDto packet,
        out System.UInt32 fallbackSeqId)
    {
        fallbackSeqId = p.SequenceId;

        if (p is not EmployeeDto ep
            || System.String.IsNullOrWhiteSpace(ep.Name)
            || System.String.IsNullOrWhiteSpace(ep.Email))
        {
            packet = null;
            return false;
        }

        packet = ep;
        return true;
    }

    private static EmployeeDto MapToPacket(Employee e, System.UInt32 sequenceId)
    {
        EmployeeDto data = InstanceManager.Instance
            .GetOrCreateInstance<ObjectPoolManager>()
            .Get<EmployeeDto>();

        data.SequenceId = sequenceId;
        data.EmployeeId = e.Id;
        data.Name = e.Name;
        data.Email = e.Email;
        data.Address = e.Address ?? System.String.Empty;
        data.PhoneNumber = e.PhoneNumber ?? System.String.Empty;
        data.Gender = e.Gender;
        data.Position = e.Position;
        data.Status = e.Status;
        data.DateOfBirth = e.DateOfBirth;
        data.StartDate = e.StartDate;
        data.EndDate = e.EndDate;

        return data;
    }

    private static EmployeeListQuery BuildListQuery(EmployeeQueryRequest request)
        => new(
            Page: request.Page,
            PageSize: request.PageSize,
            SearchTerm: request.SearchTerm,
            SortBy: request.SortBy,
            SortDescending: request.SortDescending,
            FilterPosition: request.FilterPosition,
            FilterStatus: request.FilterStatus,
            FilterGender: request.FilterGender);

    private static void ReturnDtos(System.Collections.Generic.IEnumerable<EmployeeDto> dtos)
    {
        if (dtos is null)
        {
            return;
        }

        var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
        foreach (EmployeeDto dto in dtos)
        {
            pool.Return(dto);
        }
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
            $"[APP.{nameof(EmployeeOps)}:{operation}] reason={reason} seq={sequenceId}");
        await connection.SendAsync(
            ControlType.ERROR,
            reason,
            advice, new ControlDirectiveOptions(ControlFlags.NONE, sequenceId, 0u, 0u, 0)).ConfigureAwait(false);
    }
}








