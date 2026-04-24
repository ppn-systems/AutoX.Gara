// Copyright (c) 2026 PPN Corporation. All rights reserved.



using AutoX.Gara.Domain.Enums.Customers;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;



namespace AutoX.Gara.Shared.Protocol.Customers;



/// <summary>

/// Packet g?i t? client l�n server d? truy v?n danh s�ch kh�ch h�ng

/// c� h? tr? ph�n trang, t�m ki?m, l?c v� s?p x?p.

/// </summary>

[SerializePackable(SerializeLayout.Explicit)]

public sealed class CustomerQueryRequest : PacketBase<CustomerQueryRequest>

{

    // --- Fixed-size fields (d?t tru?c) ---------------------------------------

    // T?t c? field c? d?nh k�ch thu?c ph?i d?ng tru?c dynamic field (SearchTerm)

    // d? PacketBase t�nh Length ch�nh x�c.


    /// <summary>S? trang c?n l?y (b?t d?u t? 1).</summary>

    [SerializeOrder(PacketHeaderOffset.Region + 1)]

    public int Page { get; set; } = 1;



    /// <summary>S? b?n ghi t?i da tr�n m?i trang.</summary>

    [SerializeOrder(PacketHeaderOffset.Region + 2)]

    public int PageSize { get; set; } = 20;



    /// <summary>

    /// C?t d�ng d? s?p x?p k?t qu?.

    /// M?c d?nh: <see cref="CustomerSortField.CreatedAt"/>.

    /// </summary>

    [SerializeOrder(PacketHeaderOffset.Region + 3)]

    public CustomerSortField SortBy { get; set; } = CustomerSortField.CreatedAt;



    /// <summary>

    /// <c>true</c> = s?p x?p gi?m d?n (m?i nh?t l�n d?u),

    /// <c>false</c> = s?p x?p tang d?n.

    /// </summary>

    [SerializeOrder(PacketHeaderOffset.Region + 4)]

    public bool SortDescending { get; set; } = true;



    /// <summary>

    /// L?c theo lo?i kh�ch h�ng.

    /// <c>CustomerType.None</c> (m?c d?nh) = kh�ng filter, tr? v? t?t c?.

    /// </summary>

    [SerializeOrder(PacketHeaderOffset.Region + 5)]

    public CustomerType FilterType { get; set; } = CustomerType.None;



    /// <summary>

    /// L?c theo h?ng th�nh vi�n.

    /// <c>MembershipLevel.None</c> (m?c d?nh) = kh�ng filter, tr? v? t?t c?.

    /// </summary>

    [SerializeOrder(PacketHeaderOffset.Region + 6)]

    public MembershipLevel FilterMembership { get; set; } = MembershipLevel.None;



    // --- Dynamic-size field (d?t cu?i) ---------------------------------------



    /// <summary>

    /// T? kh�a t�m ki?m theo t�n, email, S�T ho?c ghi ch� n?i b?.

    /// R?ng = kh�ng �p d?ng filter.

    /// <para>

    /// Dynamic field ? ph?i d?ng cu?i d? <see cref="PacketBase{TSelf}.Length"/>

    /// t�nh d�ng wire-size (UTF-8 byte count).

    /// </para>

    /// </summary>

    [SerializeOrder(PacketHeaderOffset.Region + 7)]

    public string SearchTerm { get; set; } = string.Empty;



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

        SearchTerm = string.Empty;

        OpCode = OpCommand.NONE.AsUInt16();

    }

}
