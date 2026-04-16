// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Customers;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Common.Abstractions;
using Nalix.Framework.DataFrames;

namespace AutoX.Gara.Shared.Protocol.Customers;

/// <summary>
/// Packet g?i t? client lên server d? truy v?n danh sách khách hàng
/// có h? tr? phân trang, tìm ki?m, l?c và s?p x?p.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class CustomerQueryRequest : PacketBase<CustomerQueryRequest>
{
    // --- Fixed-size fields (d?t tru?c) ---------------------------------------
    // T?t c? field c? d?nh kích thu?c ph?i d?ng tru?c dynamic field (SearchTerm)
    // d? PacketBase tính Length chính xác.

    /// <summary>S? trang c?n l?y (b?t d?u t? 1).</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public System.Int32 Page { get; set; } = 1;

    /// <summary>S? b?n ghi t?i da trên m?i trang.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public System.Int32 PageSize { get; set; } = 20;

    /// <summary>
    /// C?t dùng d? s?p x?p k?t qu?.
    /// M?c d?nh: <see cref="CustomerSortField.CreatedAt"/>.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public CustomerSortField SortBy { get; set; } = CustomerSortField.CreatedAt;

    /// <summary>
    /// <c>true</c> = s?p x?p gi?m d?n (m?i nh?t lên d?u),
    /// <c>false</c> = s?p x?p tang d?n.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public System.Boolean SortDescending { get; set; } = true;

    /// <summary>
    /// L?c theo lo?i khách hàng.
    /// <c>CustomerType.None</c> (m?c d?nh) = không filter, tr? v? t?t c?.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public CustomerType FilterType { get; set; } = CustomerType.None;

    /// <summary>
    /// L?c theo h?ng thành viên.
    /// <c>MembershipLevel.None</c> (m?c d?nh) = không filter, tr? v? t?t c?.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public MembershipLevel FilterMembership { get; set; } = MembershipLevel.None;

    // --- Dynamic-size field (d?t cu?i) ---------------------------------------

    /// <summary>
    /// T? khóa tìm ki?m theo tên, email, SÐT ho?c ghi chú n?i b?.
    /// R?ng = không áp d?ng filter.
    /// <para>
    /// Dynamic field — ph?i d?ng cu?i d? <see cref="PacketBase{TSelf}.Length"/>
    /// tính dúng wire-size (UTF-8 byte count).
    /// </para>
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public System.String SearchTerm { get; set; } = System.String.Empty;

    // --- Constructor ---------------------------------------------------------

    public CustomerQueryRequest() => OpCode = OpCommand.NONE.AsUInt16();

    // --- Pool Reset -----------------------------------------------------------

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