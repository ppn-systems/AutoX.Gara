// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Packets.Customers;
using Nalix.Common.Connection;
using Nalix.Common.Enums;
using Nalix.Common.Messaging.Packets.Abstractions;
using Nalix.Common.Messaging.Packets.Attributes;
using Nalix.Common.Messaging.Protocols;
using Nalix.Network.Connections;

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
            // MALFORMED_PACKET: Packet từ client không đúng định dạng hoặc thiếu field cần thiết.
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY).ConfigureAwait(false);

            return;
        }

        System.Collections.Generic.List<Customer> customers = await s_customer.GetAllAsync(packet.Page, packet.PageSize);

        System.Collections.Generic.List<CustomerDataPacket> customerPackets = customers.ConvertAll(c => new CustomerDataPacket
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

        // Gửi trả về danh sách khách hàng cho client.
        await connection.TCP.SendAsync(new CustomersPacket()
        {
            Customers = customerPackets,
            SequenceId = packet.SequenceId
        });
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
            !IS_VALID_EMAIL(packet.Email) ||
            !IS_VALID_PHONE_NUMBER(packet.PhoneNumber))
        {
            // MALFORMED_PACKET: Packet từ client không đúng định dạng hoặc thiếu field cần thiết.
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY).ConfigureAwait(false);

            return;
        }
        // Kiểm tra email/số điện thoại đã tồn tại
        if (await s_customer.AnyAsync(c => c.Email == packet.Email || c.PhoneNumber == packet.PhoneNumber))
        {
            // ALREADY_EXISTS: Email hoặc số điện thoại đã tồn tại trong database, không cho phép trùng.
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.ALREADY_EXISTS,
                ProtocolAdvice.FIX_AND_RETRY).ConfigureAwait(false);
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
                ProtocolAdvice.NONE).ConfigureAwait(false);
        }
        catch (System.Exception)
        {
            // INTERNAL_ERROR: Lỗi hệ thống (ví dụ không ghi DB được, lỗi nền tảng).
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY).ConfigureAwait(false);
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
            !IS_VALID_EMAIL(packet.Email) ||
            !IS_VALID_PHONE_NUMBER(packet.PhoneNumber))
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
                ProtocolAdvice.DO_NOT_RETRY).ConfigureAwait(false);
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
                ProtocolAdvice.NONE).ConfigureAwait(false);
        }
        catch (System.Exception)
        {
            // INTERNAL_ERROR: Lỗi ghi database hoặc nền tảng.
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY).ConfigureAwait(false);
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
                ProtocolAdvice.NONE).ConfigureAwait(false);
        }
        catch (System.Exception)
        {
            // INTERNAL_ERROR: Lỗi nền tảng hoặc database khi xóa.
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY).ConfigureAwait(false);
        }
    }

    #region Private Methods

    /// <summary>
    /// Validates email by simple algorithm (no regex).
    /// </summary>
    public static System.Boolean IS_VALID_EMAIL(System.String email)
    {
        if (System.String.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        if (email.Contains(' '))
        {
            return false;
        }

        System.Int32 atIndex = email.IndexOf('@');
        System.Int32 dotIndex = email.LastIndexOf('.');

        // Must contain '@' and '.' after '@'
        if (atIndex <= 0)
        {
            return false; // '@' cannot be first
        }

        if (dotIndex < atIndex + 2)
        {
            return false; // '.' must be after '@' and at least one character
        }

        if (dotIndex == email.Length - 1)
        {
            return false; // '.' cannot be last
        }

        // Must only have one '@'
        if (email.IndexOf('@', atIndex + 1) != -1)
        {
            return false;
        }

        // No consecutive dots
        for (System.Int32 i = 1; i < email.Length; i++)
        {
            if (email[i] == '.' && email[i - 1] == '.')
            {
                return false;
            }
        }

        // All parts must be non-empty
        System.String local = email[..atIndex];
        System.String domain = email.Substring(atIndex + 1, dotIndex - atIndex - 1);
        System.String tld = email[(dotIndex + 1)..];

        return !System.String.IsNullOrWhiteSpace(local) && !System.String.IsNullOrWhiteSpace(domain) && !System.String.IsNullOrWhiteSpace(tld);
    }

    /// <summary>
    /// Validates Vietnam phone number by simple algorithm (no regex).
    /// </summary>
    public static System.Boolean IS_VALID_PHONE_NUMBER(System.String phone)
    {
        if (System.String.IsNullOrWhiteSpace(phone))
        {
            return false;
        }

        if (phone.Contains(' '))
        {
            return false;
        }

        // Only digits
        foreach (System.Char c in phone)
        {
            if (c is < '0' or > '9')
            {
                return false;
            }
        }

        // Length 10 hoặc 11
        if (phone.Length is not 10 and not 11)
        {
            return false;
        }

        // Must start with '0'
        if (phone[0] != '0')
        {
            return false;
        }

        System.Boolean prefixOk = false;
        System.String[] validPrefixes = ["03", "05", "07", "08", "09"];

        foreach (System.String prefix in validPrefixes)
        {
            if (phone.StartsWith(prefix)) { prefixOk = true; break; }
        }

        return prefixOk;
    }

    #endregion Private Methods
}