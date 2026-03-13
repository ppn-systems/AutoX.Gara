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

    INVOICE_GET = 0x1A0,

    INVOICE_CREATE = 0x1A1,

    INVOICE_UPDATE = 0x1A2,

    INVOICE_DELETE = 0x1A3,

    REPAIR_ORDER_GET = 0x1B0,

    REPAIR_ORDER_CREATE = 0x1B1,

    REPAIR_ORDER_UPDATE = 0x1B2,

    REPAIR_ORDER_DELETE = 0x1B3,

    TRANSACTION_GET = 0x1C0,

    TRANSACTION_CREATE = 0x1C1,

    TRANSACTION_UPDATE = 0x1C2,

    TRANSACTION_DELETE = 0x1C3,

    /// <summary>
    /// Get list of employees.
    /// </summary>
    EMPLOYEE_GET = 6001,

    /// <summary>
    /// Create a new employee.
    /// </summary>
    EMPLOYEE_CREATE = 6002,

    /// <summary>
    /// Update an existing employee.
    /// </summary>
    EMPLOYEE_UPDATE = 6003,

    /// <summary>
    /// Change employee status.
    /// </summary>
    EMPLOYEE_CHANGE_STATUS = 6004,
}
