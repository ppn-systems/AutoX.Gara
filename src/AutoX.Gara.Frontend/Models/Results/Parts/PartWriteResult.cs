using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Inventory;

using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Frontend.Results.Parts;

/// <summary>

/// Result of a part write (create/update/delete) operation.

/// </summary>

public sealed class PartWriteResult

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

    /// Updated entity returned from server (null for delete/discontinue).

    /// </summary>

    public PartDto? UpdatedEntity { get; private init; }

    /// <summary>

    /// Creates a successful result.

    /// </summary>

    public static PartWriteResult Success(PartDto? updatedEntity = null)

        => new() { IsSuccess = true, UpdatedEntity = updatedEntity };

    /// <summary>

    /// Creates a failure result.

    /// </summary>

    public static PartWriteResult Failure(

        string message,

        ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)

        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };

    /// <summary>

    /// Creates a timeout result.

    /// </summary>

    public static PartWriteResult Timeout()

        => new()

        {
            IsSuccess = false,

            ErrorMessage = "Y�u c?u h?t th?i gian ch?. Vui l�ng th? l?i.",

            Advice = ProtocolAdvice.BACKOFF_RETRY

        };
}
