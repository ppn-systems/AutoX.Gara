// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;

namespace AutoX.Gara.Shared.Protocol.Employees;

[SerializePackable(SerializeLayout.Explicit)]
public sealed class EmployeeSalaryDto : PacketBase<EmployeeSalaryDto>
{
    // Fixed-size fields

    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public System.Int32? EmployeeSalaryId { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public System.Int32 EmployeeId { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public System.Decimal Salary { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public SalaryType SalaryType { get; set; } = SalaryType.Monthly;

    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public System.Decimal SalaryUnit { get; set; } = 1;

    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public System.DateTime EffectiveFrom { get; set; } = System.DateTime.UtcNow;

    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public System.DateTime? EffectiveTo { get; set; }

    // Dynamic field(s)

    [SerializeOrder(PacketHeaderOffset.Region + 8)]
    public System.String Note { get; set; } = System.String.Empty;

    public EmployeeSalaryDto() => OpCode = OpCommand.NONE.AsUInt16();

    public override void ResetForPool()
    {
        base.ResetForPool();

        SequenceId = 0;
        EmployeeSalaryId = null;
        EmployeeId = 0;
        Salary = 0;
        SalaryType = SalaryType.Monthly;
        SalaryUnit = 1;
        EffectiveFrom = System.DateTime.UtcNow;
        EffectiveTo = null;
        Note = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}
