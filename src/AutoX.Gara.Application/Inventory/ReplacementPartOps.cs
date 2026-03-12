// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Inventory;
using AutoX.Gara.Infrastructure.Abstractions;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Protocol.Inventory;
using Nalix.Common.Networking.Abstractions;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Attributes;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security.Enums;
using Nalix.Framework.Injection;
using Nalix.Network.Connections;
using Nalix.Shared.Memory.Pooling;
using Nalix.Shared.Serialization;

namespace AutoX.Gara.Application.Inventory;

/// <summary>
/// Packet controller xử lý tất cả nghiệp vụ CRUD cho <c>ReplacementPart</c> (phụ tùng kho).
/// <para>
/// Điểm khác biệt so với <c>SparePartOps</c>:
/// <list type="bullet">
///   <item>DELETE là hard delete (xóa vĩnh viễn khỏi DB) vì không cần audit trail.</item>
///   <item>Validate <c>ExpiryDate</c> &gt; <c>DateAdded</c> trước khi lưu.</item>
///   <item><c>IncreaseStock</c> / <c>DecreaseStock</c> là các operation riêng biệt
///         để tránh race condition khi update số lượng.</item>
/// </list>
/// </para>
/// </summary>
[PacketController]
public sealed class ReplacementPartOps(IReplacementPartRepository replacementParts)
{
    private readonly IReplacementPartRepository _replacementParts = replacementParts
        ?? throw new System.ArgumentNullException(nameof(replacementParts));

    // ─── GET LIST ─────────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.REPLACEMENT_PART_GET)]
    public async System.Threading.Tasks.Task GetAsync(IPacket p, IConnection connection)
    {
        if (p is not ReplacementPartQueryRequest packet)
        {
            System.UInt32 fallbackSeq = p is IPacketSequenced ps ? ps.SequenceId : 0;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);

            return;
        }

        ReplacementPartQueryResponse response = null;

        try
        {
            ReplacementPartListQuery query = new(
                Page: packet.Page,
                PageSize: packet.PageSize,
                SearchTerm: packet.SearchTerm,
                SortBy: packet.SortBy,
                SortDescending: packet.SortDescending,
                FilterInStock: packet.FilterInStock,
                FilterDefective: packet.FilterDefective,
                FilterExpired: packet.FilterExpired);

            (System.Collections.Generic.List<ReplacementPart> items, System.Int32 totalCount)
                = await _replacementParts.GetPageAsync(query).ConfigureAwait(false);

            response = new()
            {
                TotalCount = totalCount,
                SequenceId = packet.SequenceId,
                Parts = items.ConvertAll(r => MapToPacket(r, sequenceId: 0))
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
                foreach (ReplacementPartDto dto in response.Parts)
                {
                    pool.Return(dto);
                }
            }
        }
    }

    // ─── CREATE ───────────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.REPLACEMENT_PART_CREATE)]
    public async System.Threading.Tasks.Task CreateAsync(IPacket p, IConnection connection)
    {
        if (p is not ReplacementPartDto packet ||
            System.String.IsNullOrWhiteSpace(packet.PartCode) ||
            System.String.IsNullOrWhiteSpace(packet.PartName))
        {
            System.UInt32 fallbackSeq = p is IPacketSequenced ps ? ps.SequenceId : 0;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);

            return;
        }

        // Validate ExpiryDate phải sau DateAdded
        if (packet.ExpiryDate.HasValue && packet.ExpiryDate.Value < packet.DateAdded)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);

            return;
        }

        System.Boolean existed = await _replacementParts
            .ExistsByPartCodeAsync(packet.PartCode).ConfigureAwait(false);

        if (existed)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.ALREADY_EXISTS,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);

            return;
        }

        ReplacementPartDto confirmed = null;

        try
        {
            ReplacementPart newPart = new()
            {
                PartCode = packet.PartCode,
                PartName = packet.PartName,
                Manufacturer = packet.Manufacturer ?? System.String.Empty,
                Quantity = packet.Quantity,
                UnitPrice = packet.UnitPrice,
                DateAdded = packet.DateAdded,
                ExpiryDate = packet.ExpiryDate
                // IsDefective mặc định false theo entity
            };

            await _replacementParts.AddAsync(newPart).ConfigureAwait(false);
            await _replacementParts.SaveChangesAsync().ConfigureAwait(false);

            confirmed = MapToPacket(newPart, packet.SequenceId);
            System.Boolean sent = await connection.TCP.SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);

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
                ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
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
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.REPLACEMENT_PART_UPDATE)]
    public async System.Threading.Tasks.Task UpdateAsync(IPacket p, IConnection connection)
    {
        if (p is not ReplacementPartDto packet || packet.PartId is null ||
            System.String.IsNullOrWhiteSpace(packet.PartName))
        {
            System.UInt32 fallbackSeq = p is IPacketSequenced ps ? ps.SequenceId : 0;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);

            return;
        }

        if (packet.ExpiryDate < packet.DateAdded)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);

            return;
        }

        ReplacementPart existing = await _replacementParts
            .GetByIdAsync(packet.PartId.Value).ConfigureAwait(false);

        if (existing is null)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.NOT_FOUND,
                ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);

            return;
        }

        // Dùng domain methods thay vì gán thẳng để giữ business rules
        existing.PartName = packet.PartName;
        existing.Manufacturer = packet.Manufacturer ?? System.String.Empty;
        existing.UnitPrice = packet.UnitPrice;
        existing.DateAdded = packet.DateAdded;
        existing.ExpiryDate = packet.ExpiryDate;

        // IsDefective chỉ được thay đổi qua domain methods (MarkAsDefective/UnmarkAsDefective)
        if (packet.IsDefective && !existing.IsDefective)
        {
            existing.MarkAsDefective();
        }
        else if (!packet.IsDefective && existing.IsDefective)
        {
            existing.UnmarkAsDefective();
        }

        ReplacementPartDto confirmed = null;

        try
        {
            _replacementParts.Update(existing);
            await _replacementParts.SaveChangesAsync().ConfigureAwait(false);

            confirmed = MapToPacket(existing, packet.SequenceId);
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
                ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
        }
        finally
        {
            if (confirmed is not null)
            {
                InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Return(confirmed);
            }
        }
    }

    // ─── DELETE (Hard) ────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((System.UInt16)OpCommand.REPLACEMENT_PART_DELETE)]
    public async System.Threading.Tasks.Task DeleteAsync(IPacket p, IConnection connection)
    {
        if (p is not ReplacementPartDto packet || packet.PartId is null)
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
            ReplacementPart existing = await _replacementParts
                .GetByIdAsync(packet.PartId.Value).ConfigureAwait(false);

            if (existing is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);

                return;
            }

            _replacementParts.Delete(existing);
            await _replacementParts.SaveChangesAsync().ConfigureAwait(false);

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

    private static ReplacementPartDto MapToPacket(ReplacementPart r, System.UInt32 sequenceId)
    {
        ReplacementPartDto dto = InstanceManager.Instance
            .GetOrCreateInstance<ObjectPoolManager>()
            .Get<ReplacementPartDto>();

        dto.SequenceId = sequenceId;
        dto.PartId = r.Id;
        dto.PartCode = r.PartCode;
        dto.PartName = r.PartName;
        dto.Manufacturer = r.Manufacturer;
        dto.Quantity = r.Quantity;
        dto.UnitPrice = r.UnitPrice;
        dto.IsDefective = r.IsDefective;
        dto.DateAdded = r.DateAdded;
        dto.ExpiryDate = r.ExpiryDate;

        return dto;
    }
}