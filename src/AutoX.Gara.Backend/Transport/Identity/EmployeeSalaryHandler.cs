using AutoX.Gara.Application.Employees;
using AutoX.Gara.Backend.Transport.Common;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Protocol.Employees;
using Nalix.Common.Networking;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security;
using Nalix.Framework.DataFrames.Pooling;
using Nalix.Framework.Injection;
using Nalix.Framework.Memory.Objects;
using System;
using System.Threading.Tasks;


namespace AutoX.Gara.Backend.Transport.Identity;

/// <summary>
/// Packet Handler for employee salary related operations.
/// </summary>
[PacketController]
public sealed class EmployeeSalaryHandler(EmployeeSalaryAppService salaryService)
{
    private readonly EmployeeSalaryAppService _salaryService = salaryService ?? throw new ArgumentNullException(nameof(salaryService));

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.EMPLOYEE_SALARY_GET)]
    public async ValueTask GetAsync(IPacketContext<EmployeeSalaryQueryRequest> context)
    {
        EmployeeSalaryQueryRequest packet = context.Packet;
        IConnection connection = context.Connection;

        var query = new EmployeeSalaryListQuery(packet.Page, packet.PageSize, packet.SearchTerm, packet.SortBy, packet.SortDescending,
            packet.FilterEmployeeId > 0 ? packet.FilterEmployeeId : null, packet.FilterSalaryType, packet.FilterFromDate, packet.FilterToDate);

        var result = await _salaryService.GetPageAsync(query).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        using var lease = PacketPool<EmployeeSalaryQueryResponse>.Rent();
        var response = lease.Value;
        response.TotalCount = result.Data!.totalCount;
        response.SequenceId = packet.SequenceId;
        response.Salaries = result.Data.items.ConvertAll(es => MapToPacket(es, packet.SequenceId));

        try
        {
            await connection.TCP.SendAsync(response).ConfigureAwait(false);

        }
        finally
        {
            var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
            foreach (var dto in response.Salaries)
            {
                pool.Return(dto);
            }
        }
    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((ushort)OpCommand.EMPLOYEE_SALARY_CREATE)]
    public async ValueTask CreateAsync(IPacketContext<EmployeeSalaryDto> context)
    {
        EmployeeSalaryDto packet = context.Packet;
        IConnection connection = context.Connection;

        if (packet.EmployeeId <= 0)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }

        var salary = new EmployeeSalary
        {
            EmployeeId = packet.EmployeeId,
            Salary = packet.Salary,
            SalaryType = packet.SalaryType,
            SalaryUnit = packet.SalaryUnit,
            EffectiveFrom = packet.EffectiveFrom,
            EffectiveTo = packet.EffectiveTo,
            Note = packet.Note ?? string.Empty
        };

        var result = await _salaryService.CreateAsync(salary).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        var confirmed = MapToPacket(result.Data!, packet.SequenceId);
        confirmed.OpCode = (ushort)OpCommand.EMPLOYEE_SALARY_CREATE;
        try
        {
            await connection.TCP.SendAsync(confirmed).ConfigureAwait(false);

        }
        finally
        {
            InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Return(confirmed);
        }
    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((ushort)OpCommand.EMPLOYEE_SALARY_UPDATE)]
    public async ValueTask UpdateAsync(IPacketContext<EmployeeSalaryDto> context)
    {
        EmployeeSalaryDto packet = context.Packet;
        IConnection connection = context.Connection;

        if (packet.EmployeeSalaryId == null)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }

        var salary = new EmployeeSalary
        {
            Id = packet.EmployeeSalaryId.Value,
            EmployeeId = packet.EmployeeId,
            Salary = packet.Salary,
            SalaryType = packet.SalaryType,
            SalaryUnit = packet.SalaryUnit,
            EffectiveFrom = packet.EffectiveFrom,
            EffectiveTo = packet.EffectiveTo,
            Note = packet.Note ?? string.Empty
        };

        var result = await _salaryService.UpdateAsync(salary).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        var confirmed = MapToPacket(result.Data!, packet.SequenceId);
        confirmed.OpCode = (ushort)OpCommand.EMPLOYEE_SALARY_UPDATE;
        try
        {
            await connection.TCP.SendAsync(confirmed).ConfigureAwait(false);

        }
        finally
        {
            InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Return(confirmed);
        }
    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((ushort)OpCommand.EMPLOYEE_SALARY_DELETE)]
    public async ValueTask DeleteAsync(IPacketContext<EmployeeSalaryDto> context)
    {
        EmployeeSalaryDto packet = context.Packet;
        IConnection connection = context.Connection;

        if (packet.EmployeeSalaryId == null)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }

        var result = await _salaryService.DeleteAsync(packet.EmployeeSalaryId.Value).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        await context.OkAsync().ConfigureAwait(false);

    }

    private static EmployeeSalaryDto MapToPacket(EmployeeSalary e, ushort sequenceId)
    {
        var dto = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Get<EmployeeSalaryDto>();
        dto.SequenceId = sequenceId;
        dto.EmployeeSalaryId = e.Id;
        dto.EmployeeId = e.EmployeeId;
        dto.Salary = e.Salary;
        dto.SalaryType = e.SalaryType;
        dto.SalaryUnit = e.SalaryUnit;
        dto.EffectiveFrom = e.EffectiveFrom;
        dto.EffectiveTo = e.EffectiveTo;
        dto.Note = e.Note ?? string.Empty;
        return dto;
    }


}


