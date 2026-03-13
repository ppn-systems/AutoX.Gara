// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace AutoX.Gara.Frontend.Results.Accounts;

/// <summary>
/// Kết quả kết nối + handshake.
/// Dùng static factory methods thay vì constructor để ý nghĩa rõ ràng hơn.
/// </summary>
public sealed class ConnectionResult
{
    public System.Boolean IsSuccess { get; private init; }
    public System.String? ErrorMessage { get; private init; }

    private ConnectionResult() { }

    public static ConnectionResult Success() => new() { IsSuccess = true };

    public static ConnectionResult Failure(System.String message) => new()
    {
        IsSuccess = false,
        ErrorMessage = message
    };
}