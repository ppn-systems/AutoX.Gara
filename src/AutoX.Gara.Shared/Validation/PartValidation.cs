using System;

namespace AutoX.Gara.Shared.Validation;

/// <summary>
/// Quy tắc kiểm tra tính hợp lệ của Phụ tùng/Linh kiện.
/// </summary>
public static class PartValidation
{
    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name) && name.Length >= 2 && name.Length <= 200;

    public static bool IsValidPrice(decimal purchasePrice, decimal sellingPrice)
    {
        return purchasePrice >= 0 && sellingPrice >= purchasePrice;
    }

    public static bool IsValidQuantity(int quantity) => quantity >= 0;
}
