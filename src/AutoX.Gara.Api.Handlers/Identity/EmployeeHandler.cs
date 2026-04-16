using AutoX.Gara.Shared.Enums;
using Nalix.Common.Networking.Protocols;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Services;
using AutoX.Gara.Domain.Entities.Identity;
using Nalix.Common.Networking.Protocols;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Protocol.Employees;
using Microsoft.Extensions.Logging;
using Nalix.Common.Networking;
using Nalix.Common.Networking.Packets;
using AutoX.Gara.Api.Handlers.Common;
using Nalix.Framework.DataFrames.SignalFrames;
using Nalix.Framework.DataFrames.Pooling;
using Nalix.Common.Security;
using Nalix.Framework.Injection;
using Nalix.Framework.Memory.Objects;
using Nalix.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AutoX.Gara.Api.Handlers.Identity;

/// <summary>
/// Packet Handler for employee related operations.
/// </summary>
[PacketController]
public sealed class EmployeeHandler(IEmployeeAppService employeeService)
{
    private readonly IEmployeeAppService _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.EMPLOYEE_GET)]
    public async ValueTask GetAsync(IPacketContext<EmployeeQueryRequest> context)
    {
        EmployeeQueryRequest packet = context.Packet;
        IConnection connection = context.Connection;

        var result = await _employeeService.GetPageAsync(BuildListQuery(packet)).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        using var lease = PacketPool<EmployeeQueryResponse>.Rent();
        var response = lease.Value;
        response.TotalCount = result.Data!.totalCount;
        response.SequenceId = packet.SequenceId;
        response.Employees = result.Data.items.ConvertAll(e => MapToPacket(e, packet.SequenceId));

        try
        {
            await connection.TCP.SendAsync(response).ConfigureAwait(false);

        }
        finally
        {
            ReturnDtos(response.Employees);
        }
    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((ushort)OpCommand.EMPLOYEE_CREATE)]
    public async ValueTask CreateAsync(IPacketContext<EmployeeDto> context)
    {
        EmployeeDto packet = context.Packet;
        IConnection connection = context.Connection;

        if (string.IsNullOrWhiteSpace(packet.Name))
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }

        var employee = new Employee
        {
            Name = packet.Name,
            Email = packet.Email ?? string.Empty,
            Address = packet.Address ?? string.Empty,
            PhoneNumber = packet.PhoneNumber ?? string.Empty,
            Gender = packet.Gender ?? AutoX.Gara.Domain.Enums.Gender.None,
            Position = packet.Position ?? AutoX.Gara.Domain.Enums.Employees.Position.None,
            Status = packet.Status ?? AutoX.Gara.Domain.Enums.Employees.EmploymentStatus.None,
            DateOfBirth = packet.DateOfBirth,
            StartDate = packet.StartDate ?? DateTime.UtcNow,
            EndDate = packet.EndDate
        };

        var result = await _employeeService.CreateAsync(employee).ConfigureAwait(false);
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
            InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Return(confirmed);
        }
    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((ushort)OpCommand.EMPLOYEE_UPDATE)]
    public async ValueTask UpdateAsync(IPacketContext<EmployeeDto> context)
    {
        EmployeeDto packet = context.Packet;
        IConnection connection = context.Connection;

        if (packet.EmployeeId == null)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }

        var employee = new Employee
        {
            Id = packet.EmployeeId.Value,
            Name = packet.Name,
            Email = packet.Email ?? string.Empty,
            Address = packet.Address ?? string.Empty,
            PhoneNumber = packet.PhoneNumber ?? string.Empty,
            Gender = packet.Gender ?? AutoX.Gara.Domain.Enums.Gender.None,
            Position = packet.Position ?? AutoX.Gara.Domain.Enums.Employees.Position.None,
            Status = packet.Status ?? AutoX.Gara.Domain.Enums.Employees.EmploymentStatus.None,
            DateOfBirth = packet.DateOfBirth,
            StartDate = packet.StartDate ?? DateTime.UtcNow,
            EndDate = packet.EndDate
        };

        var result = await _employeeService.UpdateAsync(employee).ConfigureAwait(false);
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
            InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Return(confirmed);
        }
    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((ushort)OpCommand.EMPLOYEE_CHANGE_STATUS)]
    public async ValueTask ChangeStatusAsync(IPacketContext<EmployeeDto> context)
    {
        EmployeeDto packet = context.Packet;
        IConnection connection = context.Connection;

        if (packet.EmployeeId == null || packet.Status == null)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }

        var result = await _employeeService.ChangeStatusAsync(packet.EmployeeId.Value, packet.Status.Value).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }

        await context.OkAsync().ConfigureAwait(false);

    }

    private static EmployeeListQuery BuildListQuery(EmployeeQueryRequest request)
        => new(request.Page, request.PageSize, request.SearchTerm, request.SortBy, request.SortDescending, request.FilterPosition, request.FilterStatus, request.FilterGender);

    private static EmployeeDto MapToPacket(Employee e, uint sequenceId)
    {
        var data = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Get<EmployeeDto>();
        data.SequenceId = sequenceId;
        data.EmployeeId = e.Id;
        data.Name = e.Name;
        data.Email = e.Email;
        data.Address = e.Address;
        data.PhoneNumber = e.PhoneNumber;
        data.Gender = e.Gender;
        data.Position = e.Position;
        data.Status = e.Status;
        data.DateOfBirth = e.DateOfBirth;
        data.StartDate = e.StartDate;
        data.EndDate = e.EndDate;
        return data;
    }

    private static void ReturnDtos(IEnumerable<EmployeeDto> dtos)
    {
        if (dtos == null) return;
        var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
        foreach (var dto in dtos) pool.Return(dto);
    }

    
}