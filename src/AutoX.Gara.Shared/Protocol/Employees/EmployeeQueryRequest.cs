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
/// Packet gửi từ client lên server để truy vấn danh sách nhân viên
/// có hỗ trợ phân trang, tìm kiếm, lọc theo chức vụ/trạng thái/giới tính và sắp xếp.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class EmployeeQueryRequest : PacketBase<EmployeeQueryRequest>
{

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32 Page { get; set; } = 1;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public System.Int32 PageSize { get; set; } = 20;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public EmployeeSortField SortBy { get; set; } = EmployeeSortField.Name;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public System.Boolean SortDescending { get; set; } = false;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 5)]
    public Position FilterPosition { get; set; } = Position.None;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 6)]
    public EmploymentStatus FilterStatus { get; set; } = EmploymentStatus.None;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 7)]
    public Gender FilterGender { get; set; } = Gender.None;

    // ─── Dynamic-size field ───────────────────────────────────────────────────

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 8)]
    public System.String SearchTerm { get; set; } = System.String.Empty;

    // ─── Constructor ───────────────────────────────────────────────────────────

    public EmployeeQueryRequest() => OpCode = OpCommand.NONE.AsUInt16();

    // ─── Pool Reset ────────────────────────────────────────────���───────────────

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
        SearchTerm = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}