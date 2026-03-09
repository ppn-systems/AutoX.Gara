// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Frontend.ViewModels.Results;

/// <summary>
/// Represents the result of a single customer write operation (create/update/delete).
/// </summary>
public sealed class CustomerWriteResult
{
    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public System.Boolean IsSuccess { get; private init; }

    /// <summary>Gets the error message if the operation failed.</summary>
    public System.String? ErrorMessage { get; private init; }

    /// <summary>Gets the protocol advice for error handling.</summary>
    public ProtocolAdvice Advice { get; private init; }

    /// <summary>Creates a successful write result.</summary>
    public static CustomerWriteResult Success()
        => new() { IsSuccess = true };

    /// <summary>Creates a failure write result with the given error message and advice.</summary>
    public static CustomerWriteResult Failure(System.String message, ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };

    /// <summary>Creates a timeout failure write result.</summary>
    public static CustomerWriteResult Timeout()
        => new() { IsSuccess = false, ErrorMessage = "Yêu cầu hết thời gian chờ. Vui lòng thử lại.", Advice = ProtocolAdvice.BACKOFF_RETRY };
}