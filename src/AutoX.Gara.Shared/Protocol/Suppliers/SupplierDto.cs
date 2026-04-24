// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using System;
namespace AutoX.Gara.Shared.Protocol.Suppliers;
/// <summary>
/// Packet mang dữ liệu nhà cung c?p, dùng cho các thao tác Create, Update và Query.
/// <para>
/// Fixed-size fields (enum, int, DateTime) d?t TRU?C dynamic string fields
/// d? <see cref="PacketBase{TSelf}.Length"/> tính dúng wire-size.
/// </para>
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class SupplierDto : PacketBase<SupplierDto>
{
    // --- Fixed-size fields ----------------------------------------------------
    /// <summary>
    /// ID nhà cung c?p. <c>null</c> khi t?o m?i.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public int? SupplierId { get; set; }
    /// <summary>Tr?ng thái nhà cung c?p.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public SupplierStatus? Status { get; set; }
    /// <summary>Ði?u kho?n thanh toán.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public PaymentTerms? PaymentTerms { get; set; }
    /// <summary>Ngày b?t d?u h?p tác.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public DateTime? ContractStartDate { get; set; }
    /// <summary>Ngày k?t thúc h?p tác (n?u có).</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public DateTime? ContractEndDate { get; set; }
    // --- Dynamic-size fields --------------------------------------------------
    /// <summary>Tên nhà cung c?p.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public string Name { get; set; }
    /// <summary>Email nhà cung c?p.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public string Email { get; set; }
    /// <summary>Ð?a ch? nhà cung c?p.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 8)]
    public string Address { get; set; }
    /// <summary>Mã s? thu?.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 9)]
    public string TaxCode { get; set; }
    /// <summary>Tài kho?n ngân hàng.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 10)]
    public string BankAccount { get; set; }
    /// <summary>
    /// Danh sách SÐT liên h?, phân cách b?ng d?u ph?y.
    /// VD: "0901234567,0912345678"
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 11)]
    public string PhoneNumbers { get; set; }
    /// <summary>Ghi chú n?i b?. T?i da 500 ký t?.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 12)]
    public string Notes { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    // --- Constructor ----------------------------------------------------------
    /// <summary>Kh?i t?o <see cref="SupplierDto"/> v?i giá tr? m?c d?nh r?ng.</summary>
    public SupplierDto()
    {
        Name = string.Empty;
        Email = string.Empty;
        Address = string.Empty;
        TaxCode = string.Empty;
        BankAccount = string.Empty;
        PhoneNumbers = string.Empty;
        Notes = string.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
    // --- Pool Reset -----------------------------------------------------------
    /// <inheritdoc/>
    public override void ResetForPool()
    {
        base.ResetForPool();
        SequenceId = 0;
        SupplierId = null;
        Status = null;
        PaymentTerms = null;
        ContractStartDate = null;
        ContractEndDate = null;
        Name = string.Empty;
        Email = string.Empty;
        Address = string.Empty;
        TaxCode = string.Empty;
        BankAccount = string.Empty;
        PhoneNumbers = string.Empty;
        Notes = string.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}
