using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace AutoX.Gara.Shared.Enums;

public enum TransactionSortField : byte
{
    TransactionDate = 0,
    Amount = 1,
    Status = 2,
    Type = 3,
    PaymentMethod = 4,
}
