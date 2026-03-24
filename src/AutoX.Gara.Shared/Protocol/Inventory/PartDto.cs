// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Parts;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Shared.Frames;

namespace AutoX.Gara.Shared.Protocol.Inventory;

/// <summary>
/// Packet mang dữ liệu một phụ tùng (<c>Part</c>),
/// dùng cho các thao tác create, update, và query response.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class PartDto : PacketBase<PartDto>
{
    // ─── Fixed-size fields ────────────────────────────────────────────────────

    /// <summary>Id phụ tùng. Null khi tạo mới.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32? PartId { get; set; }

    /// <summary>Id nhà cung cấp.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public System.Int32 SupplierId { get; set; }

    /// <summary>Loại phụ tùng.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public PartCategory? PartCategory { get; set; }

    /// <summary>Số lượng trong kho.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public System.Int32 InventoryQuantity { get; set; }

    /// <summary>Giá nhập.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 5)]
    public System.Decimal PurchasePrice { get; set; }

    /// <summary>Giá bán.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 6)]
    public System.Decimal SellingPrice { get; set; }

    /// <summary>Phụ tùng có bị lỗi không.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 7)]
    public System.Boolean IsDefective { get; set; }

    /// <summary>Đã ngừng bán chưa.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 8)]
    public System.Boolean IsDiscontinued { get; set; }

    /// <summary>Ngày nhập kho.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 9)]
    public System.DateOnly DateAdded { get; set; }

    /// <summary>Ngày hết hạn (nullable).</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 10)]
    public System.DateOnly? ExpiryDate { get; set; }

    // ─── Dynamic-size fields ──────────────────────────────────────────────────

    /// <summary>Mã SKU/PartCode (tối đa 12 ký tự, chỉ chữ và số).</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 11)]
    public System.String PartCode { get; set; }

    /// <summary>Tên phụ tùng.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 12)]
    public System.String PartName { get; set; }

    /// <summary>Nhà sản xuất.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 13)]
    public System.String Manufacturer { get; set; }

    // ─── Constructor ─────────────────────────────────────────────────────────

    /// <summary>Khởi tạo với giá trị mặc định.</summary>
    public PartDto()
    {
        PartCode = System.String.Empty;
        PartName = System.String.Empty;
        Manufacturer = System.String.Empty;
        DateAdded = System.DateOnly.FromDateTime(System.DateTime.UtcNow);
        OpCode = OpCommand.NONE.AsUInt16();
    }

    // ─── Pool Reset ───────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void ResetForPool()
    {
        base.ResetForPool();

        SequenceId = 0;
        PartId = null;
        SupplierId = 0;
        PartCategory = null;
        InventoryQuantity = 0;
        PurchasePrice = 0;
        SellingPrice = 0;
        IsDefective = false;
        IsDiscontinued = false;
        DateAdded = System.DateOnly.FromDateTime(System.DateTime.UtcNow);
        ExpiryDate = null;
        PartCode = System.String.Empty;
        PartName = System.String.Empty;
        Manufacturer = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }

    // ─── Transformer ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public static PartDto Compress(PartDto packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        return packet; // Packet đủ nhỏ, không cần compress
    }

    /// <inheritdoc/>
    public static PartDto Decompress(PartDto packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        return packet;
    }
}