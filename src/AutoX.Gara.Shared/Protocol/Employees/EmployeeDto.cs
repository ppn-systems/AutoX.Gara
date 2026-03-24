// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Shared.Frames;

namespace AutoX.Gara.Shared.Protocol.Employees;

/// <summary>
/// Packet mang dữ liệu nhân viên, dùng cho các thao tác Create, Update, Query.
/// <para>
/// Fixed-size fields đặt TRƯỚC dynamic string fields để tính đúng wire-size.
/// </para>
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class EmployeeDto : PacketBase<EmployeeDto>
{
    // ─── Fixed-size fields ────────────────────────────────────────────────────

    /// <summary>ID nhân viên. Null khi tạo mới.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32? EmployeeId { get; set; }

    /// <summary>Giới tính.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public Gender? Gender { get; set; }

    /// <summary>Chức vụ.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public Position? Position { get; set; }

    /// <summary>Trạng thái làm việc.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public EmploymentStatus? Status { get; set; }

    /// <summary>Ngày sinh.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 5)]
    public System.DateTime? DateOfBirth { get; set; }

    /// <summary>Ngày bắt đầu làm việc.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 6)]
    public System.DateTime? StartDate { get; set; }

    /// <summary>Ngày kết thúc hợp đồng.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 7)]
    public System.DateTime? EndDate { get; set; }

    // ─── Dynamic-size fields ──────────────────────────────────────────────────

    /// <summary>Tên nhân viên.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 8)]
    public System.String Name { get; set; }

    /// <summary>Địa chỉ nhân viên.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 9)]
    public System.String Address { get; set; }

    /// <summary>Số điện thoại nhân viên.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 10)]
    public System.String PhoneNumber { get; set; }

    /// <summary>Email nhân viên.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 11)]
    public System.String Email { get; set; }

    // ─── Constructor ──────────────────────────────────────────────────────────

    public EmployeeDto()
    {
        Name = System.String.Empty;
        Address = System.String.Empty;
        PhoneNumber = System.String.Empty;
        Email = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }

    // ─── Pool Reset ───────────────────────────────────────────────────────────

    public override void ResetForPool()
    {
        base.ResetForPool();

        SequenceId = 0;
        EmployeeId = null;
        Gender = null;
        Position = null;
        Status = null;
        DateOfBirth = null;
        StartDate = null;
        EndDate = null;
        Name = System.String.Empty;
        Address = System.String.Empty;
        PhoneNumber = System.String.Empty;
        Email = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}