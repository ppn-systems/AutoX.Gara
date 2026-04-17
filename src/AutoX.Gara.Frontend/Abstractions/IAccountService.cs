using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Models.Results.Accounts;
using AutoX.Gara.Frontend.Results.Accounts;
using System.Threading;

namespace AutoX.Gara.Frontend.Abstractions;

/// <summary>
/// Abstraction cho to�n b? lu?ng login: connect + handshake + authenticate.
/// T�ch kh?i ViewModel d? d? test v� thay th?.
/// </summary>
public interface IAccountService
{
    /// <summary>K?t n?i v� th?c hi?n handshake v?i server.</summary>
    System.Threading.Tasks.Task<ConnectionResult> ConnectAsync(CancellationToken ct = default);

    /// <summary>G?i th�ng tin đăng nhập v� tr? v? k?t qu? x�c th?c.</summary>
    System.Threading.Tasks.Task<LoginResult> AuthenticateAsync(string username, string password, CancellationToken ct = default);
}
