// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Models.Results.Accounts;
using System.Threading;
using System.Threading.Tasks;

namespace AutoX.Gara.Frontend.Abstractions;

public interface IAccountService
{
    Task<ConnectionResult> ConnectAsync(CancellationToken ct = default);

    Task<LoginResult> AuthenticateAsync(string username, string password, CancellationToken ct = default);

    Task<LoginResult> RegisterAsync(string username, string password, CancellationToken ct = default);
}
