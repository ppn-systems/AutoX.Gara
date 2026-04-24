// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Extensions;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using System;
namespace AutoX.Gara.Contracts.Employees;
[SerializePackable(SerializeLayout.Explicit)]
public sealed class EmployeeSalaryQueryRequest : PacketBase<EmployeeSalaryQueryRequest>
{
    [SerializeOrder(0)]
    public int Page { get; set; } = 1;
    [SerializeOrder(1)]
    public int PageSize { get; set; } = 20;
    [SerializeOrder(2)]
    public EmployeeSalarySortField SortBy { get; set; } = EmployeeSalarySortField.EffectiveFrom;
    [SerializeOrder(3)]
    public bool SortDescending { get; set; } = true;
    [SerializeOrder(4)]
    public int FilterEmployeeId { get; set; } = 0;
    [SerializeOrder(5)]
    public SalaryType? FilterSalaryType { get; set; } = null;
    [SerializeOrder(6)]
    public DateTime? FilterFromDate { get; set; } = null;
    [SerializeOrder(7)]
    public DateTime? FilterToDate { get; set; } = null;
    [SerializeOrder(8)]
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



