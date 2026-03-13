// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Results.Accounts;
using System.Threading;

namespace AutoX.Gara.Frontend.Abstractions;

/// <summary>
/// Abstraction cho toàn bộ luồng login: connect → handshake → authenticate.
/// Tách khỏi ViewModel để dễ test và thay thế.
/// </summary>
public interface IAccountService
{
    /// <summary>Kết nối và thực hiện handshake với server.</summary>
    System.Threading.Tasks.Task<ConnectionResult> ConnectAsync(CancellationToken ct = default);

    /// <summary>Gửi thông tin đăng nhập và trả về kết quả xác thực.</summary>
    System.Threading.Tasks.Task<LoginResult> AuthenticateAsync(System.String username, System.String password, CancellationToken ct = default);
}