// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Employees;
using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Frontend.Models.Results.Employees;

/// <summary>
/// Result of an employee list query operation.
/// </summary>
public sealed class EmployeeListResult
{
    public System.Boolean IsSuccess { get; private init; }
    public System.String? ErrorMessage { get; private init; }
    public ProtocolAdvice Advice { get; private init; }
    public System.Collections.Generic.List<EmployeeDto> Employees { get; private init; } = [];
    public System.Int32 TotalCount { get; private init; } = -1;
    public System.Boolean HasMore { get; private init; }

    public static EmployeeListResult Success(
        System.Collections.Generic.List<EmployeeDto> employees,
        System.Int32 totalCount = -1,
        System.Boolean hasMore = false)
        => new() { IsSuccess = true, Employees = employees, TotalCount = totalCount, HasMore = hasMore };

    public static EmployeeListResult Failure(
        System.String message,
        ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };

    public static EmployeeListResult Timeout()
        => new()
        {
            IsSuccess = false,
            ErrorMessage = "Yêu cầu hết thời gian chờ. Vui lòng thử lại.",
            Advice = ProtocolAdvice.BACKOFF_RETRY
        };
}
