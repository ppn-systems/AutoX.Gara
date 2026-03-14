// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Frontend.Results.Accounts;

/// <summary>
/// K?t qu? dang nh?p, g?m thành công ho?c l?i kèm reason + advice t? server.
/// </summary>
public sealed class LoginResult
{
    public System.Boolean IsSuccess { get; private init; }
    public System.String? ErrorMessage { get; private init; }

    /// <summary>
    /// Advice t? server: FIX_AND_RETRY, DO_NOT_RETRY, BACKOFF_RETRY...
    /// Null n?u thành công.
    /// </summary>
    public ProtocolAdvice? Advice { get; private init; }

    private LoginResult() { }

    public static LoginResult Success() => new() { IsSuccess = true };

    public static LoginResult Failure(System.String message, ProtocolAdvice advice) => new()
    {
        IsSuccess = false,
        ErrorMessage = message,
        Advice = advice
    };

    public static LoginResult Timeout() => new()
    {
        IsSuccess = false,
        ErrorMessage = "Không nh?n du?c ph?n h?i t? server. Vui lòng th? l?i.",
        Advice = ProtocolAdvice.BACKOFF_RETRY
    };
}
