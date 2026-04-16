// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Protocol.Billings;
using Nalix.Common.Networking;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security;
using Nalix.Framework.Injection;
using Nalix.Framework.Memory.Objects;
using Nalix.Framework.Serialization;
using Nalix.Runtime.Extensions;

namespace AutoX.Gara.Application.Billings;

[PacketController]
public sealed class ServiceItemOps(IDataSessionFactory dataSessionFactory)
{
    private readonly IDataSessionFactory _dataSessionFactory = dataSessionFactory
        ?? throw new System.ArgumentNullException(nameof(dataSessionFactory));

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.SERVICE_ITEM_GET)]
    public async System.Threading.Tasks.Task GetAsync(IPacket p, IConnection connection)
    {
        if (p is not ServiceItemQueryRequest packet)
        {
            System.UInt32 fallbackSeq = p.SequenceId;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, fallbackSeq, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        ServiceItemQueryResponse response = null;

        try
        {
            ServiceItemListQuery query = new(
                Page: packet.Page,
                PageSize: packet.PageSize,
                SearchTerm: packet.SearchTerm,
                SortBy: packet.SortBy,
                SortDescending: packet.SortDescending,
                FilterType: packet.FilterType,
                FilterMinUnitPrice: packet.FilterMinUnitPrice,
                FilterMaxUnitPrice: packet.FilterMaxUnitPrice);

            await using var session = _dataSessionFactory.Create();
            var repo = session.ServiceItems;

            (System.Collections.Generic.List<ServiceItem> items, System.Int32 totalCount) =
                await repo.GetPageAsync(query).ConfigureAwait(false);

            response = new()
            {
                TotalCount = totalCount,
                SequenceId = packet.SequenceId,
                ServiceItems = items.ConvertAll(s => MapToPacket(s, sequenceId: 0))
            };

            await connection.TCP.SendAsync(LiteSerializer.Serialize(response)).ConfigureAwait(false);
        }
        catch (System.ArgumentException)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.VALIDATION_FAILED,
                ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
        finally
        {
            if (response?.ServiceItems != null)
            {
                var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
                foreach (ServiceItemDto dto in response.ServiceItems)
                {
                    pool.Return(dto);
                }
            }
        }
    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.SERVICE_ITEM_CREATE)]
    public async System.Threading.Tasks.Task CreateAsync(IPacket p, IConnection connection)
    {
        if (!TryParseServiceItemPacket(p, out ServiceItemDto packet, out System.UInt32 fallbackSeq) || packet.ServiceItemId is not null)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, fallbackSeq, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        ServiceItemDto confirmed = null;
        try
        {
            await using var session = _dataSessionFactory.Create();
            var repo = session.ServiceItems;

            ServiceItem entity = new()
            {
                Description = packet.Description?.Trim() ?? System.String.Empty,
                Type = packet.Type,
                UnitPrice = packet.UnitPrice
            };

            await repo.AddAsync(entity).ConfigureAwait(false);
            await repo.SaveChangesAsync().ConfigureAwait(false);

            confirmed = MapToPacket(entity, packet.SequenceId);
            await connection.TCP.SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);
        }
        catch (System.ArgumentException)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.VALIDATION_FAILED,
                ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
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
            if (confirmed != null)
            {
                InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Return(confirmed);
            }
        }
    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.SERVICE_ITEM_UPDATE)]
    public async System.Threading.Tasks.Task UpdateAsync(IPacket p, IConnection connection)
    {
        if (!TryParseServiceItemPacket(p, out ServiceItemDto packet, out System.UInt32 fallbackSeq) || packet.ServiceItemId is null)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, fallbackSeq, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        ServiceItemDto confirmed = null;
        try
        {
            await using var session = _dataSessionFactory.Create();
            var repo = session.ServiceItems;

            ServiceItem existing = await repo.GetByIdAsync(packet.ServiceItemId.Value).ConfigureAwait(false);
            if (existing is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
                return;
            }

            existing.Description = packet.Description?.Trim() ?? System.String.Empty;
            existing.Type = packet.Type;
            existing.UnitPrice = packet.UnitPrice;

            repo.Update(existing);
            await repo.SaveChangesAsync().ConfigureAwait(false);

            confirmed = MapToPacket(existing, packet.SequenceId);
            await connection.TCP.SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);
        }
        catch (System.ArgumentException)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.VALIDATION_FAILED,
                ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
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
            if (confirmed != null)
            {
                InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Return(confirmed);
            }
        }
    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((System.UInt16)OpCommand.SERVICE_ITEM_DELETE)]
    public async System.Threading.Tasks.Task DeleteAsync(IPacket p, IConnection connection)
    {
        if (p is not ServiceItemDto packet || packet.ServiceItemId is null)
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
            var repo = session.ServiceItems;

            ServiceItem existing = await repo.GetByIdAsync(packet.ServiceItemId.Value).ConfigureAwait(false);
            if (existing is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
                return;
            }

            repo.Delete(existing);
            await repo.SaveChangesAsync().ConfigureAwait(false);

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

    private static System.Boolean TryParseServiceItemPacket(IPacket p, out ServiceItemDto packet, out System.UInt32 fallbackSeqId)
    {
        fallbackSeqId = p.SequenceId;

        if (p is not ServiceItemDto dto)
        {
            packet = null;
            return false;
        }

        if (System.String.IsNullOrWhiteSpace(dto.Description) || dto.Description.Trim().Length > 255)
        {
            packet = null;
            return false;
        }

        if (dto.UnitPrice <= 0)
        {
            packet = null;
            return false;
        }

        packet = dto;
        return true;
    }

    private static ServiceItemDto MapToPacket(ServiceItem serviceItem, System.UInt32 sequenceId)
    {
        ServiceItemDto dto = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Get<ServiceItemDto>();

        dto.SequenceId = sequenceId;
        dto.ServiceItemId = serviceItem.Id;
        dto.Description = serviceItem.Description ?? System.String.Empty;
        dto.Type = serviceItem.Type;
        dto.UnitPrice = serviceItem.UnitPrice;

        return dto;
    }
}






