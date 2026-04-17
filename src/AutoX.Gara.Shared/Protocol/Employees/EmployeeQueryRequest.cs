using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.



using AutoX.Gara.Domain.Enums;

using AutoX.Gara.Domain.Enums.Employees;

using Nalix.Common.Networking.Protocols;

using AutoX.Gara.Shared.Extensions;

using Nalix.Common.Networking.Packets;

using Nalix.Common.Serialization;

using Nalix.Framework.DataFrames;



namespace AutoX.Gara.Shared.Protocol.Employees;



/// <summary>

/// Packet g?i t? client l�n server d? truy v?n danh s�ch nh�n vi�n

/// c� h? tr? ph�n trang, t�m ki?m, l?c theo ch?c v?/tr?ng th�i/gi?i t�nh v� s?p x?p.

/// </summary>

[SerializePackable(SerializeLayout.Explicit)]

public sealed class EmployeeQueryRequest : PacketBase<EmployeeQueryRequest>

{



    [SerializeOrder(PacketHeaderOffset.Region + 1)]

    public int Page { get; set; } = 1;



    [SerializeOrder(PacketHeaderOffset.Region + 2)]

    public int PageSize { get; set; } = 20;



    [SerializeOrder(PacketHeaderOffset.Region + 3)]

    public EmployeeSortField SortBy { get; set; } = EmployeeSortField.Name;



    [SerializeOrder(PacketHeaderOffset.Region + 4)]

    public bool SortDescending { get; set; } = false;



    [SerializeOrder(PacketHeaderOffset.Region + 5)]

    public Position FilterPosition { get; set; } = Position.None;



    [SerializeOrder(PacketHeaderOffset.Region + 6)]

    public EmploymentStatus FilterStatus { get; set; } = EmploymentStatus.None;



    [SerializeOrder(PacketHeaderOffset.Region + 7)]

    public Gender FilterGender { get; set; } = Gender.None;



    // --- Dynamic-size field ---------------------------------------------------



    [SerializeOrder(PacketHeaderOffset.Region + 8)]

    public string SearchTerm { get; set; } = string.Empty;



    // --- Constructor -----------------------------------------------------------



    public EmployeeQueryRequest() => OpCode = OpCommand.NONE.AsUInt16();



    // --- Pool Reset --------------------------------------------???---------------



    public override void ResetForPool()

    {

        base.ResetForPool();



        SequenceId = 0;

        Page = 1;

        PageSize = 20;

        SortBy = EmployeeSortField.Name;

        SortDescending = false;

        FilterPosition = Position.None;

        FilterStatus = EmploymentStatus.None;

        FilterGender = Gender.None;

        SearchTerm = string.Empty;

        OpCode = OpCommand.NONE.AsUInt16();

    }

}
