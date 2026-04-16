using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;

namespace AutoX.Gara.Shared.Protocol.Auth;

/// <summary>
/// ViewModel d�nh cho dang nh?p c?a ngu?i d�ng h? th?ng.
/// Ch? ch?a th�ng tin t?i thi?u (username v� password) m� client g?i l�n server.
/// Kh�ng luu b?t k? th�ng tin b?o m?t nh?y c?m n�o ngo�i t�i kho?n v� m?t kh?u d?ng clear text (ch? d? x�c th?c m?t l?n).
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public class LoginRequestModel : PacketBase<LoginRequestModel>
{
    /// <summary>
    /// T�n dang nh?p c?a ngu?i d�ng.
    /// </summary>
    [SerializeOrder(1)]
    public string Username { get; set; }

    /// <summary>
    /// M?t kh?u nh?p v�o t? ngu?i d�ng (clear text, ch? s? d?ng d? x�c th?c, kh�ng luu tr?).
    /// </summary>
    [SerializeOrder(2)]
    public string Password { get; set; }
}