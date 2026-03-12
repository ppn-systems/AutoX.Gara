// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Shared.Frames;

namespace AutoX.Gara.Shared.Protocol.Inventory;

/// <summary>
/// Packet mang dữ liệu một phụ tùng kho (<c>ReplacementPart</c>),
/// dùng cho các thao tác create, update, và query response.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class ReplacementPartDto : PacketBase<ReplacementPartDto>, IPacketTransformer<ReplacementPartDto>, IPacketSequenced
{
    // ─── Fixed-size fields ────────────────────────────────────────────────────

    /// <inheritdoc/>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION)]
    public System.UInt32 SequenceId { get; set; }

    /// <summary>Id phụ tùng kho. Null khi tạo mới.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32? PartId { get; set; }

    /// <summary>Số lượng trong kho.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public System.Int32 Quantity { get; set; }

    /// <summary>Đơn giá.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public System.Decimal UnitPrice { get; set; }

    /// <summary>Phụ tùng có bị lỗi không.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public System.Boolean IsDefective { get; set; }

    /// <summary>Ngày nhập kho.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 5)]
    public System.DateOnly DateAdded { get; set; }

    /// <summary>Ngày hết hạn (nullable).</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 6)]
    public System.DateOnly? ExpiryDate { get; set; }

    // ─── Dynamic-size fields ──────────────────────────────────────────────────

    /// <summary>Mã SKU/PartCode (tối đa 12 ký tự, chỉ chữ và số).</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 7)]
    public System.String PartCode { get; set; }

    /// <summary>Tên phụ tùng.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 8)]
    public System.String PartName { get; set; }

    /// <summary>Nhà sản xuất.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 9)]
    public System.String Manufacturer { get; set; }

    // ─── Constructor ─────────────────────────────────────────────────────────

    /// <summary>Khởi tạo với giá trị mặc định.</summary>
    public ReplacementPartDto()
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
        Quantity = 0;
        UnitPrice = 0;
        IsDefective = false;
        DateAdded = System.DateOnly.FromDateTime(System.DateTime.UtcNow);
        ExpiryDate = null;
        PartCode = System.String.Empty;
        PartName = System.String.Empty;
        Manufacturer = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }

    // ─── Transformer ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public static ReplacementPartDto Compress(ReplacementPartDto packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        return packet; // Packet đủ nhỏ, không cần compress
    }

    /// <inheritdoc/>
    public static ReplacementPartDto Decompress(ReplacementPartDto packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        return packet;
    }
}
