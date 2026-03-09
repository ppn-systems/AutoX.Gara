// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Frontend.ViewModels.Results;

/// <summary>
/// Kết quả đăng nhập, gồm thành công hoặc lỗi kèm reason + advice từ server.
/// </summary>
public sealed class LoginResult
{
    public System.Boolean IsSuccess { get; private init; }
    public System.String? ErrorMessage { get; private init; }

    /// <summary>
    /// Advice từ server: FIX_AND_RETRY, DO_NOT_RETRY, BACKOFF_RETRY...
    /// Null nếu thành công.
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
        ErrorMessage = "Không nhận được phản hồi từ server. Vui lòng thử lại.",
        Advice = ProtocolAdvice.BACKOFF_RETRY
    };
}