// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Shared.Frames;

namespace AutoX.Gara.Shared.Protocol.Employees;

[SerializePackable(SerializeLayout.Explicit)]
public sealed class EmployeeSalaryQueryRequest : PacketBase<EmployeeSalaryQueryRequest>
{
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32 Page { get; set; } = 1;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public System.Int32 PageSize { get; set; } = 20;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public EmployeeSalarySortField SortBy { get; set; } = EmployeeSalarySortField.EffectiveFrom;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public System.Boolean SortDescending { get; set; } = true;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 5)]
    public System.Int32 FilterEmployeeId { get; set; } = 0;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 6)]
    public SalaryType? FilterSalaryType { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 7)]
    public System.DateTime? FilterFromDate { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 8)]
    public System.DateTime? FilterToDate { get; set; } = null;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 9)]
    public System.String SearchTerm { get; set; } = System.String.Empty;

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
        SearchTerm = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}

