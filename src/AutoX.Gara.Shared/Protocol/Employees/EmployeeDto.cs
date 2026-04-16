// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;

namespace AutoX.Gara.Shared.Protocol.Employees;

/// <summary>
/// Packet mang d? li?u nhân viên, dùng cho các thao tác Create, Update, Query.
/// <para>
/// Fixed-size fields d?t TRU?C dynamic string fields d? tính dúng wire-size.
/// </para>
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class EmployeeDto : PacketBase<EmployeeDto>
{
    // --- Fixed-size fields ----------------------------------------------------

    /// <summary>ID nhân viên. Null khi t?o m?i.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public System.Int32? EmployeeId { get; set; }

    /// <summary>Gi?i tính.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public Gender? Gender { get; set; }

    /// <summary>Ch?c v?.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public Position? Position { get; set; }

    /// <summary>Tr?ng thái làm vi?c.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public EmploymentStatus? Status { get; set; }

    /// <summary>Ngày sinh.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public System.DateTime? DateOfBirth { get; set; }

    /// <summary>Ngày b?t d?u làm vi?c.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public System.DateTime? StartDate { get; set; }

    /// <summary>Ngày k?t thúc h?p d?ng.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public System.DateTime? EndDate { get; set; }

    // --- Dynamic-size fields --------------------------------------------------

    /// <summary>Tên nhân viên.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 8)]
    public System.String Name { get; set; }

    /// <summary>Ð?a ch? nhân viên.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 9)]
    public System.String Address { get; set; }

    /// <summary>S? di?n tho?i nhân viên.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 10)]
    public System.String PhoneNumber { get; set; }

    /// <summary>Email nhân viên.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 11)]
    public System.String Email { get; set; }

    // --- Constructor ----------------------------------------------------------

    public EmployeeDto()
    {
        Name = System.String.Empty;
        Address = System.String.Empty;
        PhoneNumber = System.String.Empty;
        Email = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }

    // --- Pool Reset -----------------------------------------------------------

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