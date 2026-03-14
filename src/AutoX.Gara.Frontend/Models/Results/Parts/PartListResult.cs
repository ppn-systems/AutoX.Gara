// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Inventory;
using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Frontend.Results.Parts;

/// <summary>
/// Result of a part list query operation.
/// </summary>
public sealed class PartListResult
{
    /// <summary>
    /// Indicates success or failure.
    /// </summary>
    public System.Boolean IsSuccess { get; private init; }

    /// <summary>
    /// Error message if failed.
    /// </summary>
    public System.String? ErrorMessage { get; private init; }

    /// <summary>
    /// Protocol advice for error handling.
    /// </summary>
    public ProtocolAdvice Advice { get; private init; }

    /// <summary>
    /// List of parts returned.
    /// </summary>
    public System.Collections.Generic.List<PartDto> Parts { get; private init; } = [];

    /// <summary>
    /// Total count of parts matching filter.
    /// </summary>
    public System.Int32 TotalCount { get; private init; } = -1;

    /// <summary>
    /// Indicates if there are more pages available.
    /// </summary>
    public System.Boolean HasMore { get; private init; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static PartListResult Success(
        System.Collections.Generic.List<PartDto> parts,
        System.Int32 totalCount = -1,
        System.Boolean hasMore = false)
        => new() { IsSuccess = true, Parts = parts, TotalCount = totalCount, HasMore = hasMore };

    /// <summary>
    /// Creates a failure result.
    /// </summary>
    public static PartListResult Failure(
        System.String message,
        ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };

    /// <summary>
    /// Creates a timeout result.
    /// </summary>
    public static PartListResult Timeout()
        => new()
        {
            IsSuccess = false,
            ErrorMessage = "Yêu cầu hết thời gian chờ. Vui lòng thử lại.",
            Advice = ProtocolAdvice.BACKOFF_RETRY
        };
}
