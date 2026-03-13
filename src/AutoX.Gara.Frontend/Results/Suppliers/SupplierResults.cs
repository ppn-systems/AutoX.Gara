// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Suppliers;
using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Frontend.Results.Suppliers;

/// <summary>
/// Result of a supplier list query operation.
/// </summary>
public sealed class SupplierListResult
{
    public System.Boolean IsSuccess { get; private init; }
    public System.String? ErrorMessage { get; private init; }
    public ProtocolAdvice Advice { get; private init; }
    public System.Collections.Generic.List<SupplierDto> Suppliers { get; private init; } = [];
    public System.Int32 TotalCount { get; private init; } = -1;
    public System.Boolean HasMore { get; private init; }

    public static SupplierListResult Success(
        System.Collections.Generic.List<SupplierDto> suppliers,
        System.Int32 totalCount = -1,
        System.Boolean hasMore = false)
        => new() { IsSuccess = true, Suppliers = suppliers, TotalCount = totalCount, HasMore = hasMore };

    public static SupplierListResult Failure(
        System.String message,
        ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };

    public static SupplierListResult Timeout()
        => new()
        {
            IsSuccess = false,
            ErrorMessage = "Yêu cầu hết thời gian chờ. Vui lòng thử lại.",
            Advice = ProtocolAdvice.BACKOFF_RETRY
        };
}