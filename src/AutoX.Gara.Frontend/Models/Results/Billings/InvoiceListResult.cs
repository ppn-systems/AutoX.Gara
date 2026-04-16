using System;
using System.Collections.Generic;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Billings;

using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Frontend.Results.Billings;

public sealed class InvoiceListResult

{
    public bool IsSuccess { get; private init; }

    public string? ErrorMessage { get; private init; }

    public ProtocolAdvice Advice { get; private init; }

    public List<InvoiceDto> Invoices { get; private init; } = [];

    public int TotalCount { get; private init; } = -1;

    public bool HasMore { get; private init; }

    public static InvoiceListResult Success(List<InvoiceDto> invoices, int totalCount, bool hasMore)

        => new() { IsSuccess = true, Invoices = invoices, TotalCount = totalCount, HasMore = hasMore };

    public static InvoiceListResult Failure(string message, ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)

        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };

    public static InvoiceListResult Timeout()

        => new()

        {
            IsSuccess = false,

            ErrorMessage = "Y�u c?u h?t th?i gian ch?. Vui l�ng th? l?i.",

            Advice = ProtocolAdvice.BACKOFF_RETRY

        };
}