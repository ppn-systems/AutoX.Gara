// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Customers;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Packets.Customers;
using AutoX.Gara.Shared.Validation;
using Nalix.Common.Networking.Abstractions;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Attributes;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security.Enums;
using Nalix.Network.Connections;
using Nalix.Shared.Serialization;
using System.Linq;

namespace AutoX.Gara.Application.Customers;

/// <summary>
/// Service quản lý khách hàng, chỉ nhân viên có quyền thao tác các nghiệp vụ CRUD.
/// </summary>
[PacketController]
public sealed class CustomerOps(AutoXDbContext context)
{
    private readonly DataRepository<Customer> _customers = new(context);

    // ─── GET LIST ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Lấy danh sách khách hàng với phân trang, tìm kiếm, lọc và sắp xếp.
    /// Chỉ trả về khách hàng chưa bị xóa mềm (DeletedAt == null).
    /// </summary>
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.CUSTOMER_LIST)]
    public async System.Threading.Tasks.Task GetListAsync(
        IPacket p,
        IConnection connection)
    {
        if (p is not CustomersQueryPacket packet)
        {
            System.UInt32 seqId = p is IPacketSequenced ps0 ? ps0.SequenceId : 0;
            await SendErrorAsync(connection, ProtocolReason.MALFORMED_PACKET, ProtocolAdvice.DO_NOT_RETRY, seqId)
                .ConfigureAwait(false);
            return;
        }

        try
        {
            // ── Soft delete: chỉ lấy bản ghi chưa bị xóa ─────────────────
            System.Linq.IQueryable<Customer> query = _customers.AsQueryable()
                .Where(c => c.DeletedAt == null);

            // ── Search ────────────────────────────────────────────────────
            if (!System.String.IsNullOrWhiteSpace(packet.SearchTerm))
            {
                System.String term = packet.SearchTerm.Trim().ToLowerInvariant();
                query = query.Where(c =>
                    (c.Name != null && c.Name.ToLower().Contains(term)) ||
                    (c.Email != null && c.Email.ToLower().Contains(term)) ||
                    (c.PhoneNumber != null && c.PhoneNumber.Contains(term)) ||
                    (c.Notes != null && c.Notes.ToLower().Contains(term)));
            }

            // ── Filter theo CustomerType ───────────────────────────────────
            if (packet.FilterType != CustomerType.None)
            {
                query = query.Where(c => c.Type == packet.FilterType);
            }

            // ── Filter theo MembershipLevel ───────────────────────────────
            if (packet.FilterMembership != MembershipLevel.None)
            {
                query = query.Where(c => c.Membership == packet.FilterMembership);
            }

            // ── Sort ───────────────────────────────────────────────────────
            query = (packet.SortBy, packet.SortDescending) switch
            {
                (CustomerSortField.Name, false) => query.OrderBy(c => c.Name),
                (CustomerSortField.Name, true) => query.OrderByDescending(c => c.Name),
                (CustomerSortField.Email, false) => query.OrderBy(c => c.Email),
                (CustomerSortField.Email, true) => query.OrderByDescending(c => c.Email),
                (CustomerSortField.CreatedAt, false) => query.OrderBy(c => c.CreatedAt),
                (CustomerSortField.CreatedAt, true) => query.OrderByDescending(c => c.CreatedAt),
                (CustomerSortField.UpdatedAt, false) => query.OrderBy(c => c.UpdatedAt),
                (CustomerSortField.UpdatedAt, true) => query.OrderByDescending(c => c.UpdatedAt),
                _ => query.OrderByDescending(c => c.CreatedAt)
            };

            System.Int32 totalCount = await _customers.CountAsync(query).ConfigureAwait(false);

            System.Collections.Generic.List<Customer> customers =
                await _customers.GetPagedAsync(query, packet.Page, packet.PageSize)
                                .ConfigureAwait(false) ?? [];

            System.Collections.Generic.List<CustomerDataPacket> customerPackets =
                customers.ConvertAll(c => MapToPacket(c, sequenceId: 0));

            CustomersPacket response = new()
            {
                TotalCount = totalCount,
                Customers = customerPackets,
                SequenceId = packet.SequenceId
            };

            System.Byte[] buffer = LiteSerializer.Serialize(response);

            System.Console.WriteLine(
                $"[SERVER] Sending CustomersPacket: SeqId={response.SequenceId}, " +
                $"Count={response.Customers.Count}, TotalCount={response.TotalCount}, " +
                $"BufferLen={buffer.Length}");

            System.Boolean sent = await connection.TCP.SendAsync(buffer).ConfigureAwait(false);
            System.Console.WriteLine($"[SERVER] SendAsync result: {sent}");

            if (!sent)
            {
                await SendErrorAsync(
                    connection, ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId)
                .ConfigureAwait(false);
            }
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"[SERVER] EXCEPTION: {ex.GetType().Name}: {ex.Message}");
            System.Console.WriteLine($"[SERVER] StackTrace: {ex.StackTrace}");
            if (ex.InnerException is not null)
            {
                System.Console.WriteLine($"[SERVER] Inner: {ex.InnerException.Message}");
            }

            await SendErrorAsync(connection, ProtocolReason.INTERNAL_ERROR, ProtocolAdvice.RETRY, packet.SequenceId)
                .ConfigureAwait(false);
        }
    }

    // ─── CREATE ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Tạo mới khách hàng và trả về entity đã được lưu (bao gồm Id, timestamps từ DB).
    /// </summary>
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.CUSTOMER_CREATE)]
    public async System.Threading.Tasks.Task CreateAsync(
        IPacket p,
        IConnection connection)
    {
        if (!TryParseCustomerPacket(p, out CustomerDataPacket packet, out System.UInt32 fallbackSeq))
        {
            await SendErrorAsync(connection, ProtocolReason.MALFORMED_PACKET, ProtocolAdvice.DO_NOT_RETRY, fallbackSeq)
                .ConfigureAwait(false);
            return;
        }

        if (packet!.DateOfBirth != default && packet.DateOfBirth > System.DateTime.UtcNow)
        {
            await SendErrorAsync(connection, ProtocolReason.MALFORMED_PACKET, ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId)
                .ConfigureAwait(false);
            return;
        }

        // Kiểm tra trùng lặp — chỉ xét bản ghi chưa bị soft delete
        System.Boolean existed = await _customers.AnyAsync(
            c => c.DeletedAt == null && (c.Email == packet.Email || c.PhoneNumber == packet.PhoneNumber))
            .ConfigureAwait(false);

        if (existed)
        {
            await SendErrorAsync(connection, ProtocolReason.ALREADY_EXISTS, ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId)
                .ConfigureAwait(false);
            return;
        }

        try
        {
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

            await _customers.AddAsync(newCustomer).ConfigureAwait(false);
            await _customers.SaveChangesAsync().ConfigureAwait(false);

            CustomerDataPacket confirmed = MapToPacket(newCustomer, packet.SequenceId);
            await connection.TCP.SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);
        }
        catch (System.Exception)
        {
            await SendErrorAsync(connection, ProtocolReason.INTERNAL_ERROR, ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId)
                .ConfigureAwait(false);
        }
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Cập nhật thông tin khách hàng và trả về entity đã được lưu.
    /// Không cho phép update khách hàng đã bị xóa mềm.
    /// </summary>
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.CUSTOMER_UPDATE)]
    public async System.Threading.Tasks.Task UpdateAsync(
        IPacket p,
        IConnection connection)
    {
        if (!TryParseCustomerPacket(p, out CustomerDataPacket packet, out System.UInt32 fallbackSeq))
        {
            await SendErrorAsync(connection, ProtocolReason.MALFORMED_PACKET, ProtocolAdvice.DO_NOT_RETRY, fallbackSeq)
                .ConfigureAwait(false);
            return;
        }

        if (packet!.DateOfBirth != default && packet.DateOfBirth > System.DateTime.UtcNow)
        {
            await SendErrorAsync(connection, ProtocolReason.MALFORMED_PACKET, ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId)
                .ConfigureAwait(false);
            return;
        }

        Customer existing = await _customers.GetByIdAsync(packet.CustomerId).ConfigureAwait(false);

        // Không cho phép update nếu đã bị xóa mềm
        if (existing is null || existing.DeletedAt != null)
        {
            await SendErrorAsync(connection, ProtocolReason.NOT_FOUND, ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId)
                .ConfigureAwait(false);
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

        try
        {
            _customers.Update(existing);
            await _customers.SaveChangesAsync().ConfigureAwait(false);

            CustomerDataPacket confirmed = MapToPacket(existing, packet.SequenceId);
            await connection.TCP.SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);
        }
        catch (System.Exception)
        {
            await SendErrorAsync(connection, ProtocolReason.INTERNAL_ERROR, ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId)
                .ConfigureAwait(false);
        }
    }

    // ─── DELETE (Soft) ────────────────────────────────────────────────────────

    /// <summary>
    /// Xóa mềm khách hàng: ghi lại <c>DeletedAt = UtcNow</c>.
    /// Dữ liệu vẫn còn trong DB để bảo toàn lịch sử giao dịch liên quan.
    /// Yêu cầu quyền SUPERVISOR.
    /// </summary>
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((System.UInt16)OpCommand.CUSTOMER_DELETE)]
    public async System.Threading.Tasks.Task DeleteAsync(
        IPacket p,
        IConnection connection)
    {
        if (p is not CustomerDataPacket packet || packet.CustomerId == null)
        {
            System.UInt32 seqId = p is IPacketSequenced ps ? ps.SequenceId : 0;
            await SendErrorAsync(connection, ProtocolReason.MALFORMED_PACKET, ProtocolAdvice.DO_NOT_RETRY, seqId)
                .ConfigureAwait(false);
            return;
        }

        try
        {
            Customer existing = await _customers.GetByIdAsync(packet.CustomerId).ConfigureAwait(false);

            if (existing is null || existing.DeletedAt != null)
            {
                await SendErrorAsync(connection, ProtocolReason.NOT_FOUND, ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId)
                    .ConfigureAwait(false);
                return;
            }

            // Soft delete — không xóa cứng khỏi DB
            System.DateTime now = System.DateTime.UtcNow;
            existing.DeletedAt = now;
            existing.UpdatedAt = now;

            _customers.Update(existing);
            await _customers.SaveChangesAsync().ConfigureAwait(false);

            await connection.SendAsync(
                ControlType.NONE,
                ProtocolReason.NONE,
                ProtocolAdvice.NONE,
                packet.SequenceId).ConfigureAwait(false);
        }
        catch (System.Exception)
        {
            await SendErrorAsync(connection, ProtocolReason.INTERNAL_ERROR, ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId)
                .ConfigureAwait(false);
        }
    }

    // ─── Private Helpers ─────────────────────────────────────────────────────

    private static System.Boolean TryParseCustomerPacket(
        IPacket p,
        out CustomerDataPacket packet,
        out System.UInt32 fallbackSeqId)
    {
        fallbackSeqId = p is IPacketSequenced ps ? ps.SequenceId : 0;

        if (p is not CustomerDataPacket cp ||
            !AccountValidation.IsValidEmail(cp.Email) ||
            !AccountValidation.IsValidVietnamPhoneNumber(cp.PhoneNumber))
        {
            packet = null;
            return false;
        }

        packet = cp;
        return true;
    }

    /// <summary>Maps a <see cref="Customer"/> entity to a <see cref="CustomerDataPacket"/>.</summary>
    private static CustomerDataPacket MapToPacket(Customer c, System.UInt32 sequenceId) => new()
    {
        SequenceId = sequenceId,
        CustomerId = c.Id,
        Type = c.Type,
        Name = c.Name,
        Email = c.Email,
        TaxCode = c.TaxCode,
        Address = c.Address,
        Membership = c.Membership,
        PhoneNumber = c.PhoneNumber,
        DateOfBirth = c.DateOfBirth,
        Gender = c.Gender,
        Notes = c.Notes ?? System.String.Empty,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };

    private static System.Threading.Tasks.Task SendErrorAsync(
        IConnection connection,
        ProtocolReason reason,
        ProtocolAdvice advice,
        System.UInt32 sequenceId)
        => sequenceId == 0
            ? connection.SendAsync(ControlType.ERROR, reason, advice)
            : connection.SendAsync(ControlType.ERROR, reason, advice, sequenceId);
}