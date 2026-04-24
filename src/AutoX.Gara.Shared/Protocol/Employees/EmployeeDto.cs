// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using System;
namespace AutoX.Gara.Shared.Protocol.Employees;
/// <summary>
/// Packet mang dữ liệu nh�n vi�n, d�ng cho c�c thao t�c Create, Update, Query.
/// <para>
/// Fixed-size fields d?t TRU?C dynamic string fields d? t�nh d�ng wire-size.
/// </para>
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class EmployeeDto : PacketBase<EmployeeDto>
{
    // --- Fixed-size fields ----------------------------------------------------
    /// <summary>ID nh�n vi�n. Null khi t?o m?i.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public int? EmployeeId { get; set; }
    /// <summary>Gi?i t�nh.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public Gender? Gender { get; set; }
    /// <summary>Ch?c v?.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public Position? Position { get; set; }
    /// <summary>Tr?ng th�i l�m vi?c.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public EmploymentStatus? Status { get; set; }
    /// <summary>Ng�y sinh.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public DateTime? DateOfBirth { get; set; }
    /// <summary>Ng�y b?t d?u l�m vi?c.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public DateTime? StartDate { get; set; }
    /// <summary>Ng�y k?t th�c h?p d?ng.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public DateTime? EndDate { get; set; }
    // --- Dynamic-size fields --------------------------------------------------
    /// <summary>T�n nh�n vi�n.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 8)]
    public string Name { get; set; }
    /// <summary>�?a ch? nh�n vi�n.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 9)]
    public string Address { get; set; }
    /// <summary>S? di?n tho?i nh�n vi�n.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 10)]
    public string PhoneNumber { get; set; }
    /// <summary>Email nh�n vi�n.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 11)]
    public string Email { get; set; }
    // --- Constructor ----------------------------------------------------------
    public EmployeeDto()
    {
        Name = string.Empty;
        Address = string.Empty;
        PhoneNumber = string.Empty;
        Email = string.Empty;
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
        Name = string.Empty;
        Address = string.Empty;
        PhoneNumber = string.Empty;
        Email = string.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}
