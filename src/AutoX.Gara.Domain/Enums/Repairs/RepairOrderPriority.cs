using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace AutoX.Gara.Domain.Enums.Repairs;

/// <summary>
/// Mức độ ưu tiên của lệnh sửa chữa.
/// </summary>
public enum RepairOrderPriority
{
    None = 0,
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4
}
