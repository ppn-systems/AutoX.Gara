// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Parts;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;

namespace AutoX.Gara.Shared.Protocol.Inventory;

/// <summary>
/// Packet mang d? li?u m?t ph? tùng (<c>Part</c>),
/// dùng cho các thao tác create, update, và query response.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class PartDto : PacketBase<PartDto>
{
    // --- Fixed-size fields ----------------------------------------------------

    /// <summary>Id ph? tùng. Null khi t?o m?i.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public System.Int32? PartId { get; set; }

    /// <summary>Id nhà cung c?p.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public System.Int32 SupplierId { get; set; }

    /// <summary>Lo?i ph? tùng.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public PartCategory? PartCategory { get; set; }

    /// <summary>S? lu?ng trong kho.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public System.Int32 InventoryQuantity { get; set; }

    /// <summary>Giá nh?p.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public System.Decimal PurchasePrice { get; set; }

    /// <summary>Giá bán.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public System.Decimal SellingPrice { get; set; }

    /// <summary>Ph? tùng có b? l?i không.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public System.Boolean IsDefective { get; set; }

    /// <summary>Ðã ng?ng bán chua.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 8)]
    public System.Boolean IsDiscontinued { get; set; }

    /// <summary>Ngày nh?p kho.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 9)]
    public System.DateOnly DateAdded { get; set; }

    /// <summary>Ngày h?t h?n (nullable).</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 10)]
    public System.DateOnly? ExpiryDate { get; set; }

    // --- Dynamic-size fields --------------------------------------------------

    /// <summary>Mã SKU/PartCode (t?i da 12 ký t?, ch? ch? và s?).</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 11)]
    public System.String PartCode { get; set; }

    /// <summary>Tên ph? tùng.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 12)]
    public System.String PartName { get; set; }

    /// <summary>Nhà s?n xu?t.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 13)]
    public System.String Manufacturer { get; set; }

    // --- Constructor ---------------------------------------------------------

    /// <summary>Kh?i t?o v?i giá tr? m?c d?nh.</summary>
    public PartDto()
    {
        PartCode = System.String.Empty;
        PartName = System.String.Empty;
        Manufacturer = System.String.Empty;
        DateAdded = System.DateOnly.FromDateTime(System.DateTime.UtcNow);
        OpCode = OpCommand.NONE.AsUInt16();
    }

    // --- Pool Reset -----------------------------------------------------------

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

    // --- Transformer ---------------------------------------------------------

    /// <inheritdoc/>
    public static PartDto Compress(PartDto packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        return packet; // Packet d? nh?, không c?n compress
    }

    /// <inheritdoc/>
    public static PartDto Decompress(PartDto packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        return packet;
    }
}