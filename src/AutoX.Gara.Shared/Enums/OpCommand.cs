// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace AutoX.Gara.Shared.Enums;

public enum OpCommand : System.UInt16
{
    NONE = 0x00,

    HANDSHAKE = 0x01,

    LOGIN = 0x50,

    LOGOUT = 0x51,

    REGISTER = 0x52,

    CHANGE_PASSWORD = 0x53,

    CUSTOMER_GET = 0x100,

    CUSTOMER_CREATE = 0x101,

    CUSTOMER_UPDATE = 0x102,

    CUSTOMER_DELETE = 0x103,

    VEHICLE_GET = 0x150,

    VEHICLE_CREATE = 0x151,

    VEHICLE_UPDATE = 0x152,

    VEHICLE_DELETE = 0x153,

    SUPPLIER_GET = 0x160,

    SUPPLIER_CREATE = 0x161,

    SUPPLIER_UPDATE = 0x162,

    SUPPLIER_DELETE = 0x163,

    SUPPLIER_CHANGE_STATUS = 0x164,

    PART_GET = 0x180,

    PART_CREATE = 0x181,

    PART_UPDATE = 0x182,

    PART_DELETE = 0x183,
}