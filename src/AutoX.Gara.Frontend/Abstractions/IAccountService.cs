// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Results.Accounts;
using System.Threading;

namespace AutoX.Gara.Frontend.Abstractions;

/// <summary>
/// Abstraction cho toàn b? lu?ng login: connect ? handshake ? authenticate.
/// Tách kh?i ViewModel d? d? test và thay th?.
/// </summary>
public interface IAccountService
{
    /// <summary>K?t n?i và th?c hi?n handshake v?i server.</summary>
    System.Threading.Tasks.Task<ConnectionResult> ConnectAsync(CancellationToken ct = default);

    /// <summary>G?i thông tin dang nh?p và tr? v? k?t qu? xác th?c.</summary>
    System.Threading.Tasks.Task<LoginResult> AuthenticateAsync(System.String username, System.String password, CancellationToken ct = default);
}
