// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Inventory;
using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Frontend.ViewModels.Results;

/// <summary>
/// Kết quả của thao tác truy vấn danh sách phụ tùng kho (<c>ReplacementPart</c>).
/// </summary>
public sealed class ReplacementPartListResult
{
    public System.Boolean IsSuccess { get; private init; }
    public System.String? ErrorMessage { get; private init; }
    public ProtocolAdvice Advice { get; private init; }
    public System.Collections.Generic.List<ReplacementPartDto> Parts { get; private init; } = [];
    public System.Int32 TotalCount { get; private init; } = -1;
    public System.Boolean HasMore { get; private init; }

    public static ReplacementPartListResult Success(
        System.Collections.Generic.List<ReplacementPartDto> parts,
        System.Int32 totalCount = -1,
        System.Boolean hasMore = false)
        => new() { IsSuccess = true, Parts = parts, TotalCount = totalCount, HasMore = hasMore };

    public static ReplacementPartListResult Failure(
        System.String message,
        ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };

    public static ReplacementPartListResult Timeout()
        => new()
        {
            IsSuccess = false,
            ErrorMessage = "Yêu cầu hết thời gian chờ. Vui lòng thử lại.",
            Advice = ProtocolAdvice.BACKOFF_RETRY
        };
}

/// <summary>
/// Kết quả của thao tác ghi (create / update / delete) cho <c>ReplacementPart</c>.
/// </summary>
public sealed class ReplacementPartWriteResult
{
    public System.Boolean IsSuccess { get; private init; }
    public System.String? ErrorMessage { get; private init; }
    public ProtocolAdvice Advice { get; private init; }

    /// <summary>Entity được server xác nhận sau create/update. Null khi delete.</summary>
    public ReplacementPartDto? UpdatedEntity { get; private init; }

    public static ReplacementPartWriteResult Success(ReplacementPartDto? updatedEntity = null)
        => new() { IsSuccess = true, UpdatedEntity = updatedEntity };

    public static ReplacementPartWriteResult Failure(
        System.String message,
        ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };

    public static ReplacementPartWriteResult Timeout()
        => new()
        {
            IsSuccess = false,
            ErrorMessage = "Yêu cầu hết thời gian chờ. Vui lòng thử lại.",
            Advice = ProtocolAdvice.BACKOFF_RETRY
        };
}
