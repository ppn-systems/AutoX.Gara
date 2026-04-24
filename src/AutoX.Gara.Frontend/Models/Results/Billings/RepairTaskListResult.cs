// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Contracts.Protocol.Repairs;
using Nalix.Common.Networking.Protocols;
using System.Collections.Generic;
namespace AutoX.Gara.Frontend.Models.Results.Billings;
public sealed class RepairTaskListResult
{
    public bool IsSuccess { get; private init; }
    public string? ErrorMessage { get; private init; }
    public ProtocolAdvice Advice { get; private init; }
    public List<RepairTaskDto> RepairTasks { get; private init; } = [];
    public int TotalCount { get; private init; } = -1;
    public bool HasMore { get; private init; }
    public static RepairTaskListResult Success(
        List<RepairTaskDto> repairTasks,
        int totalCount = -1,
        bool hasMore = false)
        => new()
        {
            IsSuccess = true,
            RepairTasks = repairTasks,
            TotalCount = totalCount,
            HasMore = hasMore
        };
    public static RepairTaskListResult Failure(string message, ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };
    public static RepairTaskListResult Timeout()
        => new()
        {
            IsSuccess = false,
            ErrorMessage = "Y�u c?u h?t th?i gian ch?. Vui l�ng th? l?i.",
            Advice = ProtocolAdvice.BACKOFF_RETRY
        };
}

