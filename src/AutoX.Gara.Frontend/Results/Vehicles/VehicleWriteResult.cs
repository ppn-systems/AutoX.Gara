// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Vehicles;
using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Frontend.Results.Vehicles;

/// <summary>
/// Represents the result of a single vehicle write operation (create / update / delete).
/// </summary>
public sealed class VehicleWriteResult
{
    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public System.Boolean IsSuccess { get; private init; }

    /// <summary>Gets the error message if the operation failed.</summary>
    public System.String? ErrorMessage { get; private init; }

    /// <summary>Gets the protocol advice for error handling on failure.</summary>
    public ProtocolAdvice Advice { get; private init; }

    /// <summary>
    /// Entity được server echo lại sau khi create/update thành công.
    /// null khi delete hoặc server không echo.
    /// </summary>
    public VehicleDto? UpdatedEntity { get; private init; }

    // ─── Factory Methods ─────────────────────────────────────────────────────

    public static VehicleWriteResult Success(VehicleDto? updatedEntity = null)
        => new() { IsSuccess = true, UpdatedEntity = updatedEntity };

    public static VehicleWriteResult Failure(
        System.String message,
        ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };

    public static VehicleWriteResult Timeout()
        => new()
        {
            IsSuccess = false,
            ErrorMessage = "Yêu cầu hết thời gian chờ. Vui lòng thử lại.",
            Advice = ProtocolAdvice.BACKOFF_RETRY
        };
}