// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Shared.Frames;

namespace AutoX.Gara.Shared.Protocol.Suppliers;

/// <summary>
/// Packet gửi từ client lên server để truy vấn danh sách nhà cung cấp
/// có hỗ trợ phân trang, tìm kiếm, lọc theo trạng thái/điều khoản và sắp xếp.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class SupplierQueryRequest : PacketBase<SupplierQueryRequest>
{
    // ─── Fixed-size fields (đặt trước) ───────────────────────────────────────

    /// <summary>Số trang cần lấy (bắt đầu từ 1).</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32 Page { get; set; } = 1;

    /// <summary>Số bản ghi tối đa trên mỗi trang.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public System.Int32 PageSize { get; set; } = 20;

    /// <summary>
    /// Cột dùng để sắp xếp kết quả.
    /// Mặc định: <see cref="SupplierSortField.Name"/>.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public SupplierSortField SortBy { get; set; } = SupplierSortField.Name;

    /// <summary>
    /// <c>true</c> = sắp xếp giảm dần,
    /// <c>false</c> = sắp xếp tăng dần.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public System.Boolean SortDescending { get; set; } = false;

    /// <summary>
    /// Lọc theo trạng thái nhà cung cấp.
    /// <c>SupplierStatus.None</c> (mặc định) = không filter, trả về tất cả.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 5)]
    public SupplierStatus FilterStatus { get; set; } = SupplierStatus.None;

    /// <summary>
    /// Lọc theo điều khoản thanh toán.
    /// <c>PaymentTerms.None</c> (mặc định) = không filter, trả về tất cả.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 6)]
    public PaymentTerms FilterPaymentTerms { get; set; } = PaymentTerms.None;

    // ─── Dynamic-size field (đặt cuối) ───────────────────────────────────────

    /// <summary>
    /// Từ khóa tìm kiếm theo tên, email, mã số thuế hoặc ghi chú.
    /// Rỗng = không áp dụng filter.
    /// <para>
    /// Dynamic field — phải đứng cuối để <see cref="PacketBase{TSelf}.Length"/>
    /// tính đúng wire-size (UTF-8 byte count).
    /// </para>
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 7)]
    public System.String SearchTerm { get; set; } = System.String.Empty;

    // ─── Constructor ──────────────────────────────────────────────────────────

    public SupplierQueryRequest() => OpCode = OpCommand.NONE.AsUInt16();

    // ─── Pool Reset ───────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void ResetForPool()
    {
        base.ResetForPool();

        SequenceId = 0;
        Page = 1;
        PageSize = 20;
        SortBy = SupplierSortField.Name;
        SortDescending = false;
        FilterStatus = SupplierStatus.None;
        FilterPaymentTerms = PaymentTerms.None;
        SearchTerm = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}