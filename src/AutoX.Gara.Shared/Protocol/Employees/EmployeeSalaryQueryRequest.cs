using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Employees;
using Nalix.Common.Networking.Protocols;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;

namespace AutoX.Gara.Shared.Protocol.Employees;

[SerializePackable(SerializeLayout.Explicit)]
public sealed class EmployeeSalaryQueryRequest : PacketBase<EmployeeSalaryQueryRequest>
{
    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public int Page { get; set; } = 1;

    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public int PageSize { get; set; } = 20;

    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public EmployeeSalarySortField SortBy { get; set; } = EmployeeSalarySortField.EffectiveFrom;

    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public bool SortDescending { get; set; } = true;

    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public int FilterEmployeeId { get; set; } = 0;

    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public SalaryType? FilterSalaryType { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public DateTime? FilterFromDate { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.Region + 8)]
    public DateTime? FilterToDate { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.Region + 9)]
    public string SearchTerm { get; set; } = string.Empty;

    public EmployeeSalaryQueryRequest() => OpCode = OpCommand.NONE.AsUInt16();

    public override void ResetForPool()
    {
        base.ResetForPool();

        SequenceId = 0;
        Page = 1;
        PageSize = 20;
        SortBy = EmployeeSalarySortField.EffectiveFrom;
        SortDescending = true;
        FilterEmployeeId = 0;
        FilterSalaryType = null;
        FilterFromDate = null;
        FilterToDate = null;
        SearchTerm = string.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}
