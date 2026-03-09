// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Customers;
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
using System;

namespace AutoX.Gara.Application.Customers;

/// <summary>
/// Service quản lý khách hàng, chỉ nhân viên có quyền thao tác các nghiệp vụ CRUD.
/// </summary>
[PacketController]
public sealed class CustomerOps(AutoXDbContext context)
{
    private readonly DataRepository<Customer> s_customer = new(context);

    /// <summary>
    /// Lấy danh sách khách hàng (phân trang), chỉ cho nhân viên.
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
            if (p is not IPacketSequenced ps)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.MALFORMED_PACKET,
                    ProtocolAdvice.DO_NOT_RETRY).ConfigureAwait(false);

                return;
            }

            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, ps.SequenceId).ConfigureAwait(false);

            return;
        }

        try
        {
            System.Collections.Generic.List<Customer> customers;
            System.Collections.Generic.List<CustomerDataPacket> customerPackets;

            customers = await s_customer.GetAllAsync(packet.Page, packet.PageSize) ?? [];

            customerPackets = customers.ConvertAll(c => new CustomerDataPacket
            {
                Type = c.Type,
                Name = c.Name,
                Email = c.Email,
                CustomerId = c.Id,
                TaxCode = c.TaxCode,
                Address = c.Address,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                Membership = c.Membership,
                PhoneNumber = c.PhoneNumber,
                DateOfBirth = c.DateOfBirth
            });

            CustomersPacket customersPacket = new()
            {
                Customers = customerPackets,
                SequenceId = packet.SequenceId
            };

            Boolean ok = await connection.TCP.SendAsync(LiteSerializer.Serialize(customersPacket))
                                             .ConfigureAwait(false);
        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.RETRY, packet.SequenceId).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Tạo mới khách hàng, chỉ cho nhân viên.
    /// </summary>
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.CUSTOMER_CREATE)]
    public async System.Threading.Tasks.Task CreateAsync(
        IPacket p,
        IConnection connection)
    {
        if (p is not CustomerDataPacket packet ||
            !AccountValidation.IsValidEmail(packet.Email) ||
            !AccountValidation.IsValidVietnamPhoneNumber(packet.PhoneNumber))
        {
            if (p is not IPacketSequenced ps)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.MALFORMED_PACKET,
                    ProtocolAdvice.DO_NOT_RETRY).ConfigureAwait(false);

                return;
            }

            // MALFORMED_PACKET: Packet từ client không đúng định dạng hoặc thiếu field cần thiết.
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, ps.SequenceId).ConfigureAwait(false);

            return;
        }

        // Kiểm tra email/số điện thoại đã tồn tại
        Boolean existed = await s_customer.AnyAsync(c => c.Email == packet.Email || c.PhoneNumber == packet.PhoneNumber);

        if (existed)
        {
            // ALREADY_EXISTS: Email hoặc số điện thoại đã tồn tại trong database, không cho phép trùng.
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.ALREADY_EXISTS,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);
            return;
        }

        try
        {
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
                CreatedAt = System.DateTime.UtcNow,
                UpdatedAt = System.DateTime.UtcNow
            };

            await s_customer.AddAsync(newCustomer);
            await s_customer.SaveChangesAsync();

            // NONE: Thành công, không có lỗi.
            await connection.SendAsync(
                ControlType.NONE,
                ProtocolReason.NONE,
                ProtocolAdvice.NONE, packet.SequenceId).ConfigureAwait(false);
        }
        catch (System.Exception)
        {
            // INTERNAL_ERROR: Lỗi hệ thống (ví dụ không ghi DB được, lỗi nền tảng).
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Sửa thông tin khách hàng, chỉ cho nhân viên.
    /// </summary>
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.CUSTOMER_UPDATE)]
    public async System.Threading.Tasks.Task UpdateAsync(
        IPacket p,
        IConnection connection)
    {
        if (p is not CustomerDataPacket packet ||
            !AccountValidation.IsValidEmail(packet.Email) ||
            !AccountValidation.IsValidVietnamPhoneNumber(packet.PhoneNumber))
        {
            // MALFORMED_PACKET: Packet sai format, không parse được dữ liệu khách hàng.
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY).ConfigureAwait(false);
            return;
        }

        // Kiểm tra tồn tại
        Customer existing = await s_customer.GetByIdAsync(packet.CustomerId);
        if (existing == null)
        {
            // NOT_FOUND: Không tìm thấy khách hàng cần sửa trong DB.
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.NOT_FOUND,
                ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
            return;
        }

        // Update all business fields
        existing.Name = packet.Name;
        existing.Type = packet.Type;
        existing.Email = packet.Email;
        existing.TaxCode = packet.TaxCode;
        existing.Address = packet.Address;
        existing.Membership = packet.Membership;
        existing.PhoneNumber = packet.PhoneNumber;
        existing.DateOfBirth = packet.DateOfBirth;
        existing.UpdatedAt = System.DateTime.UtcNow;

        try
        {
            s_customer.Update(existing);
            await s_customer.SaveChangesAsync();

            // NONE: Update thành công.
            await connection.SendAsync(
                ControlType.NONE,
                ProtocolReason.NONE,
                ProtocolAdvice.NONE, packet.SequenceId).ConfigureAwait(false);
        }
        catch (System.Exception)
        {
            // INTERNAL_ERROR: Lỗi ghi database hoặc nền tảng.
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Xóa khách hàng, chỉ cho nhân viên.
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
            // MALFORMED_PACKET: Packet xóa nhưng không đúng định dạng hoặc thiếu id.
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY).ConfigureAwait(false);
            return;
        }

        try
        {
            await s_customer.DeleteAsync(packet.CustomerId);
            await s_customer.SaveChangesAsync();

            // NONE: Xóa thành công.
            await connection.SendAsync(
                ControlType.NONE,
                ProtocolReason.NONE,
                ProtocolAdvice.NONE, packet.SequenceId).ConfigureAwait(false);
        }
        catch (System.Exception)
        {
            // INTERNAL_ERROR: Lỗi nền tảng hoặc database khi xóa.
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
        }
    }
}