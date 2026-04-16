using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Frontend.Models.Results.Accounts;

/// <summary>

/// K?t qu? dang nh?p, g?m th�nh c�ng ho?c l?i k�m reason + advice t? server.

/// </summary>

public sealed class LoginResult

{
    public bool IsSuccess { get; private init; }

    public string? ErrorMessage { get; private init; }

    /// <summary>

    /// Advice t? server: FIX_AND_RETRY, DO_NOT_RETRY, BACKOFF_RETRY...

    /// Null n?u th�nh c�ng.

    /// </summary>

    public ProtocolAdvice? Advice { get; private init; }

    private LoginResult() { }

    public static LoginResult Success() => new() { IsSuccess = true };

    public static LoginResult Failure(string message, ProtocolAdvice advice) => new()

    {
        IsSuccess = false,

        ErrorMessage = message,

        Advice = advice

    };

    public static LoginResult Timeout() => new()

    {
        IsSuccess = false,

        ErrorMessage = "Kh�ng nh�n du?c ph?n h?i t? server. Vui l�ng th? l?i.",

        Advice = ProtocolAdvice.BACKOFF_RETRY

    };
}