// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Inventory;
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Infrastructure.Repositories;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Models;
using AutoX.Gara.Shared.Protocol.Suppliers;
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

namespace AutoX.Gara.Application.Suppliers;

/// <summary>
/// Packet controller xử lý tất cả nghiệp vụ CRUD cho Supplier.
/// <list type="bullet">
///   <item><see cref="GetAsync"/> — Lấy danh sách có phân trang, filter, sort.</item>
///   <item><see cref="CreateAsync"/> — Tạo mới nhà cung cấp.</item>
///   <item><see cref="UpdateAsync"/> — Cập nhật thông tin nhà cung cấp.</item>
///   <item><see cref="ChangeStatusAsync"/> — Thay đổi trạng thái (Active/Inactive/Suspended/...).</item>
/// </list>
/// </summary>
[PacketController]
public sealed class SupplierOps(AutoXDbContextFactory dbContextFactory)
{
    private readonly AutoXDbContextFactory _dbContextFactory = dbContextFactory
        ?? throw new System.ArgumentNullException(nameof(dbContextFactory));

    // ─── GET LIST ─────────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.SUPPLIER_GET)]
    public async System.Threading.Tasks.Task GetAsync(
        IPacket p,
        IConnection connection)
    {
        if (p is not SupplierQueryRequest packet)
        {
            System.UInt32 fallbackSeq = p is IPacketSequenced ps0 ? ps0.SequenceId : 0;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);
            return;
        }

        SupplierQueryResponse response = null;

        try
        {
            SupplierListQuery query = new(
                Page: packet.Page,
                PageSize: packet.PageSize,
                SearchTerm: packet.SearchTerm,
                SortBy: packet.SortBy,
                SortDescending: packet.SortDescending,
                FilterStatus: packet.FilterStatus,
                FilterPaymentTerms: packet.FilterPaymentTerms);

            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var suppliers = new SupplierRepository(db);

            (System.Collections.Generic.List<Supplier> items, System.Int32 totalCount)
                = await suppliers.GetPageAsync(query).ConfigureAwait(false);

            response = new()
            {
                TotalCount = totalCount,
                SequenceId = packet.SequenceId,
                Suppliers = items.ConvertAll(s => MapToPacket(s, sequenceId: 0))
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
                foreach (SupplierDto dto in response.Suppliers)
                {
                    InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>().Return(dto);
                }
            }
        }
    }

    // ─── CREATE ───────────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.SUPPLIER_CREATE)]
    public async System.Threading.Tasks.Task CreateAsync(
        IPacket p,
        IConnection connection)
    {
        if (!TryParseSupplierPacket(p, out SupplierDto packet, out System.UInt32 fallbackSeq))
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);
            return;
        }

        // ContractEndDate nếu có phải sau ContractStartDate
        if (packet!.ContractEndDate.HasValue
            && packet.ContractStartDate.HasValue
            && packet.ContractEndDate <= packet.ContractStartDate)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);
            return;
        }

        SupplierDto confirmed = null;

        try
        {
            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var suppliers = new SupplierRepository(db);

            System.Boolean existed = await suppliers
                .ExistsByContactAsync(packet.Email, packet.TaxCode).ConfigureAwait(false);

            if (existed)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.ALREADY_EXISTS,
                    ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);
                return;
            }

            Supplier newSupplier = new()
            {
                Name = packet.Name,
                Email = packet.Email,
                Address = packet.Address,
                TaxCode = packet.TaxCode,
                BankAccount = packet.BankAccount,
                Notes = packet.Notes ?? System.String.Empty,
                PaymentTerms = packet.PaymentTerms ?? Domain.Enums.Payments.PaymentTerms.None,
                Status = packet.Status ?? SupplierStatus.Active,
                ContractStartDate = packet.ContractStartDate ?? System.DateTime.UtcNow,
                ContractEndDate = packet.ContractEndDate,
                PhoneNumbers = []
            };

            if (!System.String.IsNullOrWhiteSpace(packet.PhoneNumbers))
            {
                foreach (System.String phone in packet.PhoneNumbers
                    .Split(',', System.StringSplitOptions.RemoveEmptyEntries))
                {
                    System.String trimmed = phone.Trim();
                    if (AccountValidation.IsValidVietnamPhoneNumber(trimmed))
                    {
                        newSupplier.PhoneNumbers.Add(new SupplierContactPhone
                        {
                            PhoneNumber = trimmed
                        });
                    }
                }
            }

            await suppliers.AddAsync(newSupplier).ConfigureAwait(false);
            await suppliers.SaveChangesAsync().ConfigureAwait(false);

            confirmed = MapToPacket(newSupplier, packet.SequenceId);
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
                InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                    .Return(confirmed);
            }
        }
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.SUPPLIER_UPDATE)]
    public async System.Threading.Tasks.Task UpdateAsync(
        IPacket p,
        IConnection connection)
    {
        if (!TryParseSupplierPacket(p, out SupplierDto packet, out System.UInt32 fallbackSeq))
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);
            return;
        }

        if (packet!.SupplierId is null)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);
            return;
        }

        if (packet.ContractEndDate.HasValue
            && packet.ContractStartDate.HasValue
            && packet.ContractEndDate <= packet.ContractStartDate)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);
            return;
        }

        SupplierDto confirmed = null;

        try
        {
            await using AutoXDbContext db = _dbContextFactory.CreateDbContext();
            var suppliers = new SupplierRepository(db);

            Supplier existing = await suppliers
                .GetByIdAsync(packet.SupplierId.Value).ConfigureAwait(false);

            if (existing is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
                return;
            }

            existing.Name = packet.Name;
            existing.Email = packet.Email;
            existing.Address = packet.Address;
            existing.TaxCode = packet.TaxCode;
            existing.BankAccount = packet.BankAccount;
            existing.Notes = packet.Notes ?? System.String.Empty;
            existing.ContractEndDate = packet.ContractEndDate;

            if (packet.PaymentTerms.HasValue)
            {
                existing.PaymentTerms = packet.PaymentTerms.Value;
            }

            if (packet.ContractStartDate.HasValue)
            {
                existing.ContractStartDate = packet.ContractStartDate.Value;
            }

            suppliers.Update(existing);
            await suppliers.SaveChangesAsync().ConfigureAwait(false);

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
                InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                    .Return(confirmed);
            }
        }
    }

    // ─── CHANGE STATUS ────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((System.UInt16)OpCommand.SUPPLIER_CHANGE_STATUS)]
    public async System.Threading.Tasks.Task ChangeStatusAsync(
        IPacket p,
        IConnection connection)
    {
        if (p is not SupplierDto packet
            || packet.SupplierId is null
            || packet.Status is null)
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
            var suppliers = new SupplierRepository(db);

            Supplier existing = await suppliers
                .GetByIdAsync(packet.SupplierId.Value).ConfigureAwait(false);

            if (existing is null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
                return;
            }

            existing.Status = packet.Status.Value;

            suppliers.Update(existing);
            await suppliers.SaveChangesAsync().ConfigureAwait(false);

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

    private static System.Boolean TryParseSupplierPacket(
        IPacket p,
        out SupplierDto packet,
        out System.UInt32 fallbackSeqId)
    {
        fallbackSeqId = p is IPacketSequenced ps ? ps.SequenceId : 0;

        if (p is not SupplierDto sp ||
            !AccountValidation.IsValidEmail(sp.Email))
        {
            packet = null;
            return false;
        }

        packet = sp;
        return true;
    }

    private static SupplierDto MapToPacket(Supplier s, System.UInt32 sequenceId)
    {
        SupplierDto data = InstanceManager.Instance
            .GetOrCreateInstance<ObjectPoolManager>()
            .Get<SupplierDto>();

        data.SequenceId = sequenceId;
        data.SupplierId = s.Id;
        data.Name = s.Name;
        data.Email = s.Email;
        data.Address = s.Address;
        data.TaxCode = s.TaxCode;
        data.BankAccount = s.BankAccount;
        data.Notes = s.Notes ?? System.String.Empty;
        data.Status = s.Status;
        data.PaymentTerms = s.PaymentTerms;
        data.ContractStartDate = s.ContractStartDate;
        data.ContractEndDate = s.ContractEndDate;

        data.PhoneNumbers = s.PhoneNumbers?.Count > 0
            ? System.String.Join(',', System.Linq.Enumerable.Select(s.PhoneNumbers, ph => ph.PhoneNumber))
            : System.String.Empty;

        return data;
    }
}