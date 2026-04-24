// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Contracts.Protocol.Invoices;
using Nalix.Common.Networking.Protocols;
using System.Collections.Generic;
namespace AutoX.Gara.Frontend.Models.Results.Billings;
public sealed class TransactionListResult
{
    public bool IsSuccess { get; private init; }
    public string? ErrorMessage { get; private init; }
    public ProtocolAdvice Advice { get; private init; }
    public List<TransactionDto> Transactions { get; private init; } = [];
    public int TotalCount { get; private init; } = -1;
    public bool HasMore { get; private init; }
    public static TransactionListResult Success(List<TransactionDto> transactions, int totalCount, bool hasMore)
        => new() { IsSuccess = true, Transactions = transactions, TotalCount = totalCount, HasMore = hasMore };
    public static TransactionListResult Failure(string message, ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };
    public static TransactionListResult Timeout()
        => new()
        {
            IsSuccess = false,
            ErrorMessage = "Y�u c?u h?t th?i gian ch?. Vui l�ng th? l?i.",
            Advice = ProtocolAdvice.BACKOFF_RETRY
        };
}

