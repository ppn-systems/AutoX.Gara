// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Models;
using System.Threading.Tasks;

namespace AutoX.Gara.Application.Abstractions.Services;

/// <summary>
/// Định nghĩa các nghiệp vụ cốt lõi về tài khoản.
/// Tách biệt hoàn toàn khỏi Logic Giao thức (Nalix).
/// </summary>
public interface IAccountAppService
{
    /// <summary>
    /// Xử lý đăng nhập và xác thực thông tin người dùng.
    /// </summary>
    Task<ServiceResult<AuthData>> AuthenticateAsync(string username, string password);

    /// <summary>
    /// Xử lý đăng ký tài khoản mới.
    /// </summary>
    Task<ServiceResult<AuthData>> RegisterAsync(string username, string password);
}
