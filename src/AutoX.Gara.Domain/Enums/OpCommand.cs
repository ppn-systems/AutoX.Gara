// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace AutoX.Gara.Domain.Enums;

public enum OpCommand : System.UInt16
{
    NONE = 0x00,

    HANDSHAKE = 0x01,

    LOGIN = 0x50,

    LOGOUT = 0x51,

    REGISTER = 0x52,

    CHANGE_PASSWORD = 0x53,

    CUSTOMER_LIST = 0x100,

    CUSTOMER_CREATE = 0x101,

    CUSTOMER_UPDATE = 0x102,

    CUSTOMER_DELETE = 0x103,
}