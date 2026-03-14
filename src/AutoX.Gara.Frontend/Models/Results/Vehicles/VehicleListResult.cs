// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Vehicles;
using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Frontend.Results.Vehicles;

/// <summary>
/// Represents the result of a vehicle list query operation (l?y danh sách xe theo customer).
/// </summary>
public sealed class VehicleListResult
{
    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public System.Boolean IsSuccess { get; private init; }

    /// <summary>Gets the error message if the operation failed.</summary>
    public System.String? ErrorMessage { get; private init; }

    /// <summary>Gets the protocol advice for error handling.</summary>
    public ProtocolAdvice Advice { get; private init; }

    /// <summary>Gets the list of vehicle data packets returned from the server.</summary>
    public System.Collections.Generic.List<VehicleDto> Vehicles { get; private init; } = [];

    /// <summary>
    /// T?ng s? xe c?a customer trên server (dùng d? tính t?ng trang).
    /// -1 nghia là server không tr? v?.
    /// </summary>
    public System.Int32 TotalCount { get; private init; } = -1;

    /// <summary>Server còn page ti?p theo không (khi TotalCount không có).</summary>
    public System.Boolean HasMore { get; private init; }

    // --- Factory Methods -----------------------------------------------------

    public static VehicleListResult Success(
        System.Collections.Generic.List<VehicleDto> vehicles,
        System.Int32 totalCount = -1,
        System.Boolean hasMore = false)
        => new()
        {
            IsSuccess = true,
            Vehicles = vehicles,
            TotalCount = totalCount,
            HasMore = hasMore
        };

    public static VehicleListResult Failure(
        System.String message,
        ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };

    public static VehicleListResult Timeout()
        => new()
        {
            IsSuccess = false,
            ErrorMessage = "Yêu c?u h?t th?i gian ch?. Vui lòng th? l?i.",
            Advice = ProtocolAdvice.BACKOFF_RETRY
        };
}
