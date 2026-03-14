// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Billings;
using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Frontend.Results.ServiceItems;

public sealed class ServiceItemListResult
{
    public System.Boolean IsSuccess { get; private init; }
    public System.String? ErrorMessage { get; private init; }
    public ProtocolAdvice Advice { get; private init; }
    public System.Collections.Generic.List<ServiceItemDto> ServiceItems { get; private init; } = [];
    public System.Int32 TotalCount { get; private init; } = -1;
    public System.Boolean HasMore { get; private init; }

    public static ServiceItemListResult Success(
        System.Collections.Generic.List<ServiceItemDto> serviceItems,
        System.Int32 totalCount = -1,
        System.Boolean hasMore = false)
        => new()
        {
            IsSuccess = true,
            ServiceItems = serviceItems,
            TotalCount = totalCount,
            HasMore = hasMore
        };

    public static ServiceItemListResult Failure(System.String message, ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };

    public static ServiceItemListResult Timeout()
        => new()
        {
            IsSuccess = false,
            ErrorMessage = "Yêu cầu hết thời gian chờ. Vui lòng thử lại.",
            Advice = ProtocolAdvice.BACKOFF_RETRY
        };
}

