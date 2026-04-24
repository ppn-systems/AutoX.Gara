// Copyright (c) 2026 PPN Corporation. All rights reserved.
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
namespace AutoX.Gara.Shared.Protocol.Auth;
/// <summary>
/// ViewModel d�nh cho đăng nhập c?a ngu?i d�ng hệ thống.
/// Chỉ chứa th�ng tin tối thiểu (username v� password) m� client g?i l�n server.
/// Kh�ng luu bất kỳ th�ng tin bảo mật nhạy cảm n�o ngo�i t�i kho?n v� mật khẩu dạng clear text (chỉ để x�c th?c một lần).
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public class LoginRequestModel : PacketBase<LoginRequestModel>
{
    /// <summary>
    /// T�n đăng nhập c?a ngu?i d�ng.
    /// </summary>
    [SerializeOrder(1)]
    public string Username { get; set; }
    /// <summary>
    /// Mật khẩu nh?p v�o t? ngu?i d�ng (clear text, ch? sử dụng d? x�c th?c, kh�ng lưu trữ).
    /// </summary>
    [SerializeOrder(2)]
    public string Password { get; set; }
}
