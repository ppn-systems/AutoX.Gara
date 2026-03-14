// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Invoices;
using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Frontend.Results.Billings;

public sealed class RepairOrderListResult
{
    public System.Boolean IsSuccess { get; private init; }
    public System.String? ErrorMessage { get; private init; }
    public ProtocolAdvice Advice { get; private init; }

    public System.Collections.Generic.List<RepairOrderDto> RepairOrders { get; private init; } = [];

    public System.Int32 TotalCount { get; private init; } = -1;
    public System.Boolean HasMore { get; private init; }

    public static RepairOrderListResult Success(
        System.Collections.Generic.List<RepairOrderDto> repairOrders,
        System.Int32 totalCount = -1,
        System.Boolean hasMore = false)
        => new()
        {
            IsSuccess = true,
            RepairOrders = repairOrders,
            TotalCount = totalCount,
            HasMore = hasMore
        };

    public static RepairOrderListResult Failure(
        System.String message,
        ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };

    public static RepairOrderListResult Timeout()
        => new()
        {
            IsSuccess = false,
            ErrorMessage = "Yêu cầu hết thời gian chờ. Vui lòng thử lại.",
            Advice = ProtocolAdvice.BACKOFF_RETRY
        };
}

