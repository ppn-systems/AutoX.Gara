// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace AutoX.Gara.Shared.Enums;

/// <summary>
/// C�c c?t c� th? d�ng d? s?p x?p k?t qu? truy v?n <c>ReplacementPart</c>.
/// </summary>
public enum PartSortField : byte
{
    /// <summary>S?p x?p theo t�n ph? t�ng.</summary>
    PartName = 0,

    /// <summary>S?p x?p theo ng�y nh?p kho.</summary>
    DateAdded = 1,

    /// <summary>S?p x?p theo ng�y h?t h?n.</summary>
    ExpiryDate = 2,

    /// <summary>S?p x?p theo s? lu?ng.</summary>
    Quantity = 3,

    /// <summary>S?p x?p theo don gi�.</summary>
    UnitPrice = 4,



    /// <summary>S?p x?p theo gi� nh?p.</summary>
    PurchasePrice = 5,

    /// <summary>S?p x?p theo gi� b�n.</summary>
    SellingPrice = 6,

    /// <summary>S?p x?p theo s? lu?ng t?n kho.</summary>
    InventoryQuantity = 7,



    /// <summary>

    /// Gi� tr? t?i da (kh�ng ph?i c?t th?c t?, d�ng d? validate input).

    /// </summary>

    TotalValue = 8,
}
