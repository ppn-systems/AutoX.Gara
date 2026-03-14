// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Employees;
using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Frontend.Results.Employees;

public sealed class EmployeeSalaryWriteResult
{
    public System.Boolean IsSuccess { get; private init; }
    public System.String? ErrorMessage { get; private init; }
    public ProtocolAdvice Advice { get; private init; }
    public EmployeeSalaryDto? Salary { get; private init; }

    public static EmployeeSalaryWriteResult Success(EmployeeSalaryDto? salary = null)
        => new() { IsSuccess = true, Salary = salary };

    public static EmployeeSalaryWriteResult Failure(
        System.String message,
        ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };

    public static EmployeeSalaryWriteResult Timeout()
        => new()
        {
            IsSuccess = false,
            ErrorMessage = "Yêu cầu hết thời gian chờ. Vui lòng thử lại.",
            Advice = ProtocolAdvice.BACKOFF_RETRY
        };
}

