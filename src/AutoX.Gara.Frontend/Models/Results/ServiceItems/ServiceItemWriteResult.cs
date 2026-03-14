// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Billings;
using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Frontend.Results.ServiceItems;

/// <summary>
/// Result of a service item write (create/update/delete) operation.
/// </summary>
public sealed class ServiceItemWriteResult
{
    public System.Boolean IsSuccess { get; private init; }
    public System.String? ErrorMessage { get; private init; }
    public ProtocolAdvice Advice { get; private init; }
    public ServiceItemDto? UpdatedEntity { get; private init; }

    public static ServiceItemWriteResult Success(ServiceItemDto? updatedEntity = null)
        => new() { IsSuccess = true, UpdatedEntity = updatedEntity };

    public static ServiceItemWriteResult Failure(
        System.String message,
        ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };

    public static ServiceItemWriteResult Timeout()
        => new()
        {
            IsSuccess = false,
            ErrorMessage = "Yêu cầu hết thời gian chờ. Vui lòng thử lại.",
            Advice = ProtocolAdvice.BACKOFF_RETRY
        };
}
