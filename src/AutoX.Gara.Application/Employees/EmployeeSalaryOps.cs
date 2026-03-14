// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Infrastructure.Repositories;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Protocol.Employees;
using Nalix.Common.Networking.Abstractions;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Attributes;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security.Enums;
using Nalix.Framework.Injection;
using Nalix.Network.Connections;
using Nalix.Shared.Memory.Pooling;
using Nalix.Shared.Serialization;

namespace AutoX.Gara.Application.Employees;

[PacketController]
public sealed class EmployeeSalaryOps(AutoXDbContextFactory dbContextFactory)
{
    private readonly AutoXDbContextFactory _dbContextFactory = dbContextFactory
        ?? throw new System.ArgumentNullException(nameof(dbContextFactory));

    // ─── GET LIST ─────────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.EMPLOYEE_SALARY_GET)]
    public async System.Threading.Tasks.Task GetAsync(IPacket p, IConnection connection)
    {
        if (p is not EmployeeSalaryQueryRequest packet)
        {
            System.UInt32 fallbackSeq = p is IPacketSequenced ps ? ps.SequenceId : 0;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);
            return;
        }

        EmployeeSalaryQueryResponse response = null;

        try
        {
            EmployeeSalaryListQuery query = new(
                Page: packet.Page,
                PageSize: packet.PageSize,
                SearchTerm: packet.SearchTerm,
                SortBy: packet.SortBy,
                SortDescending: packet.SortDescending,
                FilterEmployeeId: packet.FilterEmployeeId > 0 ? packet.FilterEmployeeId : null,
                FilterSalaryType: packet.FilterSalaryType,
                FilterFromDate: packet.FilterFromDate,
                FilterToDate: packet.FilterToDate);

            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var repo = new EmployeeSalaryRepository(db);

            (System.Collections.Generic.List<EmployeeSalary> items, System.Int32 totalCount)
                = await repo.GetPageAsync(query).ConfigureAwait(false);

            response = new()
            {
                TotalCount = totalCount,
                SequenceId = packet.SequenceId,
                Salaries = items.ConvertAll(es => MapToPacket(es, sequenceId: 0))
            };

            System.Boolean sent = await connection.TCP
                .SendAsync(LiteSerializer.Serialize(response)).ConfigureAwait(false);

            if (!sent)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
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
            if (response is not null)
            {
                var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
                foreach (EmployeeSalaryDto dto in response.Salaries)
                {
                    pool.Return(dto);
                }
            }
        }
    }

    // ─── CREATE ───────────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((System.UInt16)OpCommand.EMPLOYEE_SALARY_CREATE)]
    public async System.Threading.Tasks.Task CreateAsync(IPacket p, IConnection connection)
    {
        if (!TryParseSalaryPacket(p, out EmployeeSalaryDto packet, out System.UInt32 fallbackSeq))
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);
            return;
        }

        EmployeeSalaryDto confirmed = null;
        try
        {
            if (packet!.EmployeeId <= 0 || packet.Salary < 0 || packet.SalaryUnit < 0)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.VALIDATION_FAILED,
                    ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);
                return;
            }

            if (packet.EffectiveTo.HasValue && packet.EffectiveTo.Value < packet.EffectiveFrom)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.VALIDATION_FAILED,
                    ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);
                return;
            }

            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var repo = new EmployeeSalaryRepository(db);

            EmployeeSalary entity = new()
            {
                EmployeeId = packet.EmployeeId,
                Salary = packet.Salary,
                SalaryType = packet.SalaryType,
                SalaryUnit = packet.SalaryUnit,
                EffectiveFrom = packet.EffectiveFrom,
                EffectiveTo = packet.EffectiveTo,
                Note = packet.Note ?? System.String.Empty
            };

            await repo.AddAsync(entity).ConfigureAwait(false);
            await repo.SaveChangesAsync().ConfigureAwait(false);

            confirmed = MapToPacket(entity, packet.SequenceId);
            confirmed.OpCode = (System.UInt16)OpCommand.EMPLOYEE_SALARY_CREATE;

            System.Boolean sent = await connection.TCP
                .SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);

            if (!sent)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
            }
        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, packet!.SequenceId).ConfigureAwait(false);
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
    [PacketOpcode((System.UInt16)OpCommand.EMPLOYEE_SALARY_UPDATE)]
    public async System.Threading.Tasks.Task UpdateAsync(IPacket p, IConnection connection)
    {
        if (!TryParseSalaryPacket(p, out EmployeeSalaryDto packet, out System.UInt32 fallbackSeq)
            || packet!.EmployeeSalaryId is null)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);
            return;
        }

        EmployeeSalaryDto confirmed = null;
        try
        {
            if (packet.EmployeeId <= 0 || packet.Salary < 0 || packet.SalaryUnit < 0)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.VALIDATION_FAILED,
                    ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);
                return;
            }

            if (packet.EffectiveTo.HasValue && packet.EffectiveTo.Value < packet.EffectiveFrom)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.VALIDATION_FAILED,
                    ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);
                return;
            }

            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var repo = new EmployeeSalaryRepository(db);

            EmployeeSalary existing = await repo
                .GetByIdAsync(packet.EmployeeSalaryId.Value).ConfigureAwait(false);

            if (existing is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
                return;
            }

            existing.EmployeeId = packet.EmployeeId;
            existing.Salary = packet.Salary;
            existing.SalaryType = packet.SalaryType;
            existing.SalaryUnit = packet.SalaryUnit;
            existing.EffectiveFrom = packet.EffectiveFrom;
            existing.EffectiveTo = packet.EffectiveTo;
            existing.Note = packet.Note ?? System.String.Empty;

            repo.Update(existing);
            await repo.SaveChangesAsync().ConfigureAwait(false);

            confirmed = MapToPacket(existing, packet.SequenceId);
            confirmed.OpCode = (System.UInt16)OpCommand.EMPLOYEE_SALARY_UPDATE;

            System.Boolean sent = await connection.TCP
                .SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);

            if (!sent)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
            }
        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, packet!.SequenceId).ConfigureAwait(false);
        }
        finally
        {
            if (confirmed is not null)
            {
                InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Return(confirmed);
            }
        }
    }

    // ─── DELETE ───────────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((System.UInt16)OpCommand.EMPLOYEE_SALARY_DELETE)]
    public async System.Threading.Tasks.Task DeleteAsync(IPacket p, IConnection connection)
    {
        if (p is not EmployeeSalaryDto packet
            || packet.EmployeeSalaryId is null)
        {
            System.UInt32 fallbackSeq = p is IPacketSequenced ps ? ps.SequenceId : 0;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);
            return;
        }

        try
        {
            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var repo = new EmployeeSalaryRepository(db);

            EmployeeSalary existing = await repo
                .GetByIdAsync(packet.EmployeeSalaryId.Value).ConfigureAwait(false);

            if (existing is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
                return;
            }

            repo.Remove(existing);
            await repo.SaveChangesAsync().ConfigureAwait(false);

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

    // ─── Private Helpers ──────────────────────────────────────────────────────

    private static System.Boolean TryParseSalaryPacket(
        IPacket p,
        out EmployeeSalaryDto packet,
        out System.UInt32 fallbackSeqId)
    {
        fallbackSeqId = p is IPacketSequenced ps ? ps.SequenceId : 0;

        if (p is not EmployeeSalaryDto dto)
        {
            packet = null;
            return false;
        }

        packet = dto;
        return true;
    }

    private static EmployeeSalaryDto MapToPacket(EmployeeSalary e, System.UInt32 sequenceId)
    {
        EmployeeSalaryDto dto = InstanceManager.Instance
            .GetOrCreateInstance<ObjectPoolManager>()
            .Get<EmployeeSalaryDto>();

        dto.SequenceId = sequenceId;
        dto.EmployeeSalaryId = e.Id;
        dto.EmployeeId = e.EmployeeId;
        dto.Salary = e.Salary;
        dto.SalaryType = e.SalaryType;
        dto.SalaryUnit = e.SalaryUnit;
        dto.EffectiveFrom = e.EffectiveFrom;
        dto.EffectiveTo = e.EffectiveTo;
        dto.Note = e.Note ?? System.String.Empty;

        return dto;
    }
}

