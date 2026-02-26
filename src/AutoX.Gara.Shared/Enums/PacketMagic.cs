// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace AutoX.Gara.Shared.Enums;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Roslynator", "RCS1154:Sort enum members", Justification = "<Pending>")]
public enum PacketMagic : System.UInt32
{
    ACCOUNT = 0x58494C41,

    CHANGE_PASSWORD = 0x47414D45,

    CUSTOMER = 0x52455355, // "USER" - bạn có thể đổi thành "CUST" nếu muốn

    CUSTOMER_LIST = 0x54534552, // "REST" - bạn có thể đổi thành "CLST" nếu muốn

    CUSTOMER_LIST_REQUEST = 0x54534553, // "REST" - bạn có thể đổi thành "CREQ" nếu muốn
}