// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Parts;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Common.Abstractions;
using Nalix.Framework.DataFrames;

namespace AutoX.Gara.Shared.Protocol.Inventory;

// -----------------------------------------------------------------------------
// PART QUERY REQUEST
// -----------------------------------------------------------------------------

/// <summary>
/// Packet g?i t? client lên server d? truy v?n danh sách ph? tùng
/// có h? tr? phân trang, tìm ki?m, l?c và s?p x?p.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class PartQueryRequest : PacketBase<PartQueryRequest>
{
    // --- Fixed-size fields ----------------------------------------------------

    /// <summary>S? trang c?n l?y (b?t d?u t? 1).</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public System.Int32 Page { get; set; } = 1;

    /// <summary>S? b?n ghi t?i da trên m?i trang.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public System.Int32 PageSize { get; set; } = 20;

    /// <summary>C?t dùng d? s?p x?p k?t qu?.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public PartSortField SortBy { get; set; } = PartSortField.PartName;

    /// <summary><c>true</c> = gi?m d?n, <c>false</c> = tang d?n.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public System.Boolean SortDescending { get; set; } = false;

    /// <summary>
    /// L?c theo nhà cung c?p.
    /// <c>0</c> (m?c d?nh) = không filter.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public System.Int32 FilterSupplierId { get; set; } = 0;

    /// <summary>
    /// L?c theo lo?i ph? tùng.
    /// <c>null</c> = không filter.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public PartCategory? FilterCategory { get; set; } = null;

    /// <summary>
    /// L?c theo tr?ng thái t?n kho.
    /// <c>null</c> = không filter.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public System.Boolean? FilterInStock { get; set; } = null;

    /// <summary>
    /// L?c theo tr?ng thái l?i.
    /// <c>null</c> = không filter.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 8)]
    public System.Boolean? FilterDefective { get; set; } = null;

    /// <summary>
    /// L?c theo tr?ng thái h?t h?n.
    /// <c>null</c> = không filter.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 9)]
    public System.Boolean? FilterExpired { get; set; } = null;

    /// <summary>
    /// L?c theo tr?ng thái ng?ng bán.
    /// <c>null</c> = không filter.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 10)]
    public System.Boolean? FilterDiscontinued { get; set; } = null;

    // --- Dynamic-size field ---------------------------------------------------

    /// <summary>
    /// T? khóa tìm ki?m theo tên, mã SKU ho?c nhà s?n xu?t.
    /// R?ng = không áp d?ng filter.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 11)]
    public System.String SearchTerm { get; set; } = System.String.Empty;

    // --- Constructor ---------------------------------------------------------

    public PartQueryRequest() => OpCode = OpCommand.NONE.AsUInt16();

    // --- Pool Reset -----------------------------------------------------------

    /// <inheritdoc/>
    public override void ResetForPool()
    {
        base.ResetForPool();

        SequenceId = 0;
        Page = 1;
        PageSize = 20;
        SortBy = PartSortField.PartName;
        SortDescending = false;
        FilterSupplierId = 0;
        FilterCategory = null;
        FilterInStock = null;
        FilterDefective = null;
        FilterExpired = null;
        FilterDiscontinued = null;
        SearchTerm = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}