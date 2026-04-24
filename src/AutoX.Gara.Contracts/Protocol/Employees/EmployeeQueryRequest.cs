// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Extensions;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
namespace AutoX.Gara.Contracts.Employees;
/// <summary>
/// Packet g?i t? client l�n server d? truy v?n danh s�ch nh�n vi�n
/// c� h? tr? ph�n trang, t�m ki?m, l?c theo ch?c v?/tr?ng th�i/gi?i t�nh v� s?p x?p.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class EmployeeQueryRequest : PacketBase<EmployeeQueryRequest>
{
    [SerializeOrder(0)]
    public int Page { get; set; } = 1;
    [SerializeOrder(1)]
    public int PageSize { get; set; } = 20;
    [SerializeOrder(2)]
    public EmployeeSortField SortBy { get; set; } = EmployeeSortField.Name;
    [SerializeOrder(3)]
    public bool SortDescending { get; set; } = false;
    [SerializeOrder(4)]
    public Position FilterPosition { get; set; } = Position.None;
    [SerializeOrder(5)]
    public EmploymentStatus FilterStatus { get; set; } = EmploymentStatus.None;
    [SerializeOrder(6)]
    public Gender FilterGender { get; set; } = Gender.None;
    // --- Dynamic-size field ---------------------------------------------------
    [SerializeOrder(7)]
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



