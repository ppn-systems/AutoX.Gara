// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Suppliers;
using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Frontend.ViewModels.Results;

/// <summary>
/// Represents the result of a supplier list query operation.
/// </summary>
public sealed class SupplierListResult
{
    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public System.Boolean IsSuccess { get; private init; }

    /// <summary>Gets the error message if the operation failed.</summary>
    public System.String? ErrorMessage { get; private init; }

    /// <summary>Gets the protocol advice for error handling.</summary>
    public ProtocolAdvice Advice { get; private init; }

    /// <summary>Gets the list of supplier data packets returned from the server.</summary>
    public System.Collections.Generic.List<SupplierDto> Suppliers { get; private init; } = [];

    /// <summary>
    /// Total number of suppliers matching the current filter/search on the server.
    /// Used to calculate total pages accurately. -1 means unknown (server did not report it).
    /// </summary>
    public System.Int32 TotalCount { get; private init; } = -1;

    /// <summary>
    /// True when the server has more pages beyond this one.
    /// Lightweight hint used when <see cref="TotalCount"/> is unavailable.
    /// </summary>
    public System.Boolean HasMore { get; private init; }

    // ─── Factory Methods ─────────────────────────────────────────────────────

    /// <summary>Creates a successful result with the given supplier list.</summary>
    public static SupplierListResult Success(
        System.Collections.Generic.List<SupplierDto> suppliers,
        System.Int32 totalCount = -1,
        System.Boolean hasMore = false)
        => new()
        {
            IsSuccess = true,
            Suppliers = suppliers,
            TotalCount = totalCount,
            HasMore = hasMore
        };

    /// <summary>Creates a failure result with the given error message and advice.</summary>
    public static SupplierListResult Failure(
        System.String message,
        ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };

    /// <summary>Creates a timeout failure result.</summary>
    public static SupplierListResult Timeout()
        => new()
        {
            IsSuccess = false,
            ErrorMessage = "Yêu cầu hết thời gian chờ. Vui lòng thử lại.",
            Advice = ProtocolAdvice.BACKOFF_RETRY
        };
}

/// <summary>
/// Represents the result of a single supplier write operation (create / update / change status).
/// </summary>
public sealed class SupplierWriteResult
{
    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public System.Boolean IsSuccess { get; private init; }

    /// <summary>Gets the error message if the operation failed.</summary>
    public System.String? ErrorMessage { get; private init; }

    /// <summary>Gets the protocol advice for error handling on failure.</summary>
    public ProtocolAdvice Advice { get; private init; }

    /// <summary>
    /// The server-confirmed entity returned after a successful create or update.
    /// Enables optimistic UI updates without a full list reload.
    /// <c>null</c> on change-status or when the server does not echo back the entity.
    /// </summary>
    public SupplierDto? UpdatedEntity { get; private init; }

    // ─── Factory Methods ─────────────────────────────────────────────────────

    /// <summary>Creates a successful write result, optionally carrying the server-confirmed entity.</summary>
    public static SupplierWriteResult Success(SupplierDto? updatedEntity = null)
        => new() { IsSuccess = true, UpdatedEntity = updatedEntity };

    /// <summary>Creates a failure write result with the given error message and advice.</summary>
    public static SupplierWriteResult Failure(
        System.String message,
        ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };

    /// <summary>Creates a timeout failure write result.</summary>
    public static SupplierWriteResult Timeout()
        => new()
        {
            IsSuccess = false,
            ErrorMessage = "Yêu cầu hết thời gian chờ. Vui lòng thử lại.",
            Advice = ProtocolAdvice.BACKOFF_RETRY
        };
}
