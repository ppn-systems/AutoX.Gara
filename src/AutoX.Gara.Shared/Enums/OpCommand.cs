// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace AutoX.Gara.Shared.Enums;

public enum OpCommand : System.UInt16
{
    /// <summary>
    /// Lệnh không xác định hoặc không có chức năng cụ thể.
    /// </summary>
    NONE = 0x00,

    /// <summary>
    /// Lệnh để thiết lập kết nối ban đầu giữa máy khách và máy chủ.
    /// </summary>
    HANDSHAKE = 0x01,

    /// <summary>
    /// Lệnh để người chơi đăng nhập vào trò chơi.
    /// </summary>
    LOGIN = 0x50,

    /// <summary>
    /// Lệnh để người chơi đăng xuất khỏi trò chơi.
    /// </summary>
    LOGOUT = 0x51,

    /// <summary>
    /// Lệnh để người chơi đăng ký tài khoản mới.
    /// </summary>
    REGISTER = 0x52,

    /// <summary>
    /// Lệnh để người chơi thay đổi mật khẩu tài khoản.
    /// </summary>
    CHANGE_PASSWORD = 0x53,

    CUSTOMER_LIST = 0x100,

    CUSTOMER_CREATE = 0x101,

    CUSTOMER_UPDATE = 0x102,

    CUSTOMER_DELETE = 0x103,
}