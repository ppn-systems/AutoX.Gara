// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Extensions;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using System;
namespace AutoX.Gara.Contracts.Suppliers;
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
    [SerializeOrder(0)]
    public int? SupplierId { get; set; }
    /// <summary>Tr?ng thái nhà cung c?p.</summary>
    [SerializeOrder(1)]
    public SupplierStatus? Status { get; set; }
    /// <summary>Ði?u kho?n thanh toán.</summary>
    [SerializeOrder(2)]
    public PaymentTerms? PaymentTerms { get; set; }
    /// <summary>Ngày b?t d?u h?p tác.</summary>
    [SerializeOrder(3)]
    public DateTime? ContractStartDate { get; set; }
    /// <summary>Ngày k?t thúc h?p tác (n?u có).</summary>
    [SerializeOrder(4)]
    public DateTime? ContractEndDate { get; set; }
    // --- Dynamic-size fields --------------------------------------------------
    /// <summary>Tên nhà cung c?p.</summary>
    [SerializeOrder(5)]
    public string Name { get; set; }
    /// <summary>Email nhà cung c?p.</summary>
    [SerializeOrder(6)]
    public string Email { get; set; }
    /// <summary>Ð?a ch? nhà cung c?p.</summary>
    [SerializeOrder(7)]
    public string Address { get; set; }
    /// <summary>Mã s? thu?.</summary>
    [SerializeOrder(8)]
    public string TaxCode { get; set; }
    /// <summary>Tài kho?n ngân hàng.</summary>
    [SerializeOrder(9)]
    public string BankAccount { get; set; }
    /// <summary>
    /// Danh sách SÐT liên h?, phân cách b?ng d?u ph?y.
    /// VD: "0901234567,0912345678"
    /// </summary>
    [SerializeOrder(10)]
    public string PhoneNumbers { get; set; }
    /// <summary>Ghi chú n?i b?. T?i da 500 ký t?.</summary>
    [SerializeOrder(11)]
    public string Notes { get; set; }
    [SerializeOrder(12)]
    public string PhoneNumber { get; set; } = string.Empty;
    [SerializeOrder(13)]
    public string ContactPerson { get; set; } = string.Empty;
    [SerializeOrder(14)]
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
        PhoneNumber = string.Empty;
        ContactPerson = string.Empty;
        IsActive = true;
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
        PhoneNumber = string.Empty;
        ContactPerson = string.Empty;
        IsActive = true;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}



