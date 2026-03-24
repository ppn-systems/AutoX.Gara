// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Customers;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Common.Shared;
using Nalix.Shared.Frames;

namespace AutoX.Gara.Shared.Protocol.Customers;

/// <summary>
/// Packet gửi từ client lên server để truy vấn danh sách khách hàng
/// có hỗ trợ phân trang, tìm kiếm, lọc và sắp xếp.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class CustomerQueryRequest : PacketBase<CustomerQueryRequest>, IPoolable, IPacketSequenced
{
    // ─── Fixed-size fields (đặt trước) ───────────────────────────────────────
    // Tất cả field cố định kích thước phải đứng trước dynamic field (SearchTerm)
    // để PacketBase tính Length chính xác.

    /// <inheritdoc/>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION)]
    public new System.UInt32 SequenceId { get; set; }

    /// <summary>Số trang cần lấy (bắt đầu từ 1).</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32 Page { get; set; } = 1;

    /// <summary>Số bản ghi tối đa trên mỗi trang.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public System.Int32 PageSize { get; set; } = 20;

    /// <summary>
    /// Cột dùng để sắp xếp kết quả.
    /// Mặc định: <see cref="CustomerSortField.CreatedAt"/>.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public CustomerSortField SortBy { get; set; } = CustomerSortField.CreatedAt;

    /// <summary>
    /// <c>true</c> = sắp xếp giảm dần (mới nhất lên đầu),
    /// <c>false</c> = sắp xếp tăng dần.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public System.Boolean SortDescending { get; set; } = true;

    /// <summary>
    /// Lọc theo loại khách hàng.
    /// <c>CustomerType.None</c> (mặc định) = không filter, trả về tất cả.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 5)]
    public CustomerType FilterType { get; set; } = CustomerType.None;

    /// <summary>
    /// Lọc theo hạng thành viên.
    /// <c>MembershipLevel.None</c> (mặc định) = không filter, trả về tất cả.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 6)]
    public MembershipLevel FilterMembership { get; set; } = MembershipLevel.None;

    // ─── Dynamic-size field (đặt cuối) ───────────────────────────────────────

    /// <summary>
    /// Từ khóa tìm kiếm theo tên, email, SĐT hoặc ghi chú nội bộ.
    /// Rỗng = không áp dụng filter.
    /// <para>
    /// Dynamic field — phải đứng cuối để <see cref="PacketBase{TSelf}.Length"/>
    /// tính đúng wire-size (UTF-8 byte count).
    /// </para>
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 7)]
    public System.String SearchTerm { get; set; } = System.String.Empty;

    // ─── Constructor ─────────────────────────────────────────────────────────

    public CustomerQueryRequest() => OpCode = OpCommand.NONE.AsUInt16();

    // ─── Pool Reset ───────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void ResetForPool()
    {
        base.ResetForPool();

        SequenceId = 0;
        Page = 1;
        PageSize = 20;
        SortBy = CustomerSortField.CreatedAt;
        SortDescending = true;
        FilterType = CustomerType.None;
        FilterMembership = MembershipLevel.None;
        SearchTerm = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}