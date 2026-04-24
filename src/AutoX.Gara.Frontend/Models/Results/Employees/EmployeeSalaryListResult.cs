// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Contracts.Protocol.Employees;
using Nalix.Common.Networking.Protocols;
using System.Collections.Generic;
namespace AutoX.Gara.Frontend.Models.Results.Employees;
public sealed class EmployeeSalaryListResult
{
    public bool IsSuccess { get; private init; }
    public string? ErrorMessage { get; private init; }
    public ProtocolAdvice Advice { get; private init; }
    public List<EmployeeSalaryDto> Salaries { get; private init; } = [];
    public int TotalCount { get; private init; } = -1;
    public bool HasMore { get; private init; }
    public static EmployeeSalaryListResult Success(
        List<EmployeeSalaryDto> salaries,
        int totalCount = -1,
        bool hasMore = false)
        => new() { IsSuccess = true, Salaries = salaries, TotalCount = totalCount, HasMore = hasMore };
    public static EmployeeSalaryListResult Failure(
        string message,
        ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };
    public static EmployeeSalaryListResult Timeout()
        => new()
        {
            IsSuccess = false,
            ErrorMessage = "Y�u c?u h?t th?i gian ch?. Vui l�ng th? l?i.",
            Advice = ProtocolAdvice.BACKOFF_RETRY
        };
}

