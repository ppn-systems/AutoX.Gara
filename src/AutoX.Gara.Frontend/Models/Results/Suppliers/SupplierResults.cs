// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Suppliers;
using Nalix.Common.Networking.Protocols;
using System.Collections.Generic;

namespace AutoX.Gara.Frontend.Models.Results.Suppliers;

/// <summary>

/// Result of a supplier list query operation.

/// </summary>

public sealed class SupplierListResult

{
    public bool IsSuccess { get; private init; }

    public string? ErrorMessage { get; private init; }

    public ProtocolAdvice Advice { get; private init; }

    public List<SupplierDto> Suppliers { get; private init; } = [];

    public int TotalCount { get; private init; } = -1;

    public bool HasMore { get; private init; }

    public static SupplierListResult Success(

        List<SupplierDto> suppliers,

        int totalCount = -1,

        bool hasMore = false)

        => new() { IsSuccess = true, Suppliers = suppliers, TotalCount = totalCount, HasMore = hasMore };

    public static SupplierListResult Failure(

        string message,

        ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)

        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };

    public static SupplierListResult Timeout()

        => new()

        {
            IsSuccess = false,

            ErrorMessage = "Y�u c?u h?t th?i gian ch?. Vui l�ng th? l?i.",

            Advice = ProtocolAdvice.BACKOFF_RETRY

        };
}
