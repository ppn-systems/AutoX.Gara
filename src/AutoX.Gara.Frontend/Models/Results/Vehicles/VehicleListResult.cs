using System;
using System.Collections.Generic;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Vehicles;

using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Frontend.Results.Vehicles;

/// <summary>

/// Represents the result of a vehicle list query operation (l?y danh s�ch xe theo customer).

/// </summary>

public sealed class VehicleListResult

{
    /// <summary>Gets a value indicating whether the operation succeeded.</summary>

    public bool IsSuccess { get; private init; }

    /// <summary>Gets the error message if the operation failed.</summary>

    public string? ErrorMessage { get; private init; }

    /// <summary>Gets the protocol advice for error handling.</summary>

    public ProtocolAdvice Advice { get; private init; }

    /// <summary>Gets the list of vehicle data packets returned from the server.</summary>

    public List<VehicleDto> Vehicles { get; private init; } = [];

    /// <summary>

    /// T?ng s? xe c?a customer tr�n server (d�ng d? t�nh t?ng trang).

    /// -1 nghia l� server kh�ng tr? v?.

    /// </summary>

    public int TotalCount { get; private init; } = -1;

    /// <summary>Server c�n page ti?p theo kh�ng (khi TotalCount kh�ng c�).</summary>

    public bool HasMore { get; private init; }

    // --- Factory Methods -----------------------------------------------------

    public static VehicleListResult Success(

        List<VehicleDto> vehicles,

        int totalCount = -1,

        bool hasMore = false)

        => new()

        {
            IsSuccess = true,

            Vehicles = vehicles,

            TotalCount = totalCount,

            HasMore = hasMore

        };

    public static VehicleListResult Failure(

        string message,

        ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)

        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };

    public static VehicleListResult Timeout()

        => new()

        {
            IsSuccess = false,

            ErrorMessage = "Y�u c?u h?t th?i gian ch?. Vui l�ng th? l?i.",

            Advice = ProtocolAdvice.BACKOFF_RETRY

        };
}
