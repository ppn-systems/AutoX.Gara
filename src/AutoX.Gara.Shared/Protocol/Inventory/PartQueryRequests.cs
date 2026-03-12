// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Parts;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Caching;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Shared.Frames;

namespace AutoX.Gara.Shared.Protocol.Inventory;

// ═════════════════════════════════════════════════════════════════════════════
// SPARE PART QUERY REQUEST
// ═════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Packet gửi từ client lên server để truy vấn danh sách phụ tùng bán
/// có hỗ trợ phân trang, tìm kiếm, lọc và sắp xếp.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class SparePartQueryRequest : PacketBase<SparePartQueryRequest>, IPoolable, IPacketSequenced
{
    // ─── Fixed-size fields (đặt trước) ───────────────────────────────────────

    /// <inheritdoc/>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION)]
    public System.UInt32 SequenceId { get; set; }

    /// <summary>Số trang cần lấy (bắt đầu từ 1).</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32 Page { get; set; } = 1;

    /// <summary>Số bản ghi tối đa trên mỗi trang.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public System.Int32 PageSize { get; set; } = 20;

    /// <summary>Cột dùng để sắp xếp kết quả.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public SparePartSortField SortBy { get; set; } = SparePartSortField.PartName;

    /// <summary><c>true</c> = giảm dần, <c>false</c> = tăng dần.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public System.Boolean SortDescending { get; set; } = false;

    /// <summary>
    /// Lọc theo nhà cung cấp.
    /// <c>0</c> (mặc định) = không filter.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 5)]
    public System.Int32 FilterSupplierId { get; set; } = 0;

    /// <summary>
    /// Lọc theo loại phụ tùng.
    /// <c>null</c> = không filter.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 6)]
    public PartCategory? FilterCategory { get; set; } = null;

    /// <summary>
    /// Lọc theo trạng thái ngừng bán.
    /// <c>null</c> = trả về tất cả.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 7)]
    public System.Boolean? FilterDiscontinued { get; set; } = null;

    // ─── Dynamic-size field (đặt cuối) ───────────────────────────────────────

    /// <summary>
    /// Từ khóa tìm kiếm theo tên phụ tùng.
    /// Rỗng = không áp dụng filter.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 8)]
    public System.String SearchTerm { get; set; } = System.String.Empty;

    // ─── Constructor ─────────────────────────────────────────────────────────

    public SparePartQueryRequest() => OpCode = OpCommand.NONE.AsUInt16();

    // ─── Pool Reset ───────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void ResetForPool()
    {
        base.ResetForPool();

        SequenceId = 0;
        Page = 1;
        PageSize = 20;
        SortBy = SparePartSortField.PartName;
        SortDescending = false;
        FilterSupplierId = 0;
        FilterCategory = null;
        FilterDiscontinued = null;
        SearchTerm = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}

// ═════════════════════════════════════════════════════════════════════════════
// REPLACEMENT PART QUERY REQUEST
// ═════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Packet gửi từ client lên server để truy vấn danh sách phụ tùng kho
/// có hỗ trợ phân trang, tìm kiếm, lọc và sắp xếp.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class ReplacementPartQueryRequest : PacketBase<ReplacementPartQueryRequest>, IPoolable, IPacketSequenced
{
    // ─── Fixed-size fields (đặt trước) ───────────────────────────────────────

    /// <inheritdoc/>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION)]
    public System.UInt32 SequenceId { get; set; }

    /// <summary>Số trang cần lấy (bắt đầu từ 1).</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32 Page { get; set; } = 1;

    /// <summary>Số bản ghi tối đa trên mỗi trang.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public System.Int32 PageSize { get; set; } = 20;

    /// <summary>Cột dùng để sắp xếp kết quả.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public ReplacementPartSortField SortBy { get; set; } = ReplacementPartSortField.DateAdded;

    /// <summary><c>true</c> = giảm dần (mới nhất lên đầu), <c>false</c> = tăng dần.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public System.Boolean SortDescending { get; set; } = true;

    /// <summary>
    /// Lọc theo trạng thái tồn kho.
    /// <c>null</c> = không filter.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 5)]
    public System.Boolean? FilterInStock { get; set; } = null;

    /// <summary>
    /// Lọc theo trạng thái lỗi.
    /// <c>null</c> = không filter.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 6)]
    public System.Boolean? FilterDefective { get; set; } = null;

    /// <summary>
    /// Lọc theo trạng thái hết hạn.
    /// <c>null</c> = không filter.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 7)]
    public System.Boolean? FilterExpired { get; set; } = null;

    // ─── Dynamic-size field (đặt cuối) ───────────────────────────────────────

    /// <summary>
    /// Từ khóa tìm kiếm theo tên, mã SKU hoặc nhà sản xuất.
    /// Rỗng = không áp dụng filter.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 8)]
    public System.String SearchTerm { get; set; } = System.String.Empty;

    // ─── Constructor ─────────────────────────────────────────────────────────

    public ReplacementPartQueryRequest() => OpCode = OpCommand.NONE.AsUInt16();

    // ─── Pool Reset ───────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void ResetForPool()
    {
        base.ResetForPool();

        SequenceId = 0;
        Page = 1;
        PageSize = 20;
        SortBy = ReplacementPartSortField.DateAdded;
        SortDescending = true;
        FilterInStock = null;
        FilterDefective = null;
        FilterExpired = null;
        SearchTerm = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}
