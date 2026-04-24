// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Shared.Protocol.Employees;
using Nalix.Common.Networking.Protocols;
using System.Collections.Generic;
namespace AutoX.Gara.Frontend.Models.Results.Employees;
/// <summary>
/// Result of an employee list query operation.
/// </summary>
public sealed class EmployeeListResult
{
    public bool IsSuccess { get; private init; }
    public string? ErrorMessage { get; private init; }
    public ProtocolAdvice Advice { get; private init; }
    public List<EmployeeDto> Employees { get; private init; } = [];
    public int TotalCount { get; private init; } = -1;
    public bool HasMore { get; private init; }
    public static EmployeeListResult Success(
        List<EmployeeDto> employees,
        int totalCount = -1,
        bool hasMore = false)
        => new() { IsSuccess = true, Employees = employees, TotalCount = totalCount, HasMore = hasMore };
    public static EmployeeListResult Failure(
        string message,
        ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };
    public static EmployeeListResult Timeout()
        => new()
        {
            IsSuccess = false,
            ErrorMessage = "Y�u c?u h?t th?i gian ch?. Vui l�ng th? l?i.",
            Advice = ProtocolAdvice.BACKOFF_RETRY
        };
}
