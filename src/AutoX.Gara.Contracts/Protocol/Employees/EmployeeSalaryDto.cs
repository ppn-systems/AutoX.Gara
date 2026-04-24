// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Extensions;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using System;
namespace AutoX.Gara.Contracts.Employees;
[SerializePackable(SerializeLayout.Explicit)]
public sealed class EmployeeSalaryDto : PacketBase<EmployeeSalaryDto>
{
    // Fixed-size fields
    [SerializeOrder(0)]
    public int? EmployeeSalaryId { get; set; }
    [SerializeOrder(1)]
    public int EmployeeId { get; set; }
    [SerializeOrder(2)]
    public decimal Salary { get; set; }
    [SerializeOrder(3)]
    public SalaryType SalaryType { get; set; } = SalaryType.Monthly;
    [SerializeOrder(4)]
    public decimal SalaryUnit { get; set; } = 1;
    [SerializeOrder(5)]
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    [SerializeOrder(6)]
    public DateTime? EffectiveTo { get; set; }
    // Dynamic field(s)
    [SerializeOrder(7)]
    public string Note { get; set; } = string.Empty;
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
        EffectiveFrom = DateTime.UtcNow;
        EffectiveTo = null;
        Note = string.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}



