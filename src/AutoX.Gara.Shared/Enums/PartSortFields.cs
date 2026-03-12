// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace AutoX.Gara.Domain.Enums.Parts;

/// <summary>
/// Các cột có thể dùng để sắp xếp kết quả truy vấn <c>SparePart</c>.
/// </summary>
public enum SparePartSortField : System.Byte
{
    /// <summary>Sắp xếp theo tên phụ tùng.</summary>
    PartName = 0,

    /// <summary>Sắp xếp theo giá nhập.</summary>
    PurchasePrice = 1,

    /// <summary>Sắp xếp theo giá bán.</summary>
    SellingPrice = 2,

    /// <summary>Sắp xếp theo số lượng tồn kho.</summary>
    InventoryQuantity = 3,
}

/// <summary>
/// Các cột có thể dùng để sắp xếp kết quả truy vấn <c>ReplacementPart</c>.
/// </summary>
public enum ReplacementPartSortField : System.Byte
{
    /// <summary>Sắp xếp theo tên phụ tùng.</summary>
    PartName = 0,

    /// <summary>Sắp xếp theo ngày nhập kho.</summary>
    DateAdded = 1,

    /// <summary>Sắp xếp theo ngày hết hạn.</summary>
    ExpiryDate = 2,

    /// <summary>Sắp xếp theo số lượng.</summary>
    Quantity = 3,

    /// <summary>Sắp xếp theo đơn giá.</summary>
    UnitPrice = 4,
}
