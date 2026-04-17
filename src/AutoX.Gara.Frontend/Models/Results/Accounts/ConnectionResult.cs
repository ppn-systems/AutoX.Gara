using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace AutoX.Gara.Frontend.Results.Accounts;

/// <summary>

/// K?t qu? k?t n?i + handshake.

/// D�ng static factory methods thay v� constructor d? � nghia r� r�ng hon.

/// </summary>

public sealed class ConnectionResult

{
    public bool IsSuccess { get; private init; }

    public string? ErrorMessage { get; private init; }

    private ConnectionResult() { }

    public static ConnectionResult Success() => new() { IsSuccess = true };

    public static ConnectionResult Failure(string message) => new()

    {
        IsSuccess = false,

        ErrorMessage = message

    };
}
