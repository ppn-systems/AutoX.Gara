// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Parts;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Shared.Frames;

namespace AutoX.Gara.Shared.Protocol.Inventory;

/// <summary>
/// Packet mang dữ liệu một phụ tùng bán (<c>SparePart</c>),
/// dùng cho các thao tác create, update, và query response.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class SparePartDto : PacketBase<SparePartDto>, IPacketTransformer<SparePartDto>, IPacketSequenced
{
    // ─── Fixed-size fields ────────────────────────────────────────────────────

    /// <inheritdoc/>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION)]
    public System.UInt32 SequenceId { get; set; }

    /// <summary>Id phụ tùng. Null khi tạo mới.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32? SparePartId { get; set; }

    /// <summary>Id nhà cung cấp.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public System.Int32 SupplierId { get; set; }

    /// <summary>Loại phụ tùng.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public PartCategory? PartCategory { get; set; }

    /// <summary>Giá nhập.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public System.Decimal PurchasePrice { get; set; }

    /// <summary>Giá bán.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 5)]
    public System.Decimal SellingPrice { get; set; }

    /// <summary>Số lượng tồn kho.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 6)]
    public System.Int32 InventoryQuantity { get; set; }

    /// <summary>Đã ngừng bán chưa.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 7)]
    public System.Boolean IsDiscontinued { get; set; }

    // ─── Dynamic-size fields ──────────────────────────────────────────────────

    /// <summary>Tên phụ tùng.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 8)]
    public System.String PartName { get; set; }

    // ─── Constructor ─────────────────────────────────────────────────────────

    /// <summary>Khởi tạo với giá trị mặc định.</summary>
    public SparePartDto()
    {
        PartName = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }

    // ─── Pool Reset ───────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void ResetForPool()
    {
        base.ResetForPool();

        SequenceId = 0;
        SparePartId = null;
        SupplierId = 0;
        PartCategory = null;
        PurchasePrice = 0;
        SellingPrice = 0;
        InventoryQuantity = 0;
        IsDiscontinued = false;
        PartName = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }

    // ─── Transformer (no-op — không cần compress cho packet nhỏ này) ──────────

    /// <inheritdoc/>
    public static SparePartDto Compress(SparePartDto packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        return packet; // Packet đủ nhỏ, không cần compress
    }

    /// <inheritdoc/>
    public static SparePartDto Decompress(SparePartDto packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        return packet;
    }
}
