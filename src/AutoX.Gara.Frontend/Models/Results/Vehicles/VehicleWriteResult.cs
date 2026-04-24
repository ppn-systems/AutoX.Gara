// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Shared.Protocol.Vehicles;
using Nalix.Common.Networking.Protocols;
namespace AutoX.Gara.Frontend.Models.Results.Vehicles;
/// <summary>
/// Represents the result of a single vehicle write operation (create / update / delete).
/// </summary>
public sealed class VehicleWriteResult
{
    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; private init; }
    /// <summary>Gets the error message if the operation failed.</summary>
    public string? ErrorMessage { get; private init; }
    /// <summary>Gets the protocol advice for error handling on failure.</summary>
    public ProtocolAdvice Advice { get; private init; }
    /// <summary>
    /// Entity được server echo l?i sau khi create/update th�nh c�ng.
    /// null khi delete ho?c server kh�ng echo.
    /// </summary>
    public VehicleDto? UpdatedEntity { get; private init; }
    // --- Factory Methods -----------------------------------------------------
    public static VehicleWriteResult Success(VehicleDto? updatedEntity = null)
        => new() { IsSuccess = true, UpdatedEntity = updatedEntity };
    public static VehicleWriteResult Failure(
        string message,
        ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };
    public static VehicleWriteResult Timeout()
        => new()
        {
            IsSuccess = false,
            ErrorMessage = "Y�u c?u h?t th?i gian ch?. Vui l�ng th? l?i.",
            Advice = ProtocolAdvice.BACKOFF_RETRY
        };
}
