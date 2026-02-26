// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Common.Messaging.Packets;
using Nalix.Common.Serialization;

namespace AutoX.Gara.Shared.Models;

/// <summary>
/// ViewModel dành cho đăng nhập của người dùng hệ thống.
/// Chỉ chứa thông tin tối thiểu (username và password) mà client gửi lên server.
/// Không lưu bất kỳ thông tin bảo mật nhạy cảm nào ngoài tài khoản và mật khẩu dạng clear text (chỉ để xác thực một lần).
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public class AccountModel
{
    /// <summary>
    /// Tính tổng số byte thực tế (runtime) khi serialize Username và Password hiện tại (UTF-8).
    /// Dùng khi cần kiểm soát size thực tiễn.
    /// </summary>
    [SerializeIgnore]
    public System.UInt16 Length
    {
        get
        {
            System.Int32 usernameLen = System.String.IsNullOrEmpty(Username) ? 0 : System.Text.Encoding.UTF8.GetByteCount(Username);
            System.Int32 passwordLen = System.String.IsNullOrEmpty(Password) ? 0 : System.Text.Encoding.UTF8.GetByteCount(Password);
            return (System.UInt16)(usernameLen + passwordLen);
        }
    }

    /// <summary>
    /// Tên đăng nhập của người dùng.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION)]
    public System.String Username { get; set; }

    /// <summary>
    /// Mật khẩu nhập vào từ người dùng (clear text, chỉ sử dụng để xác thực, không lưu trữ).
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.String Password { get; set; }
}