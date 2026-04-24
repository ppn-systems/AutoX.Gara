// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Shared.Protocol.Employees;
using Nalix.Common.Networking.Protocols;
namespace AutoX.Gara.Frontend.Models.Results.Employees;
public sealed class EmployeeSalaryWriteResult
{
    public bool IsSuccess { get; private init; }
    public string? ErrorMessage { get; private init; }
    public ProtocolAdvice Advice { get; private init; }
    public EmployeeSalaryDto? Salary { get; private init; }
    public static EmployeeSalaryWriteResult Success(EmployeeSalaryDto? salary = null)
        => new() { IsSuccess = true, Salary = salary };
    public static EmployeeSalaryWriteResult Failure(
        string message,
        ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };
    public static EmployeeSalaryWriteResult Timeout()
        => new()
        {
            IsSuccess = false,
            ErrorMessage = "Y�u c?u h?t th?i gian ch?. Vui l�ng th? l?i.",
            Advice = ProtocolAdvice.BACKOFF_RETRY
        };
}
