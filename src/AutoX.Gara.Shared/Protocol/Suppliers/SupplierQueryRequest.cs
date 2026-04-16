// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;

namespace AutoX.Gara.Shared.Protocol.Suppliers;

/// <summary>
/// Packet g?i t? client lên server d? truy v?n danh sách nhà cung c?p
/// có h? tr? phân trang, tìm ki?m, l?c theo tr?ng thái/di?u kho?n và s?p x?p.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class SupplierQueryRequest : PacketBase<SupplierQueryRequest>
{
    // --- Fixed-size fields (d?t tru?c) ---------------------------------------

    /// <summary>S? trang c?n l?y (b?t d?u t? 1).</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public System.Int32 Page { get; set; } = 1;

    /// <summary>S? b?n ghi t?i da trên m?i trang.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public System.Int32 PageSize { get; set; } = 20;

    /// <summary>
    /// C?t dùng d? s?p x?p k?t qu?.
    /// M?c d?nh: <see cref="SupplierSortField.Name"/>.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public SupplierSortField SortBy { get; set; } = SupplierSortField.Name;

    /// <summary>
    /// <c>true</c> = s?p x?p gi?m d?n,
    /// <c>false</c> = s?p x?p tang d?n.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public System.Boolean SortDescending { get; set; } = false;

    /// <summary>
    /// L?c theo tr?ng thái nhà cung c?p.
    /// <c>SupplierStatus.None</c> (m?c d?nh) = không filter, tr? v? t?t c?.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public SupplierStatus FilterStatus { get; set; } = SupplierStatus.None;

    /// <summary>
    /// L?c theo di?u kho?n thanh toán.
    /// <c>PaymentTerms.None</c> (m?c d?nh) = không filter, tr? v? t?t c?.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public PaymentTerms FilterPaymentTerms { get; set; } = PaymentTerms.None;

    // --- Dynamic-size field (d?t cu?i) ---------------------------------------

    /// <summary>
    /// T? khóa tìm ki?m theo tên, email, mã s? thu? ho?c ghi chú.
    /// R?ng = không áp d?ng filter.
    /// <para>
    /// Dynamic field — ph?i d?ng cu?i d? <see cref="PacketBase{TSelf}.Length"/>
    /// tính dúng wire-size (UTF-8 byte count).
    /// </para>
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public System.String SearchTerm { get; set; } = System.String.Empty;

    // --- Constructor ----------------------------------------------------------

    public SupplierQueryRequest() => OpCode = OpCommand.NONE.AsUInt16();

    // --- Pool Reset -----------------------------------------------------------

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