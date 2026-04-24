// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Parts;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
namespace AutoX.Gara.Shared.Protocol.Inventory;
// -----------------------------------------------------------------------------
// PART QUERY REQUEST
// -----------------------------------------------------------------------------
/// <summary>
/// Packet g?i t? client l�n server d? truy v?n danh s�ch ph? t�ng
/// c� h? tr? ph�n trang, t�m ki?m, l?c v� s?p x?p.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class PartQueryRequest : PacketBase<PartQueryRequest>
{
    // --- Fixed-size fields ----------------------------------------------------
    /// <summary>S? trang c?n l?y (b?t d?u t? 1).</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public int Page { get; set; } = 1;
    /// <summary>S? b?n ghi t?i da tr�n m?i trang.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public int PageSize { get; set; } = 20;
    /// <summary>C?t d�ng d? s?p x?p k?t qu?.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public PartSortField SortBy { get; set; } = PartSortField.PartName;
    /// <summary><c>true</c> = gi?m d?n, <c>false</c> = tang d?n.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public bool SortDescending { get; set; } = false;
    /// <summary>
    /// L?c theo nh� cung c?p.
    /// <c>0</c> (m?c d?nh) = kh�ng filter.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public int FilterSupplierId { get; set; } = 0;
    /// <summary>
    /// L?c theo lo?i ph? t�ng.
    /// <c>null</c> = kh�ng filter.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public PartCategory? FilterCategory { get; set; } = null;
    /// <summary>
    /// L?c theo tr?ng th�i t?n kho.
    /// <c>null</c> = kh�ng filter.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public bool? FilterInStock { get; set; } = null;
    /// <summary>
    /// L?c theo tr?ng th�i l?i.
    /// <c>null</c> = kh�ng filter.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 8)]
    public bool? FilterDefective { get; set; } = null;
    /// <summary>
    /// L?c theo tr?ng th�i h?t h?n.
    /// <c>null</c> = kh�ng filter.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 9)]
    public bool? FilterExpired { get; set; } = null;
    /// <summary>
    /// L?c theo tr?ng th�i ng?ng b�n.
    /// <c>null</c> = kh�ng filter.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 10)]
    public bool? FilterDiscontinued { get; set; } = null;
    // --- Dynamic-size field ---------------------------------------------------
    /// <summary>
    /// T? kh�a t�m ki?m theo t�n, m� SKU ho?c nh� s?n xu?t.
    /// R?ng = kh�ng �p d?ng filter.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 11)]
    public string SearchTerm { get; set; } = string.Empty;
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
        SearchTerm = string.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}
