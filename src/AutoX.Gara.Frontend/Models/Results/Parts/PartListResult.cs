// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Inventory;
using Nalix.Common.Networking.Protocols;
using System.Collections.Generic;

namespace AutoX.Gara.Frontend.Models.Results.Parts;

/// <summary>

/// Result of a part list query operation.

/// </summary>

public sealed class PartListResult

{
    /// <summary>

    /// Indicates success or failure.

    /// </summary>

    public bool IsSuccess { get; private init; }

    /// <summary>

    /// Error message if failed.

    /// </summary>

    public string? ErrorMessage { get; private init; }

    /// <summary>

    /// Protocol advice for error handling.

    /// </summary>

    public ProtocolAdvice Advice { get; private init; }

    /// <summary>

    /// List of parts returned.

    /// </summary>

    public List<PartDto> Parts { get; private init; } = [];

    /// <summary>

    /// Total count of parts matching filter.

    /// </summary>

    public int TotalCount { get; private init; } = -1;

    /// <summary>

    /// Indicates if there are more pages available.

    /// </summary>

    public bool HasMore { get; private init; }

    /// <summary>

    /// Creates a successful result.

    /// </summary>

    public static PartListResult Success(

        List<PartDto> parts,

        int totalCount = -1,

        bool hasMore = false)

        => new() { IsSuccess = true, Parts = parts, TotalCount = totalCount, HasMore = hasMore };

    /// <summary>

    /// Creates a failure result.

    /// </summary>

    public static PartListResult Failure(

        string message,

        ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)

        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };

    /// <summary>

    /// Creates a timeout result.

    /// </summary>

    public static PartListResult Timeout()

        => new()

        {
            IsSuccess = false,

            ErrorMessage = "Y�u c?u h?t th?i gian ch?. Vui l�ng th? l?i.",

            Advice = ProtocolAdvice.BACKOFF_RETRY

        };
}
