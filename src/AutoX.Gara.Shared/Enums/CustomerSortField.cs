// Copyright (c) 2026 PPN Corporation. All rights reserved.
namespace AutoX.Gara.Shared.Enums;

/// <summary>
/// C�c c?t được ph�p s?p x?p trong truy v?n danh s�ch kh�ch h�ng.
/// D�ng trong <see cref="CustomersQueryPacket.SortBy"/>.
/// </summary>
public enum CustomerSortField : byte
{
    /// <summary>S?p x?p theo ng�y t?o (m?c d?nh).</summary>
    CreatedAt = 0,

    /// <summary>S?p x?p theo t�n kh�ch h�ng (A�Z ho?c Z�A).</summary>
    Name = 1,

    /// <summary>S?p x?p theo d?a ch? email.</summary>
    Email = 2,

    /// <summary>S?p x?p theo ng�y c?p nh?t g?n nh?t.</summary>
    UpdatedAt = 3,
}
