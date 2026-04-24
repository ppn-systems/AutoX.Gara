using AutoX.Gara.Application.Repairs;
using AutoX.Gara.Backend.Transport.Common;
// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Entities.Repairs;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Protocol.Repairs;
using Nalix.Common.Networking;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security;
using Nalix.Framework.DataFrames.Pooling;
using System;
using System.Threading.Tasks;
namespace AutoX.Gara.Backend.Transport.Repairs;
/// <summary>
/// Packet Handler for repair task related operations.
/// </summary>
[PacketController]
public sealed class RepairTaskHandler(RepairTaskAppService repairTaskService)
{
    private readonly RepairTaskAppService _repairTaskService = repairTaskService ?? throw new ArgumentNullException(nameof(repairTaskService));
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.REPAIR_TASK_GET)]
    public async ValueTask GetAsync(IPacketContext<RepairTaskQueryRequest> context)
    {
        RepairTaskQueryRequest packet = context.Packet;
        IConnection connection = context.Connection;
        var query = new RepairTaskListQuery(
            packet.Page,
            packet.PageSize,
            packet.SearchTerm ?? string.Empty,
            packet.SortBy,
            packet.SortDescending,
            packet.FilterRepairOrderId > 0 ? packet.FilterRepairOrderId : null,
            packet.FilterEmployeeId > 0 ? packet.FilterEmployeeId : null,
            packet.FilterServiceItemId > 0 ? packet.FilterServiceItemId : null,
            packet.FilterStatus,
            packet.FilterFromDate,
            packet.FilterToDate
        );
        var result = await _repairTaskService.GetPageAsync(query).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }
        using var lease = PacketPool<RepairTaskQueryResponse>.Rent();
        var response = lease.Value;
        response.TotalCount = result.Data!.totalCount;
        response.SequenceId = packet.SequenceId;
        response.RepairTasks = result.Data.items.ConvertAll(t => MapToPacket(t, 0));
        await connection.TCP.SendAsync(response).ConfigureAwait(false);
    }
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.REPAIR_TASK_CREATE)]
    public async ValueTask CreateAsync(IPacketContext<RepairTaskDto> context)
    {
        RepairTaskDto packet = context.Packet;
        IConnection connection = context.Connection;
        var task = new RepairTask
        {
            RepairOrderId = packet.RepairOrderId,
            EmployeeId = packet.EmployeeId,
            ServiceItemId = packet.ServiceItemId,
            Status = packet.Status,
            StartDate = packet.StartDate,
            EstimatedDuration = packet.EstimatedDuration,
            CompletionDate = packet.CompletionDate
        };
        var result = await _repairTaskService.CreateAsync(task).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }
        await connection.TCP.SendAsync(MapToPacket(result.Data!, packet.SequenceId)).ConfigureAwait(false);
    }
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.REPAIR_TASK_UPDATE)]
    public async ValueTask UpdateAsync(IPacketContext<RepairTaskDto> context)
    {
        RepairTaskDto packet = context.Packet;
        IConnection connection = context.Connection;
        if (packet.RepairTaskId == null)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }
        var task = new RepairTask
        {
            Id = packet.RepairTaskId.Value,
            RepairOrderId = packet.RepairOrderId,
            EmployeeId = packet.EmployeeId,
            ServiceItemId = packet.ServiceItemId,
            Status = packet.Status,
            StartDate = packet.StartDate,
            EstimatedDuration = packet.EstimatedDuration,
            CompletionDate = packet.CompletionDate
        };
        var result = await _repairTaskService.UpdateAsync(task).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }
        await connection.TCP.SendAsync(MapToPacket(result.Data!, packet.SequenceId)).ConfigureAwait(false);
    }
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((ushort)OpCommand.REPAIR_TASK_DELETE)]
    public async ValueTask DeleteAsync(IPacketContext<RepairTaskDto> context)
    {
        RepairTaskDto packet = context.Packet;
        IConnection connection = context.Connection;
        if (packet.RepairTaskId == null)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }
        var result = await _repairTaskService.DeleteAsync(packet.RepairTaskId.Value).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }
        await context.OkAsync().ConfigureAwait(false);
    }
    private static RepairTaskDto MapToPacket(RepairTask t, ushort sequenceId) => new()
    {
        SequenceId = sequenceId,
        RepairTaskId = t.Id,
        RepairOrderId = t.RepairOrderId,
        EmployeeId = t.EmployeeId,
        ServiceItemId = t.ServiceItemId,
        Status = t.Status,
        StartDate = t.StartDate,
        EstimatedDuration = t.EstimatedDuration,
        CompletionDate = t.CompletionDate,
        IsCompleted = t.IsCompleted
    };
}
