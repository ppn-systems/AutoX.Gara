using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using Nalix.Common.Networking.Protocols;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;

namespace AutoX.Gara.Shared.Protocol.Suppliers;

/// <summary>
/// Packet g?i t? client l�n server d? truy v?n danh s�ch nh� cung c?p
/// c� h? tr? ph�n trang, t�m ki?m, l?c theo tr?ng th�i/di?u kho?n v� s?p x?p.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class SupplierQueryRequest : PacketBase<SupplierQueryRequest>
{
    // --- Fixed-size fields (d?t tru?c) ---------------------------------------

    /// <summary>S? trang c?n l?y (b?t d?u t? 1).</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public int Page { get; set; } = 1;

    /// <summary>S? b?n ghi t?i da tr�n m?i trang.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// C?t d�ng d? s?p x?p k?t qu?.
    /// M?c d?nh: <see cref="SupplierSortField.Name"/>.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public SupplierSortField SortBy { get; set; } = SupplierSortField.Name;

    /// <summary>
    /// <c>true</c> = s?p x?p gi?m d?n,
    /// <c>false</c> = s?p x?p tang d?n.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public bool SortDescending { get; set; } = false;

    /// <summary>
    /// L?c theo tr?ng th�i nh� cung c?p.
    /// <c>SupplierStatus.None</c> (m?c d?nh) = kh�ng filter, tr? v? t?t c?.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public SupplierStatus FilterStatus { get; set; } = SupplierStatus.None;

    /// <summary>
    /// L?c theo di?u kho?n thanh to�n.
    /// <c>PaymentTerms.None</c> (m?c d?nh) = kh�ng filter, tr? v? t?t c?.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public PaymentTerms FilterPaymentTerms { get; set; } = PaymentTerms.None;

    // --- Dynamic-size field (d?t cu?i) ---------------------------------------

    /// <summary>
    /// T? kh�a t�m ki?m theo t�n, email, m� s? thu? ho?c ghi ch�.
    /// R?ng = kh�ng �p d?ng filter.
    /// <para>
    /// Dynamic field ? ph?i d?ng cu?i d? <see cref="PacketBase{TSelf}.Length"/>
    /// t�nh d�ng wire-size (UTF-8 byte count).
    /// </para>
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public string SearchTerm { get; set; } = string.Empty;

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
        SearchTerm = string.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}