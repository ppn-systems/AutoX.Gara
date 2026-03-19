// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Shared.Frames;

namespace AutoX.Gara.Shared.Protocol.Suppliers;

/// <summary>
/// Packet mang dữ liệu nhà cung cấp, dùng cho các thao tác Create, Update và Query.
/// <para>
/// Fixed-size fields (enum, int, DateTime) đặt TRƯỚC dynamic string fields
/// để <see cref="PacketBase{TSelf}.Length"/> tính đúng wire-size.
/// </para>
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class SupplierDto : PacketBase<SupplierDto>
{
    // ─── Fixed-size fields ────────────────────────────────────────────────────

    /// <summary>
    /// ID nhà cung cấp. <c>null</c> khi tạo mới.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32? SupplierId { get; set; }

    /// <summary>Trạng thái nhà cung cấp.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public SupplierStatus? Status { get; set; }

    /// <summary>Điều khoản thanh toán.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public PaymentTerms? PaymentTerms { get; set; }

    /// <summary>Ngày bắt đầu hợp tác.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public System.DateTime? ContractStartDate { get; set; }

    /// <summary>Ngày kết thúc hợp tác (nếu có).</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 5)]
    public System.DateTime? ContractEndDate { get; set; }

    // ─── Dynamic-size fields ──────────────────────────────────────────────────

    /// <summary>Tên nhà cung cấp.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 6)]
    public System.String Name { get; set; }

    /// <summary>Email nhà cung cấp.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 7)]
    public System.String Email { get; set; }

    /// <summary>Địa chỉ nhà cung cấp.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 8)]
    public System.String Address { get; set; }

    /// <summary>Mã số thuế.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 9)]
    public System.String TaxCode { get; set; }

    /// <summary>Tài khoản ngân hàng.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 10)]
    public System.String BankAccount { get; set; }

    /// <summary>
    /// Danh sách SĐT liên hệ, phân cách bằng dấu phẩy.
    /// VD: "0901234567,0912345678"
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 11)]
    public System.String PhoneNumbers { get; set; }

    /// <summary>Ghi chú nội bộ. Tối đa 500 ký tự.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 12)]
    public System.String Notes { get; set; }

    // ─── Constructor ──────────────────────────────────────────────────────────

    /// <summary>Khởi tạo <see cref="SupplierDto"/> với giá trị mặc định rỗng.</summary>
    public SupplierDto()
    {
        Name = System.String.Empty;
        Email = System.String.Empty;
        Address = System.String.Empty;
        TaxCode = System.String.Empty;
        BankAccount = System.String.Empty;
        PhoneNumbers = System.String.Empty;
        Notes = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }

    // ─── Pool Reset ───────────────────────────────────────────────────────────

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
        Name = System.String.Empty;
        Email = System.String.Empty;
        Address = System.String.Empty;
        TaxCode = System.String.Empty;
        BankAccount = System.String.Empty;
        PhoneNumbers = System.String.Empty;
        Notes = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}