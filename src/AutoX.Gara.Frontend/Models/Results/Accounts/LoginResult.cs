// Copyright (c) 2026 PPN Corporation. All rights reserved.
using Nalix.Common.Networking.Protocols;
namespace AutoX.Gara.Frontend.Models.Results.Accounts;
/// <summary>
/// Kết quả đăng nhập, gồm thành công hoặc lỗi kèm reason + advice từ server.
/// </summary>
public sealed class LoginResult
{
    public bool IsSuccess { get; private init; }
    public string? ErrorMessage { get; private init; }
    /// <summary>
    /// Advice từ server: FIX_AND_RETRY, DO_NOT_RETRY, BACKOFF_RETRY...
    /// Null nếu thành công.
    /// </summary>
    public ProtocolAdvice? Advice { get; private init; }
    private LoginResult()
    {
    }
    public static LoginResult Success() => new() { IsSuccess = true };
    public static LoginResult Failure(string message, ProtocolAdvice advice) => new()
    {
        IsSuccess = false,
        ErrorMessage = message,
        Advice = advice
    };
    public static LoginResult Timeout(string? message = null) => new()
    {
        IsSuccess = false,
        ErrorMessage = string.IsNullOrWhiteSpace(message) ? "Request timeout." : message,
        Advice = ProtocolAdvice.BACKOFF_RETRY
    };
}
