// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Billings;
using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Frontend.Results.Billings;

public sealed class InvoiceListResult
{
    public System.Boolean IsSuccess { get; private init; }
    public System.String? ErrorMessage { get; private init; }
    public ProtocolAdvice Advice { get; private init; }
    public System.Collections.Generic.List<InvoiceDto> Invoices { get; private init; } = [];
    public System.Int32 TotalCount { get; private init; } = -1;
    public System.Boolean HasMore { get; private init; }

    public static InvoiceListResult Success(System.Collections.Generic.List<InvoiceDto> invoices, System.Int32 totalCount, System.Boolean hasMore)
        => new() { IsSuccess = true, Invoices = invoices, TotalCount = totalCount, HasMore = hasMore };

    public static InvoiceListResult Failure(System.String message, ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };

    public static InvoiceListResult Timeout()
        => new()
        {
            IsSuccess = false,
            ErrorMessage = "Yêu cầu hết thời gian chờ. Vui lòng thử lại.",
            Advice = ProtocolAdvice.BACKOFF_RETRY
        };
}

