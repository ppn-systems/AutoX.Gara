// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Parts;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Extensions;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using System;
namespace AutoX.Gara.Contracts.Inventory;
/// <summary>
/// Packet mang dữ liệu m?t ph? t�ng (<c>Part</c>),
/// d�ng cho c�c thao t�c create, update, v� query response.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class PartDto : PacketBase<PartDto>
{
    // --- Fixed-size fields ----------------------------------------------------
    /// <summary>Id ph? t�ng. Null khi t?o m?i.</summary>
    [SerializeOrder(0)]
    public int? PartId { get; set; }
    /// <summary>Id nh� cung c?p.</summary>
    [SerializeOrder(1)]
    public int SupplierId { get; set; }
    /// <summary>Lo?i ph? t�ng.</summary>
    [SerializeOrder(2)]
    public PartCategory? PartCategory { get; set; }
    /// <summary>S? lu?ng trong kho.</summary>
    [SerializeOrder(3)]
    public int InventoryQuantity { get; set; }
    /// <summary>Gi� nh?p.</summary>
    [SerializeOrder(4)]
    public decimal PurchasePrice { get; set; }
    /// <summary>Gi� b�n.</summary>
    [SerializeOrder(5)]
    public decimal SellingPrice { get; set; }
    /// <summary>Ph? t�ng c� b? l?i kh�ng.</summary>
    [SerializeOrder(6)]
    public bool IsDefective { get; set; }
    /// <summary>�� ng?ng b�n chua.</summary>
    [SerializeOrder(7)]
    public bool IsDiscontinued { get; set; }
    /// <summary>Ng�y nh?p kho.</summary>
    [SerializeOrder(8)]
    public DateOnly DateAdded { get; set; }
    /// <summary>Ng�y h?t h?n (nullable).</summary>
    [SerializeOrder(9)]
    public DateOnly? ExpiryDate { get; set; }
    // --- Dynamic-size fields --------------------------------------------------
    /// <summary>M� SKU/PartCode (t?i da 12 k� t?, ch? ch? v� s?).</summary>
    [SerializeOrder(10)]
    public string PartCode { get; set; }
    /// <summary>T�n ph? t�ng.</summary>
    [SerializeOrder(11)]
    public string PartName { get; set; }
    /// <summary>Nh� s?n xu?t.</summary>
    [SerializeOrder(12)]
    public string Manufacturer { get; set; }
    // --- Constructor ---------------------------------------------------------
    /// <summary>Kh?i t?o v?i gi� tr? m?c d?nh.</summary>
    public PartDto()
    {
        PartCode = string.Empty;
        PartName = string.Empty;
        Manufacturer = string.Empty;
        DateAdded = DateOnly.FromDateTime(DateTime.UtcNow);
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
        DateAdded = DateOnly.FromDateTime(DateTime.UtcNow);
        ExpiryDate = null;
        PartCode = string.Empty;
        PartName = string.Empty;
        Manufacturer = string.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
    // --- Transformer ---------------------------------------------------------
    /// <inheritdoc/>
    public static PartDto Compress(PartDto packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        return packet; // Packet d? nh?, kh�ng c?n compress
    }
    /// <inheritdoc/>
    public static PartDto Decompress(PartDto packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        return packet;
    }
}



